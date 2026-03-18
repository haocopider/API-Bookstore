namespace Bookstore.Shared
{
    public enum BookFormatType { Physical = 0, Ebook = 1 }
    public enum OrderStatus { Pending = 0, Shipping = 1, Success = 2, Cancelled = 3 }
    public enum DiscountType { Percentage = 0, FixedAmount = 1 }
}
