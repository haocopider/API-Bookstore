using AutoMapper;
using Bookstore.Shared.Dtos;
using Bookstore.Shared.DTOs;
using Bookstore.Shared.Interfaces;
using Bookstore.Shared.Models;

namespace Bookstore.Shared.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<CheckoutResponseDto> CheckoutAsync(CheckoutRequestDto request)
        {
            if (request.Items == null || !request.Items.Any())
                return new CheckoutResponseDto { IsSuccess = false, ErrorMessage = "Đơn hàng không có sản phẩm nào." };

            var newOrder = new Order
            {
                UserId = request.UserId,
                OrderDate = DateTime.UtcNow,
                ShippingAddress = request.ShippingAddress,
                PaymentMethod = request.PaymentMethod,
                Status = 0,
                TotalAmount = 0,
                FinalAmount = 0,
                OrderItems = new List<OrderItem>()
            };

            foreach (var cartItem in request.Items)
            {
                if (cartItem.ItemType == "Book")
                {
                    var formats = await _unitOfWork.BookFormats.FindAsync(
                        bf => bf.Id == cartItem.ItemId,
                        bf => bf.Book
                    );
                    var format = formats.FirstOrDefault();

                    // Các quy tắc Business Logic (Nghiệp vụ) nằm hết ở đây
                    if (format == null)
                        return new CheckoutResponseDto { IsSuccess = false, ErrorMessage = $"Sản phẩm ID {cartItem.ItemId} không tồn tại." };

                    if (format.Stock < cartItem.Quantity)
                        return new CheckoutResponseDto { IsSuccess = false, ErrorMessage = $"Sách '{format.Book.Title}' không đủ hàng. Kho chỉ còn {format.Stock}." };

                    format.Stock -= cartItem.Quantity;
                    _unitOfWork.BookFormats.Update(format);

                    newOrder.OrderItems.Add(new OrderItem
                    {
                        ItemType = "Book",
                        ItemId = format.Id,
                        Quantity = cartItem.Quantity,
                        PriceAtPurchase = format.Price,
                        SnapshotBookTitle = format.Book.Title,
                        SnapshotFormatName = format.FormatType.ToString(),
                        SnapshotUnitPrice = format.Price
                    });

                    newOrder.TotalAmount += (format.Price * cartItem.Quantity);
                }
            }

            newOrder.FinalAmount = newOrder.TotalAmount;

            // Giao dịch an toàn bằng UnitOfWork
            await _unitOfWork.Orders.AddAsync(newOrder);
            await _unitOfWork.CommitAsync();

            return new CheckoutResponseDto
            {
                IsSuccess = true,
                OrderId = newOrder.Id,
                TotalPaid = newOrder.FinalAmount
            };
        }


        public async Task<IEnumerable<OrderHistoryDto>> GetMyOrdersAsync(int userId)
        {
            var ordersFromDb = await _unitOfWork.Orders.FindAsync(
                o => o.UserId == userId,
                o => o.OrderItems
            );

            var sortedOrders = ordersFromDb.OrderByDescending(o => o.OrderDate);
            return _mapper.Map<IEnumerable<OrderHistoryDto>>(sortedOrders);
        }
    }
}
