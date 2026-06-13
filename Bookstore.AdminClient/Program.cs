using ApexCharts;
using Blazored.LocalStorage;
using Bookstore.AdminClient;
using Bookstore.AdminClient.Auth;
using Bookstore.AdminClient.Interfaces;
using Bookstore.AdminClient.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


// Register Blazored.LocalStorage
builder.Services.AddBlazoredLocalStorage();

// Authorization / authentication
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, AuthStateProvider>();

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<NotificationService>();

builder.Services.AddTransient<AuthHeaderHandler>();

builder.Services.AddScoped<ApiErrorHandler>();

builder.Services.AddApexCharts();

var apiBase = builder.Configuration["api:baseUrl"];
builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri(apiBase);
}).AddHttpMessageHandler<AuthHeaderHandler>()
.AddHttpMessageHandler<ApiErrorHandler>();

builder.Services.AddScoped(
    sp => sp.GetRequiredService<IHttpClientFactory>()
            .CreateClient("API"));



builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));

await builder.Build().RunAsync();
