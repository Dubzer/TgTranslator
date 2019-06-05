using System;
using System.IO;

namespace Translathor
{
    public class LoggingService
    {
        private static string logDirectory { get; set; }
        private static string logFile => Path.Combine(logDirectory, $"{DateTime.UtcNow.ToString("yyyy-MM-dd")}.txt");

        public LoggingService()
        {
            logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
            Console.OutputEncoding = System.Text.Encoding.UTF8;
        }

        public static void Log(string message)
        {
            if (!Directory.Exists(logDirectory))     // Create the log directory if it doesn't exist
                Directory.CreateDirectory(logDirectory);
            if (!File.Exists(logFile))               // Create today's log file if it doesn't exist
                File.Create(logFile).Dispose();

            string logText = DateTime.Now.ToString("[HH:mm:ss] ") + message;
            File.AppendAllText(logFile, logText + "\n");     // Write the log text to a file

            Console.Out.WriteLine(logText);       // Write the log text to the console
        }
    }
}
