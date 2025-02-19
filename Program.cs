using Microsoft.Extensions.Logging;
using System;
using System.Windows.Forms;

namespace AggiuntaNumerazionePDF
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Option 1: Without logging
            Application.Run(new CambioPDF());

            // Option 2: With logging (requires setting up logging)
            // To use logging, you'll need to install the Microsoft.Extensions.Logging NuGet package
            // and configure a logger provider (e.g., console, file, etc.).  Here's a basic example
            // using the console logger:
            //
            // var loggerFactory = LoggerFactory.Create(builder =>
            // {
            //     builder.AddConsole(); // Add console logger.  You can configure other providers here.
            //     builder.SetMinimumLevel(LogLevel.Information); // Set the minimum log level.
            // });
            //
            // var logger = loggerFactory.CreateLogger<MetodoAddPDF>();
            //
            //Application.Run(new CambioPDF(logger));  // Pass the logger instance to the Form1 constructor
        }
    }
}