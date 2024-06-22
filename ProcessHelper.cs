using System.Diagnostics;

namespace HW2Dumper
{
    public static class ProcessHelper
    {
        public static void TerminateProcessesByName(string processName)
        {
            Console.WriteLine($"TerminateProcessesByName called for: {processName}");
            try
            {
                foreach (Process process in Process.GetProcessesByName(processName))
                {
                    process.Kill();
                    Console.WriteLine($"Terminated process: {processName} with PID: {process.Id}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping {processName} process: {ex.Message}");
            }
        }
    }
}
