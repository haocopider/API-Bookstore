using PinkWater.ViewModels;

namespace PinkWater.Pages;

public partial class Cart : ContentPage
{
	private readonly CartViewModel _vm = new CartViewModel();
    public Cart()
	{
		InitializeComponent();
		BindingContext = _vm;
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _vm.LoadCart();
    }
}