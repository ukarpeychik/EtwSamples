namespace SimpleEventSource
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Reflection;

    public static class Utils
    {
        public static string GetFilePath(string fileName)
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), fileName);
        }

        public static void RunWithErrorCodeCheck(Func<uint> action)
        {
            uint result = action();

            // Success and ALREADY_EXISTS are OK.
            if (result != 0 && result != 183)
            {
                throw new Win32Exception((int) result);
            }
        }
    }
}
