using Bookstore.Shared.Dtos;
using PinkWater.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;

namespace PinkWater.ViewModels
{
    internal class MainViewModel : BindableObject
    {
        private readonly BookApiService _apiService = new();
        public ObservableCollection<BookDto> Books { get; set; } = new();

        private bool _isEmpty;
        public bool IsEmpty
        {
            get => _isEmpty;
            set
            {
                _isEmpty = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotEmpty));
            }
        }

        public bool IsNotEmpty => !IsEmpty;

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public async Task LoadBooks()
        {
            if (IsBusy) return;

            IsBusy = true; // Bắt đầu tải
            try
            {
                var data = await _apiService.GetBooksAsync();
                Books.Clear();
                foreach (var item in data) Books.Add(item);
                IsEmpty = Books.Count == 0;
            }
            finally
            {
                IsBusy = false; // Kết thúc tải
            }
        }

        public ICommand GoToDetailCommand { get; }
        public ICommand GoToCartCommand { get; }

        public MainViewModel()
        {
            GoToDetailCommand = new Command<BookDto>(NavigateToDetail);
            GoToCartCommand = new Command( async () => await Shell.Current.GoToAsync(nameof(Pages.Cart)));
        }

        private async void NavigateToDetail(BookDto selectedBook)
        {
            if (selectedBook == null) return;
            var navigationParameter = new Dictionary<string, object>
        {
            { "SelectedBook", selectedBook }
        };

            await Shell.Current.GoToAsync(nameof(Pages.ProductDetail), navigationParameter);
        }
    }
}
