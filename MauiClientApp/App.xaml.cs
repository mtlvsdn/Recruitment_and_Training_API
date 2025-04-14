using MauiClientApp.Views;
using MauiClientApp.Services;

namespace MauiClientApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new StartUpPage());
        }

        // This method will be called from MauiProgram.cs after building the service provider
        public static void InitializeServices(IServiceProvider services)
        {
            ServiceHelper.Initialize(services);
        }
    }
}
