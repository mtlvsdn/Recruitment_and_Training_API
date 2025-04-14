using Microsoft.Extensions.Logging;
using MauiClientApp.Services;
using MauiClientApp.Views;
using MauiClientApp.Converters;
using MauiClientApp.Views.User;
using MauiClientApp.Views.Company;
using MauiClientApp.Views.Tests;
using MauiClientApp.ViewModels;

namespace MauiClientApp;

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
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		// Register services
		builder.Services.AddSingleton<ApiService>();
		
#if ANDROID
        string baseUrl = "http://10.0.2.2:7287";
#else
        string baseUrl = "https://localhost:7287";
#endif

		builder.Services.AddSingleton<ITestService>(serviceProvider => {
			var httpClient = new HttpClient();
#if DEBUG
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            httpClient = new HttpClient(handler);
#endif
			return new TestService(httpClient, baseUrl);
		});

        // Register converters as resources
        builder.Services.AddSingleton<StringNotNullOrEmptyToBoolConverter>();
        builder.Services.AddSingleton<InvertedBoolConverter>();
        builder.Services.AddSingleton<BoolToFontAttributesConverter>();

        // Register ViewModels (important for dependency injection)
        builder.Services.AddTransient<TestAnalyticsDetailViewModel>();
        builder.Services.AddTransient<TestResultsViewModel>();

        // Register pages
        builder.Services.AddTransient<StartUpPage>();
        builder.Services.AddTransient<UserLoginPage>();
        builder.Services.AddTransient<CompanyLoginPage>();
        builder.Services.AddTransient<TestResultsPage>();
        builder.Services.AddTransient<TestAnalyticsDetailPage>();

        var app = builder.Build();
        
        // Initialize the ServiceHelper with the service provider
        App.InitializeServices(app.Services);
        
		return app;
	}
}
