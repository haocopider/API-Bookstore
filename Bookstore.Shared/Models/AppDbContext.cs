using Bookstore.Shared.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Bookstore.Shared.Models;

public partial class AppDbContext : DbContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor httpContextAccessor)
            : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // 1. Chuẩn bị danh sách lưu log
        var auditEntries = OnBeforeSaveChanges();

        // 2. Lưu thay đổi thực tế vào DB
        var result = await base.SaveChangesAsync(cancellationToken);

        // 3. Xử lý các entity có khóa chính tự tăng (Id)
        if (auditEntries.Any(a => a.HasTemporaryProperties))
        {
            await OnAfterSaveChangesAsync(auditEntries);
            await base.SaveChangesAsync(cancellationToken); // Lưu thêm lần nữa cho log có Id mới
        }

        return result;
    }

    private List<AuditEntry> OnBeforeSaveChanges()
    {
        ChangeTracker.DetectChanges();
        var auditEntries = new List<AuditEntry>();

        // Lấy Id của người dùng từ Token (Admin)
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int? adminId = int.TryParse(userIdClaim, out var id) ? id : null;

        foreach (var entry in ChangeTracker.Entries())
        {
            // Bỏ qua các thực thể không thay đổi, không theo dõi, hoặc chính là bảng AuditLog
            if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var auditEntry = new AuditEntry(entry)
            {
                TableName = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name,
                AdminId = adminId
            };
            auditEntries.Add(auditEntry);

            foreach (var property in entry.Properties)
            {
                if (property.IsTemporary)
                {
                    // Chờ lưu xong để lấy Id tự tăng (cho hành động Thêm mới)
                    auditEntry.TemporaryProperties.Add(property);
                    continue;
                }

                string propertyName = property.Metadata.Name;
                if (property.Metadata.IsPrimaryKey())
                {
                    auditEntry.KeyValues[propertyName] = property.CurrentValue;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        auditEntry.ActionType = "CREATE";
                        auditEntry.NewValues[propertyName] = property.CurrentValue;
                        break;

                    case EntityState.Deleted:
                        auditEntry.ActionType = "DELETE";
                        auditEntry.OldValues[propertyName] = property.OriginalValue;
                        break;

                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            auditEntry.ActionType = "UPDATE";
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                        }
                        break;
                }
            }
        }

        // Với các entity không có property tạm (như Update, Delete), ta có thể convert sang AuditLog và Add luôn
        foreach (var auditEntry in auditEntries.Where(_ => !_.HasTemporaryProperties))
        {
            AuditLogs.Add(auditEntry.ToAuditLog());
        }

        return auditEntries;
    }

    private Task OnAfterSaveChangesAsync(List<AuditEntry> auditEntries)
    {
        if (auditEntries == null || auditEntries.Count == 0)
            return Task.CompletedTask;

        foreach (var auditEntry in auditEntries)
        {
            if (auditEntry.HasTemporaryProperties)
            {
                // Sau khi base.SaveChangesAsync chạy, các property tạm (như Id) đã có giá trị thực
                foreach (var prop in auditEntry.TemporaryProperties)
                {
                    if (prop.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                    else
                    {
                        auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                }
                AuditLogs.Add(auditEntry.ToAuditLog());
            }
        }
        return Task.CompletedTask;
    }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Author> Authors { get; set; }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<BookFormat> BookFormats { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Conversation> Conversations { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Promotion> Promotions { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RolePermission> RolePermissions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Admin__3214EC072655DEC2");

            entity.ToTable("Admin");

            entity.HasIndex(e => e.UserName, "UQ_Users_UserName").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Admin__A9D10534D75D74C8").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Passcode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Password).HasMaxLength(500);
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.RefreshToken).HasMaxLength(500);
            entity.Property(e => e.UserName).HasMaxLength(100);

            entity.HasOne(d => d.Role).WithMany(p => p.Admins)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Admin_Roles");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AuditLog__3214EC07FB18734C");

            entity.Property(e => e.ActionType).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.RecordId).HasMaxLength(100);
            entity.Property(e => e.TableName).HasMaxLength(100);

            entity.HasOne(d => d.Admin).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.AdminId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_AuditLogs_Admin");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Notifica__3214EC0767BAACA8");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.Order).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_Notifications_Orders");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notifications_Users");
        });

        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Authors__3214EC07BEC64C40");

            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(255);
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Books__3214EC0765577572");

            entity.Property(e => e.CoverImageUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())", "DF__Books__CreatedAt__3D5E1FD2")
                .HasColumnType("datetime");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false, "DF__Books__IsDeleted__3E52440B");
            entity.Property(e => e.PublishDate).HasColumnType("datetime");
            entity.Property(e => e.Publisher).HasMaxLength(200);
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.Author).WithMany(p => p.Books)
                .HasForeignKey(d => d.AuthorId)
                .HasConstraintName("FK__Books__AuthorId__3C69FB99");

            entity.HasMany(d => d.Categories).WithMany(p => p.Books)
                .UsingEntity<Dictionary<string, object>>(
                    "BookCategory",
                    r => r.HasOne<Category>().WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Book_Cate__Categ__4222D4EF"),
                    l => l.HasOne<Book>().WithMany()
                        .HasForeignKey("BookId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_Book_Categories_Books"),
                    j =>
                    {
                        j.HasKey("BookId", "CategoryId").HasName("PK__Book_Cat__9C7051A7FA64003F");
                        j.ToTable("Book_Categories");
                    });
        });

        modelBuilder.Entity<BookFormat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BookForm__3214EC07523AC448");

            entity.HasIndex(e => e.Sku, "UQ__BookForm__CA1ECF0D45DE155F").IsUnique();

            entity.Property(e => e.DigitalLink).HasMaxLength(500);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Rating).HasDefaultValue(0.0, "DF_BookFormats_Rating");
            entity.Property(e => e.Sku)
                .HasMaxLength(50)
                .HasColumnName("SKU");
            entity.Property(e => e.Stock).HasDefaultValue(0, "DF__BookForma__Stock__46E78A0C");

            entity.HasOne(d => d.Book).WithMany(p => p.BookFormats)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BookForma__BookI__45F365D3");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Categori__3214EC07D9D3E395");

            entity.HasIndex(e => e.Slug, "UQ__Categori__BC7B5FB6C1042F66").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Slug).HasMaxLength(100);
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Conversa__3214EC0738ADC020");

            entity.HasIndex(e => e.Id, "IX_Conversations");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())", "DF__Conversat__Creat__6EF57B66")
                .HasColumnType("datetime");
            entity.Property(e => e.Status).HasDefaultValue(0, "DF__Conversat__Statu__6E01572D");

            entity.HasOne(d => d.Customer).WithMany(p => p.Conversations)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK_Conversations_Customer");

            entity.HasOne(d => d.Staff).WithMany(p => p.Conversations)
                .HasForeignKey(d => d.StaffId)
                .HasConstraintName("FK_Conversations_Admin");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Messages__3214EC07329E14A6");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())", "DF__Messages__Create__72C60C4A")
                .HasColumnType("datetime");
            entity.Property(e => e.MessageType).HasMaxLength(20);

            entity.HasOne(d => d.Conversation).WithMany(p => p.Messages)
                .HasForeignKey(d => d.ConversationId)
                .HasConstraintName("FK__Messages__Conver__71D1E811");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Orders__3214EC07713B9556");

            entity.HasIndex(e => e.OrderCode, "UQ_OrderCode").IsUnique();

            entity.Property(e => e.FinalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.OrderCode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())", "DF__Orders__OrderDat__66603565")
                .HasColumnType("datetime");
            entity.Property(e => e.PointIsUsed).HasDefaultValue(0, "DF__Orders__PointsUs__6754599E");
            entity.Property(e => e.ShippingAddress).HasMaxLength(500);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TrackingNumber).HasMaxLength(100);

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Orders__UserId__656C112C");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrderIte__3214EC07807D2E34");

            entity.Property(e => e.ItemType).HasMaxLength(20);
            entity.Property(e => e.PriceAtPurchase).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SnapshotBookTitle).HasMaxLength(250);
            entity.Property(e => e.SnapshotUnitPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Item).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ItemId)
                .HasConstraintName("FK_OrderItems_Books");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK__OrderItem__Order__6A30C649");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Permissi__3214EC079F676ECE");

            entity.HasIndex(e => e.Code, "UQ__Permissi__A25C5AA7A23AA913").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(255);
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Promotio__3214EC071DDFC585");

            entity.Property(e => e.DiscountValue).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF__Promotion__IsAct__5AEE82B9");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.StartDate).HasColumnType("datetime");

            entity.HasMany(d => d.Books).WithMany(p => p.Promotions)
                .UsingEntity<Dictionary<string, object>>(
                    "PromotionBook",
                    r => r.HasOne<Book>().WithMany()
                        .HasForeignKey("BookId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Promotion__BookI__5EBF139D"),
                    l => l.HasOne<Promotion>().WithMany()
                        .HasForeignKey("PromotionId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Promotion__Promo__5DCAEF64"),
                    j =>
                    {
                        j.HasKey("PromotionId", "BookId").HasName("PK__Promotio__311A23EF168A00FF");
                        j.ToTable("Promotion_Books");
                    });

            entity.HasMany(d => d.Categories).WithMany(p => p.Promotions)
                .UsingEntity<Dictionary<string, object>>(
                    "PromotionCategory",
                    r => r.HasOne<Category>().WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Promotion__Categ__628FA481"),
                    l => l.HasOne<Promotion>().WithMany()
                        .HasForeignKey("PromotionId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Promotion__Promo__619B8048"),
                    j =>
                    {
                        j.HasKey("PromotionId", "CategoryId").HasName("PK__Promotio__F354BC6F8671B72D");
                        j.ToTable("Promotion_Categories");
                    });
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Reviews__3214EC07A9AECA6D");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Book).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reviews__BookId__6AEFE058");

            entity.HasOne(d => d.User).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reviews__UserId__69FBBC1F");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC07E14BFF8D");

            entity.HasIndex(e => e.Code, "UQ__Roles__A25C5AA79F54566D").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RolePerm__3214EC07E8986A14");

            entity.ToTable("Role_Permissions");

            entity.HasIndex(e => new { e.RoleId, e.PermissionId }, "UQ_Role_Permission").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Permission).WithMany(p => p.RolePermissions)
                .HasForeignKey(d => d.PermissionId)
                .HasConstraintName("FK_RolePermissions_Permissions");

            entity.HasOne(d => d.Role).WithMany(p => p.RolePermissions)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_RolePermissions_Roles");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07BDC2D377");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D1053482E06048").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())", "DF__Users__CreatedAt__14270015");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_Users_IsActive");
            entity.Property(e => e.PhoneNumber).HasMaxLength(11);
            entity.Property(e => e.Rank).HasDefaultValue(0, "DF_Users_Rank");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
