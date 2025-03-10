﻿using Microsoft.Extensions.Logging;
using MauiClient.ViewModels;

namespace MauiClient
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
                });

            builder.Services.AddSingleton<LoginPageViewModel>();
            builder.Services.AddSingleton<LoginPage>();
            builder.Logging.AddDebug();

            return builder.Build();
        }
    }
}