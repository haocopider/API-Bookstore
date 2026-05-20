using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using Bookstore.Shared.Models;

namespace Bookstore.Shared.Helpers
{
    public class AuditEntry
    {
        public AuditEntry(EntityEntry entry)
        {
            Entry = entry;
        }

        public EntityEntry Entry { get; }
        public int? AdminId { get; set; }
        public string? IpAddress { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty;
        public Dictionary<string, object?> KeyValues { get; } = new Dictionary<string, object?>();
        public Dictionary<string, object?> OldValues { get; } = new Dictionary<string, object?>();
        public Dictionary<string, object?> NewValues { get; } = new Dictionary<string, object?>();
        public List<PropertyEntry> TemporaryProperties { get; } = new List<PropertyEntry>();

        public bool HasTemporaryProperties => TemporaryProperties.Any();

        public AuditLog ToAuditLog()
        {
            var audit = new AuditLog
            {
                AdminId = AdminId,
                TableName = TableName,
                ActionType = ActionType,     // Đã khớp với DB của bạn
                CreatedAt = DateTime.UtcNow, // Đã khớp với DB của bạn
                IpAddress = IpAddress,       // Đã thêm IP

                // Nếu chỉ có 1 Id thì lấy giá trị dạng string, nếu có nhiều khóa chính thì Serialize thành JSON
                RecordId = KeyValues.Count == 1
                    ? KeyValues.Values.First()?.ToString()
                    : JsonConvert.SerializeObject(KeyValues),

                OldValues = OldValues.Count == 0 ? null : JsonConvert.SerializeObject(OldValues),
                NewValues = NewValues.Count == 0 ? null : JsonConvert.SerializeObject(NewValues)
            };
            return audit;
        }
    }
}