using AutoMapper;
using Bookstore.Shared.Dtos;
using Bookstore.Shared.Interfaces;
using Bookstore.Shared.Models;

namespace Bookstore.Shared.Services
{
    public class BookService : IBookService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IPromotionService _promotionService;

        public BookService(IUnitOfWork unitOfWork, IMapper mapper, IPromotionService promotionService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _promotionService = promotionService;
        }

        public async Task<BookAdminDto> GetBookAdminByIdAsync(int id)
        {
            var books = await _unitOfWork.Books.FindAsync(
                b => b.Id == id,
                b => b.Author,
                b => b.BookFormats,
                b => b.Categories
            );

            var book = books.FirstOrDefault();
            if (book == null) return null;

            var dto = _mapper.Map<BookAdminDto>(book);

            // Populate UpdatedBy from the latest audit log for this book
            try
            {
                var audits = await _unitOfWork.AuditLogs.FindAsync(a => a.TableName == "Books" && a.RecordId == book.Id.ToString(), a => a.Admin);
                var last = audits.OrderByDescending(a => a.CreatedAt).FirstOrDefault();
                if (last != null)
                {
                    if (last.Admin != null)
                    {
                        var name = $"{last.Admin.FirstName} {last.Admin.LastName}".Trim();
                        dto.UpdatedBy = string.IsNullOrWhiteSpace(name) ? last.Admin.UserName : name;
                    }
                }
            }
            catch
            {
                // swallow any audit lookup errors to avoid breaking book retrieval
            }

            return dto;
        }

        public async Task<IEnumerable<BookDto>> GetAllBooksAsync()
        {
            var booksFromDb = await _unitOfWork.Books.GetAllAsync(
                b => b.Author,
                b => b.BookFormats,
                b => b.Categories,
                b => b.Reviews
            );
            var bookDto = _mapper.Map<IEnumerable<BookDto>>(booksFromDb);

            foreach (var book in bookDto)
            {
                var promotion = await _promotionService.GetBestPromotionForBookAsync(book.Id, book.Price);
                if (promotion != null)
                {
                    book.Promotion = promotion;
                }
            }
            return bookDto;
        }

        public async Task<IEnumerable<BookDto>> SearchBooksAsync(BookFilterRequestDto filter)
        {
            var booksFromDb = await _unitOfWork.Books.FindAsync(
                b =>
                    b.IsDeleted == false &&
                    (
                        string.IsNullOrEmpty(filter.SearchText) ||
                        b.Title.Contains(filter.SearchText) ||
                        (b.Author != null && b.Author.Name.Contains(filter.SearchText))
                    ) &&
                    (string.IsNullOrEmpty(filter.Tag) || b.Categories.Any(c => c.Slug == filter.Tag)) &&
                    (!filter.MinPrice.HasValue || b.BookFormats.Any(f => f.Price >= filter.MinPrice.Value)) &&
                    (!filter.MaxPrice.HasValue || b.BookFormats.Any(f => f.Price <= filter.MaxPrice.Value)),
                b => b.Author,
                b => b.BookFormats,
                b => b.Categories,
                b => b.Reviews
            );

            var bookDto = _mapper.Map<IEnumerable<BookDto>>(booksFromDb);

            foreach (var book in bookDto)
            {
                var promotion = await _promotionService.GetBestPromotionForBookAsync(book.Id, book.Price);
                if (promotion != null)
                {
                    book.Promotion = promotion;
                }
            }

            return bookDto;
        }
        public async Task<IEnumerable<BookDto>> GetBooksByIdsAsync(List<int> ids)
        {
            var booksFromDb = await _unitOfWork.Books.FindAsync(
                b => ids.Contains(b.Id),
                b => b.Author,
                b => b.BookFormats,
                b => b.Categories,
                b => b.Reviews
            );

            var bookDto = _mapper.Map<IEnumerable<BookDto>>(booksFromDb);

            foreach (var book in bookDto)
            {
                var promotion = await _promotionService.GetBestPromotionForBookAsync(book.Id, book.Price);
                if (promotion != null)
                {
                    book.Promotion = promotion;
                }
            }
            return bookDto;
        }

