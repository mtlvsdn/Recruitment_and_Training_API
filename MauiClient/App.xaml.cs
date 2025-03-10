namespace MauiClient
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Exception ex = (Exception)args.ExceptionObject;
                Console.WriteLine($"Unhandled Exception: {ex.Message}");
            };

            MainPage = new AppShell();
        }
    }
}
