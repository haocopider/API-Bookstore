using PinkWater.ViewModels;

namespace PinkWater
{
    public partial class MainPage : ContentPage
    {
        private readonly MainViewModel _vm = new MainViewModel();

        public MainPage()
        {
            InitializeComponent();
            BindingContext = _vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _vm.LoadBooks();
        }
    }
}
