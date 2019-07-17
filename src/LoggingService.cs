using System;
using System.IO;

namespace TgTranslator
{
    public class LoggingService
    {
        private static string logDirectory;
        private static string LogFile => Path.Combine(logDirectory, $"{DateTime.UtcNow.ToString("yyyy-MM-dd")}.txt");

        public static void PrepareDirectory()
        {
            logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            if (!Directory.Exists(logDirectory))     // Create the log directory if it doesn't exist
                Directory.CreateDirectory(logDirectory);
            if (!File.Exists(LogFile))               // Create today's log file if it doesn't exist
                File.Create(LogFile).Dispose();
        }

        public static void Log(string message)
        {
            string logText = DateTime.Now.ToString("[HH:mm:ss] ") + message;
            File.AppendAllText(LogFile, logText + "\n");     // Write the log text to a file

            Console.Out.WriteLine(logText);       // Write the log text to the console
        }
    }
}
