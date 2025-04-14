namespace MauiClientApp.Services
{
    public static class ServiceHelper
    {
        public static IServiceProvider Services { get; private set; }

        public static void Initialize(IServiceProvider serviceProvider)
        {
            Services = serviceProvider;
        }

        public static T GetService<T>() where T : class
        {
            if (Services == null)
            {
                throw new Exception("ServiceHelper not initialized. Call Initialize first.");
            }
            
            return Services.GetService<T>() ?? 
                throw new Exception($"Service of type {typeof(T).Name} not registered.");
        }
    }
} 