        public async Task<IEnumerable<BookDto>> GetBooksByCategoryIdAsync(int categoryId)
        {
            var booksFromDb = await _unitOfWork.Books.FindAsync(
                b => b.Categories.Any(c => c.Id == categoryId),
                b => b.Author,
                b => b.BookFormats,
                b => b.Categories,
                b => b.Reviews);
            var bookDto = _mapper.Map<IEnumerable<BookDto>>(booksFromDb);

            foreach (var book in bookDto)
            {
                var promotion = await _promotionService.GetBestPromotionForBookAsync(book.Id, book.Price);
                if (promotion != null)
                {
                    book.Promotion = promotion;
                }
            }
            return bookDto;
        }

        public async Task<bool> ReviewBookAsync(ReviewBookDto reviewRequest)
        {
            var review = _mapper.Map<Review>(reviewRequest);
            await _unitOfWork.Reviews.AddAsync(review);
            await _unitOfWork.CommitAsync();
            return true;
        }

        public async Task<BookDto> GetBookByIdAsync(int id)
        {
            var bookFromDb = await _unitOfWork.Books.FindAsync(
                b => b.Id == id,
                b => b.Author,
                b => b.BookFormats,
                b => b.Categories
            );

            var bookDto = _mapper.Map<BookDto>(bookFromDb.FirstOrDefault());

            if (bookDto != null)
            {
                var promotion = await _promotionService.GetBestPromotionForBookAsync(bookDto.Id, bookDto.Price);
                if (promotion != null)
                {
                    bookDto.Promotion = promotion;
                }
            }

            return bookDto;
        }

        public async Task<IEnumerable<BookDto>> GetBooksByPromotionIdAsync(int promotionId)
        {
            var now = DateTime.UtcNow;

            var targetPromotionList = await _unitOfWork.Promotions.FindAsync(
                p => p.Id == promotionId,
                p => p.Categories
            );

            var targetPromotion = targetPromotionList.FirstOrDefault();
            if (targetPromotion == null)
                return Enumerable.Empty<BookDto>();

            var categoryIds = targetPromotion.Categories.Select(c => c.Id).ToList();

            var booksFromDb = await _unitOfWork.Books.FindAsync(
                b => b.Promotions.Any(p => p.Id == promotionId) ||
                     b.Categories.Any(c => categoryIds.Contains(c.Id)),
                b => b.Author,
                b => b.BookFormats,
                b => b.Categories,
                b => b.Promotions
            );

            var bookDtos = _mapper.Map<List<BookDto>>(booksFromDb);

            var activePromotions = await _unitOfWork.Promotions.FindAsync(
                p => p.IsActive && p.StartDate <= now && p.EndDate >= now,
                p => p.Books,
                p => p.Categories
            );

            foreach (var book in bookDtos)
            {
                var promotion = await _promotionService.GetBestPromotionForBookAsync(book.Id, book.Price);
                if (promotion != null)
                {
                    book.Promotion = promotion;
                }
            }

            return bookDtos;
        }

        public async Task<IEnumerable<BookDto>> GetBooksByAuthorIdAsync(int authorId)
        {
            var booksFromDb = await _unitOfWork.Books.FindAsync(
                b => b.AuthorId == authorId,
                b => b.Author,
                b => b.BookFormats,
                b => b.Categories
            );
            var bookDto = _mapper.Map<IEnumerable<BookDto>>(booksFromDb);

            foreach (var book in bookDto)
            {
                var promotion = await _promotionService.GetBestPromotionForBookAsync(book.Id, book.Price);
                if (promotion != null)
                {
                    book.Promotion = promotion;
                }
            }

            return bookDto;
        }

