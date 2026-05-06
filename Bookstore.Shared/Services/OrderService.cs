using AutoMapper;
using Bookstore.Shared.Dtos;
using Bookstore.Shared.Helpers;
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

        // 0: Chờ, 1:Giao, 2:Đã giao, 3.Hoàn thành, 4.Huỷ
        public async Task<OrderDto> CreateOrderAsync(int userId, CreateOrderDto request)
        {
            string orderCode = OrderHelper.GenerateOrderCode(userId);
            decimal totalAmount = request.Items.Sum(i => i.Quantity * i.UnitPrice);

            // 1. LẤY TRƯỚC DỮ LIỆU CẦN THIẾT (TỐI ƯU QUERY)
            var itemIds = request.Items.Select(i => i.BookId).ToList();
            var formatsInDb = await _unitOfWork.BookFormats.FindAsync(f => itemIds.Contains(f.Id));
            var user = await _unitOfWork.Users.GetFirstOrDefaultAsync(u => u.Id == userId);

            // 2. KHỞI TẠO ĐỐI TƯỢNG ORDER CÙNG VỚI ORDER ITEMS
            // Lưu ý: EF Core sẽ tự động lấy ID của Order gán cho OrderItems khi Commit. Bạn không cần làm thủ công!
            var order = new Order
            {
                OrderCode = orderCode,
                UserId = userId,
                ShippingAddress = request.ShippingAddress,
                TotalAmount = totalAmount,
                FinalAmount = request.FinalAmount,
                PaymentMethod = request.PaymentMethod,
                PointIsUsed = request.PointIsUsed,
                Status = 0, // Mặc định là chờ xử lý
                OrderDate = DateTime.UtcNow,
                OrderItems = request.Items.Select(item => new OrderItem
                {
                    ItemId = item.BookId,
                    Quantity = item.Quantity,
                    PriceAtPurchase = item.UnitPrice,
                    SnapshotBookTitle = item.BookTitle,
                    SnapshotUnitPrice = item.OriginalPrice
                }).ToList()
            };

            // 3. VALIDATION: KIỂM TRA ĐIỂM VÀ TỒN KHO TRƯỚC KHI THỰC HIỆN
            string errorMessage = string.Empty;

            if (request.PointIsUsed > 0 && (user == null || user.CurrentPoints < request.PointIsUsed))
            {
                errorMessage = "Điểm tích lũy không đủ.";
            }

            if (string.IsNullOrEmpty(errorMessage))
            {
                foreach (var item in request.Items)
                {
                    var format = formatsInDb.FirstOrDefault(f => f.Id == item.BookId);
                    if (format == null || format.Stock < item.Quantity)
                    {
                        errorMessage = $"Sản phẩm '{item.BookTitle}' không tồn tại hoặc không đủ số lượng.";
                        break; // Dừng kiểm tra ngay khi phát hiện lỗi
                    }
                }
            }

            // 4. XỬ LÝ NẾU CÓ LỖI (HẾT HÀNG / THIẾU ĐIỂM)
            if (!string.IsNullOrEmpty(errorMessage))
            {
                // Ghi nhận đơn hàng này thất bại (Status = 4)
                order.Status = 4;

                await _unitOfWork.Orders.AddAsync(order);
                await _unitOfWork.CommitAsync(); // Lưu đơn hàng thất bại để hệ thống tracking

                // Quăng exception ra ngoài để Controller trả về lỗi 400 cho Frontend
                throw new InvalidOperationException(errorMessage);
            }

            // 5. NẾU MỌI THỨ HỢP LỆ: TIẾN HÀNH TRỪ KHO VÀ ĐIỂM
            foreach (var item in request.Items)
            {
                var format = formatsInDb.First(f => f.Id == item.BookId);
                format.Stock -= item.Quantity;
                _unitOfWork.BookFormats.Update(format);
            }

            if (request.PointIsUsed > 0 && user != null)
            {
                user.CurrentPoints -= request.PointIsUsed;
                _unitOfWork.Users.Update(user);
            }

            // 6. LƯU TẤT CẢ VÀO DATABASE (CHỈ 1 TRANSACTION DUY NHẤT)
            // Bao gồm: Tạo Order, Tạo OrderItems, Cập nhật Stock, Cập nhật User Points
            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.CommitAsync();

            return _mapper.Map<OrderDto>(order);
        }
        public async Task<bool> CompleteOrderAsync(int userId, int orderId)
        {
            var order = await _unitOfWork.Orders.GetFirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);


            if (order == null) throw new KeyNotFoundException("Không tìm thấy đơn hàng.");

            if (order.Status != 2)
            {
                throw new InvalidOperationException("Đơn hàng chưa được giao, không thể hoàn thành.");
            }

            var user = await _unitOfWork.Users.GetFirstOrDefaultAsync(u => u.Id == userId);

            int pointsEarned = (int)(order.FinalAmount / 10000);
            if (user != null)
            {
                user.CurrentPoints += pointsEarned;
                user.TotalPoints += pointsEarned;
                user.Rank = (int)RankHelper.GetRank(user.TotalPoints);
                _unitOfWork.Users.Update(user);
            }

            order.Status = 3;

            _unitOfWork.Orders.Update(order);
            await _unitOfWork.CommitAsync();

            return true;
        }


        public async Task<IEnumerable<OrderDto>> GetOrderHistoryAsync(int userId, int? status = null)
        {
            var orders = await _unitOfWork.Orders.FindAsync(
                o => o.UserId == userId && (!status.HasValue || o.Status == status.Value),
                includes: o => o.OrderItems
            );

            var sortedOrders = orders.OrderByDescending(o => o.OrderDate);

            return _mapper.Map<IEnumerable<OrderDto>>(sortedOrders);
        }

        public async Task<OrderDto> GetOrderDetailAsync(int userId, int orderId)
        {
            var order = await _unitOfWork.Orders.GetFirstOrDefaultAsync(
                filter: o => o.Id == orderId && o.UserId == userId,
                includes: o => o.OrderItems
            );

            if (order == null) throw new KeyNotFoundException("Không tìm thấy đơn hàng.");

            return _mapper.Map<OrderDto>(order);
        }

        public async Task<bool> CancelOrderAsync(int userId, int orderId, string cancelReason)
        {
            var order = await _unitOfWork.Orders.GetFirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null) throw new KeyNotFoundException("Không tìm thấy đơn hàng.");


            if (order.Status != 0)
            {
                throw new InvalidOperationException("Đơn hàng đang được xử lý hoặc đang giao, không thể hủy.");
            }

            order.Status = 4; 
            _unitOfWork.Orders.Update(order);
            await _unitOfWork.CommitAsync();
            return true;
        }
    }
}
