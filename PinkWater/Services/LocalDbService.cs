using PinkWater.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace PinkWater.Services
{
    public class LocalDbService
    {
        private const string DbName = "BookstoreLocal.db3";
        private readonly SQLiteAsyncConnection _connection;

        public LocalDbService()
        {
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, DbName);
            _connection = new SQLiteAsyncConnection(dbPath);

            // Tạo bảng nếu nó chưa tồn tại (chạy cực nhanh)
            _connection.CreateTableAsync<CartItemLocal>().Wait();
        }

        // Lấy toàn bộ giỏ hàng
        public async Task<List<CartItemLocal>> GetCartItemsAsync()
        {
            return await _connection.Table<CartItemLocal>().ToListAsync();
        }

        // Thêm vào giỏ hàng (Xử lý thông minh: Nếu có rồi thì cộng dồn số lượng)
        public async Task AddToCartAsync(CartItemLocal item)
        {
            // Kiểm tra xem sách này đã có trong giỏ chưa
            var existingItem = await _connection.Table<CartItemLocal>()
                                                .Where(x => x.BookId == item.BookId)
                                                .FirstOrDefaultAsync();

            if (existingItem != null)
            {
                existingItem.Quantity += 1; // Tăng số lượng
                await _connection.UpdateAsync(existingItem);
            }
            else
            {
                item.Quantity = 1; // Thêm mới
                await _connection.InsertAsync(item);
            }
        }

        // Cập nhật lại số lượng sách trong giỏ
        public async Task UpdateCartItemAsync(CartItemLocal item)
        {
            await _connection.UpdateAsync(item);
        }

        // Xóa hẳn cuốn sách khỏi giỏ (khi số lượng giảm về 0)
        public async Task DeleteCartItemAsync(CartItemLocal item)
        {
            await _connection.DeleteAsync(item);
        }

        // Xóa khỏi giỏ hàng
        public async Task RemoveFromCartAsync(CartItemLocal item)
        {
            await _connection.DeleteAsync(item);
        }

        // Xóa sạch giỏ hàng (Sau khi đặt hàng thành công)
        public async Task ClearCartAsync()
        {
            await _connection.DeleteAllAsync<CartItemLocal>();
        }
    }
}
