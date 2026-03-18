namespace PinkWater
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(Pages.ProductDetail), typeof(Pages.ProductDetail));
            Routing.RegisterRoute(nameof(Pages.Cart), typeof(Pages.Cart));
        }
    }
}
