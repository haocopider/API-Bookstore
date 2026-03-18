using Bookstore.Shared.Dtos;
using PinkWater.Models;
using PinkWater.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace PinkWater.ViewModels
{
    [QueryProperty(nameof(Book), "SelectedBook")]
    public class DetailViewModel : BindableObject
    {
        private readonly LocalDbService _localDbService;
        private BookDto _book;

        public BookDto Book
        {
            get => _book;
            set { _book = value; OnPropertyChanged(); }
        }

        // Lệnh thêm vào giỏ hàng
        public ICommand AddToCartCommand { get; }

        public DetailViewModel()
        {
            // Khởi tạo Service thủ công (Hoặc tiêm qua Constructor nếu bạn dùng DI)
            _localDbService = new LocalDbService();

            AddToCartCommand = new Command(async () => await ExecuteAddToCart());
        }

        private async Task ExecuteAddToCart()
        {
            if (Book == null) return;

            // Chuyển dữ liệu sách thành Item của giỏ hàng
            var cartItem = new CartItemLocal
            {
                BookId = Book.Id,
                Title = Book.Title,
                CoverImageUrl = Book.CoverImageUrl,
                Price = Book.MinPrice // Giả sử bạn lấy giá nhỏ nhất (Ebook)
            };

            // Lưu vào SQLite
            await _localDbService.AddToCartAsync(cartItem);

            // Hiển thị thông báo Toast cho người dùng (Phong cách Shopee)
            await Shell.Current.DisplayAlert("Thành công", "Đã thêm sản phẩm vào giỏ hàng", "OK");
        }
    }
}
