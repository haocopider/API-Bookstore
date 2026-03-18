using PinkWater.ViewModels;

namespace PinkWater.Pages;

public partial class ProductDetail : ContentPage
{
	public ProductDetail()
	{
		InitializeComponent();
		BindingContext = new DetailViewModel();
    }
}