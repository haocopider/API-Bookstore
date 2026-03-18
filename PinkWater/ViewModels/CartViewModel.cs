using PinkWater.Models;
using PinkWater.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;

namespace PinkWater.ViewModels
{
    public class CartViewModel : BindableObject
    {
        private readonly LocalDbService _dbService;

        // Danh sách hiển thị lên màn hình
        public ObservableCollection<CartItemLocal> CartItems { get; set; } = new();

        // Biến lưu Tổng tiền
        private decimal _totalPrice;
        public decimal TotalPrice
        {
            get => _totalPrice;
            set { _totalPrice = value; OnPropertyChanged(); }
        }

        // Khai báo các nút bấm
        public ICommand LoadCartCommand { get; }
        public ICommand IncreaseCommand { get; }
        public ICommand DecreaseCommand { get; }

        public CartViewModel()
        {
            _dbService = new LocalDbService();

            LoadCartCommand = new Command(async () => await LoadCart());
            IncreaseCommand = new Command<CartItemLocal>(async (item) => await IncreaseQuantity(item));
            DecreaseCommand = new Command<CartItemLocal>(async (item) => await DecreaseQuantity(item));
        }

        // Hàm tải dữ liệu từ SQLite
        public async Task LoadCart()
        {
            var items = await _dbService.GetCartItemsAsync();
            CartItems.Clear();
            foreach (var item in items)
            {
                CartItems.Add(item);
            }
            CalculateTotal(); // Tính lại tiền
        }

        // Hàm nút [+]
        private async Task IncreaseQuantity(CartItemLocal item)
        {
            item.Quantity++;
            await _dbService.UpdateCartItemAsync(item); // Lưu vào Database

            // Cập nhật lại giao diện (mẹo: thay thế phần tử để UI nhận biết thay đổi)
            int index = CartItems.IndexOf(item);
            CartItems[index] = item;

            CalculateTotal();
        }

        // Hàm nút [-]
        private async Task DecreaseQuantity(CartItemLocal item)
        {
            if (item.Quantity > 1)
            {
                item.Quantity--;
                await _dbService.UpdateCartItemAsync(item);

                int index = CartItems.IndexOf(item);
                CartItems[index] = item;
            }
            else
            {
                // Nếu bằng 1 mà bấm trừ -> Xóa luôn
                bool confirm = await Shell.Current.DisplayAlert("Xóa", "Bỏ sản phẩm này khỏi giỏ hàng?", "Đồng ý", "Hủy");
                if (confirm)
                {
                    await _dbService.DeleteCartItemAsync(item);
                    CartItems.Remove(item);
                }
            }
            CalculateTotal();
        }

        // Hàm tính tổng tiền
        private void CalculateTotal()
        {
            decimal total = 0;
            foreach (var item in CartItems)
            {
                total += item.Price * item.Quantity;
            }
            TotalPrice = total;
        }
    }
}