        public async Task<bool> CreateBookAsync(CreateBookDto request)
        {
            // 1. Kiểm tra và Xử lý Tác giả (Author)
            var authorList = await _unitOfWork.Authors.FindAsync(a => a.Name.ToLower() == request.AuthorName.ToLower());
            var author = authorList.FirstOrDefault();

            // Nếu chưa tồn tại, tạo object Author mới. 
            // EF Core sẽ tự động INSERT Author này khi gọi CommitAsync
            if (author == null)
            {
                author = new Author { Name = request.AuthorName };
            }

            // 2. Kiểm tra và Xử lý Thể loại (Categories)
            var categories = new List<Category>();
            foreach (var catName in request.CategoryNames)
            {
                var catList = await _unitOfWork.Categories.FindAsync(c => c.Name.ToLower() == catName.ToLower());
                var category = catList.FirstOrDefault();

                if (category == null)
                {
                    // Tạm thời tạo Slug đơn giản, bạn có thể viết hàm chuyển tiếng Việt có dấu thành không dấu
                    category = new Category { Name = catName, Slug = catName.Replace(" ", "-").ToLower() };
                }
                categories.Add(category);
            }

            // 3. Khởi tạo đối tượng Book và BookFormats
            var book = new Book
            {
                Title = request.Title,
                Description = request.Description,
                Publisher = request.Publisher,
                CoverImageUrl = request.CoverImageUrl,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false,
                Author = author, // Gán Author (có thể là cũ hoặc mới)
                Categories = categories, // Gán list Categories (cũ hoặc mới)
                BookFormats = new List<BookFormat>()
            };

            // Ensure we have two formats: 0 = new, 1 = used
            var formatsMap = request.Formats.GroupBy(f => f.Format).ToDictionary(g => g.Key, g => g.First());

            // add new format (type 0)
            if (formatsMap.ContainsKey(0))
            {
                var f = formatsMap[0];
                book.BookFormats.Add(new BookFormat { Type = 0, Price = f.Price, Stock = f.Stock });
            }
            else if (formatsMap.ContainsKey(1))
            {
                // if only provided used format, still create new with default
                book.BookFormats.Add(new BookFormat { Type = 0, Price = 0, Stock = 0 });
            }
            else
            {
                // fallback: create default new format
                book.BookFormats.Add(new BookFormat { Type = 0, Price = 0, Stock = 0 });
            }

            // add used format (type 1)
            if (formatsMap.ContainsKey(1))
            {
                var f = formatsMap[1];
                book.BookFormats.Add(new BookFormat { Type = 1, Price = f.Price, Stock = f.Stock });
            }
            else if (formatsMap.ContainsKey(0))
            {
                // if only provided new format, still create used with default
                book.BookFormats.Add(new BookFormat { Type = 1, Price = 0, Stock = 0 });
            }
            else
            {
                // fallback: create default used format
                book.BookFormats.Add(new BookFormat { Type = 1, Price = 0, Stock = 0 });
            }

            // 4. Lưu vào Database
            await _unitOfWork.Books.AddAsync(book);
            await _unitOfWork.CommitAsync();

            return true;
        }

        public async Task<bool> UpdateBookAsync(int id, UpdateBookDto request)
        {
            // Lấy sách hiện tại cùng với các quan hệ liên quan
            var books = await _unitOfWork.Books.FindAsync(
                b => b.Id == id,
                b => b.Author,
                b => b.Categories,
                b => b.BookFormats
            );
            var book = books.FirstOrDefault();

            if (book == null || book.IsDeleted == true)
                return false;

            // 1. Cập nhật thông tin cơ bản
            book.Title = request.Title;
            book.Description = request.Description;
            book.Publisher = request.Publisher;
            book.CoverImageUrl = request.CoverImageUrl;

            // 2. Xử lý Tác giả (Tương tự Create)
            if (book.Author == null || book.Author.Name.ToLower() != request.AuthorName.ToLower())
            {
                var authorList = await _unitOfWork.Authors.FindAsync(a => a.Name.ToLower() == request.AuthorName.ToLower());
                var author = authorList.FirstOrDefault() ?? new Author { Name = request.AuthorName };
                book.Author = author;
            }

            // 3. Xử lý Thể loại (Xóa cái cũ không còn chọn, thêm cái mới)
            book.Categories.Clear(); // Xóa mapping cũ
            foreach (var catName in request.CategoryNames)
            {
                var catList = await _unitOfWork.Categories.FindAsync(c => c.Name.ToLower() == catName.ToLower());
                var category = catList.FirstOrDefault() ?? new Category { Name = catName, Slug = catName.Replace(" ", "-").ToLower() };
                book.Categories.Add(category);
            }

            // 4. Xử lý các định dạng (Formats): thêm mới / cập nhật / xóa
            var existingFormats = book.BookFormats.ToList();
            var requestFormats = request.Formats ?? new List<CreateBookFormatDto>();

            var matchedExistingIds = new HashSet<int>();

            foreach (var rf in requestFormats)
            {
                BookFormat? target = null;

                if (rf.Id.HasValue && rf.Id.Value > 0)
                {
                    target = existingFormats.FirstOrDefault(f => f.Id == rf.Id.Value);
                }

                if (target == null)
                {
                    // try match by Type
                    target = existingFormats.FirstOrDefault(f => f.Type == rf.Format);
                }

                if (target != null)
                {
                    // update existing
                    target.Price = rf.Price;
                    target.Stock = rf.Stock;
                    target.Type = rf.Format;
                    _unitOfWork.BookFormats.Update(target);
                    matchedExistingIds.Add(target.Id);
                }
                else
                {
                    // create new format
                    var nf = new BookFormat
                    {
                        Book = book,
                        Type = rf.Format,
                        Price = rf.Price,
                        Stock = rf.Stock
                    };
                    await _unitOfWork.BookFormats.AddAsync(nf);
                    book.BookFormats.Add(nf);
                }
            }

            // remove existing formats that are not present in request
            var toRemove = existingFormats.Where(f => !matchedExistingIds.Contains(f.Id)).ToList();
            foreach (var rem in toRemove)
            {
                // detach from book navigation
                book.BookFormats.Remove(rem);
                _unitOfWork.BookFormats.Remove(rem);
            }

            _unitOfWork.Books.Update(book);
            await _unitOfWork.CommitAsync();

            return true;
        }

