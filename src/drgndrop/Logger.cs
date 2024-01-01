using System.Diagnostics;

namespace drgndrop
{
    internal class Logger
    {
#if DEBUG
        public static void Log(string prefix, string method, string message)
        {
            Console.WriteLine($"[ {prefix} ][ {method} ]: {message}");
        }

        public static string GetCallerMethodName()
        {
            StackTrace stack = new StackTrace();
            return $"{stack.GetFrame(3).GetMethod().Name}::{stack.GetFrame(2).GetMethod().Name}";
        }

        public static void InfoLog(string message)
        {
            Log("INFO", GetCallerMethodName(), message);
        }

        public static void WarningLog(string message)
        {
            Log("Warning", GetCallerMethodName(), message);
        }

        public static void ErrorLog(string message)
        {
            Log("ERROR", GetCallerMethodName(), message);
        }

        public static void DebugLog(string message)
        {
            Log("DEBUG", GetCallerMethodName(), message);
        }
#else
        public static void Log(string prefix, string message)
        {
            Console.WriteLine($"[ {prefix} ]: {message}");
        }

        public static void InfoLog(string message)
        {
            Log("INFO", message);
        }

        public static void WarningLog(string message)
        {
            Log("Warning", message);
        }

        public static void ErrorLog(string message)
        {
            Log("ERROR", message);
        }

        public static void DebugLog(string message) { }
#endif
    }
}
