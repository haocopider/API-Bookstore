using Microsoft.Extensions.Logging;

namespace PinkWater
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("materialdesignicons-webfont.ttf", "MDI");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton<Services.LocalDbService>();

            // Nếu bạn đang dùng dependency injection cho các trang, khai báo luôn:
            builder.Services.AddTransient<ViewModels.DetailViewModel>();
            builder.Services.AddTransient<Pages.ProductDetail>();
            return builder.Build();
        }
    }
}