        public async Task<bool> DeleteBookAsync(int id)
        {
            var books = await _unitOfWork.Books.FindAsync(b => b.Id == id);
            var book = books.FirstOrDefault();
            if (book == null || book.IsDeleted == true)
                return false;

            book.IsDeleted = true;
            _unitOfWork.Books.Update(book);
            await _unitOfWork.CommitAsync();

            return true;
        }

        public async Task<bool> RestockAsync(RestockDto request)
        {
            // Lấy định dạng sách dựa trên ID
            var formats = await _unitOfWork.BookFormats.FindAsync(f => f.Id == request.BookFormatId);
            var bookFormat = formats.FirstOrDefault();

            if (bookFormat == null)
                return false;

            // Tăng số lượng tồn kho
            bookFormat.Stock = (bookFormat.Stock ?? 0) + request.AddedQuantity;

            _unitOfWork.BookFormats.Update(bookFormat);
            await _unitOfWork.CommitAsync();

            return true;
        }

        public async Task<IEnumerable<BookAdminDto>> GetAllBooksForAdminAsync(BookFilterRequestDto filter)
        {
            var booksFromDb = await _unitOfWork.Books.FindAsync(
                b =>
                    (string.IsNullOrEmpty(filter.SearchText) ||
                     b.Title.Contains(filter.SearchText) ||
                     (b.Author != null && b.Author.Name.Contains(filter.SearchText))) &&
                    (string.IsNullOrEmpty(filter.Tag) || b.Categories.Any(c => c.Slug == filter.Tag)) &&
                    (!filter.MinPrice.HasValue || b.BookFormats.Any(f => f.Price >= filter.MinPrice.Value)) &&
                    (!filter.MaxPrice.HasValue || b.BookFormats.Any(f => f.Price <= filter.MaxPrice.Value)) ,
                b => b.Author,
                b => b.BookFormats,
                b => b.Categories,
                b => b.Reviews
            );

            var bookDto = _mapper.Map<IEnumerable<BookAdminDto>>(booksFromDb);

            foreach (var book in bookDto)
            {
                // get last audit for this book to determine UpdatedBy
                try
                {
                    var audits = await _unitOfWork.AuditLogs.FindAsync(a => a.TableName == "Books" && a.RecordId == book.Id.ToString(), a => a.Admin);
                    var last = audits.OrderByDescending(a => a.CreatedAt).FirstOrDefault();
                    if (last != null && last.Admin != null)
                    {
                        var name = $"{last.Admin.FirstName} {last.Admin.LastName}".Trim();
                        book.UpdatedBy = string.IsNullOrWhiteSpace(name) ? last.Admin.UserName : name;
                    }
                }
                catch
                {
                    // ignore audit lookup errors
                }

                var promotion = await _promotionService.GetBestPromotionForBookAsync(book.Id, book.Price);
                if (promotion != null)
                {
                    book.Promotion = promotion;
                }
            }
            return bookDto;
        }
    }
}
