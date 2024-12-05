using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace ExeBlocker
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var query = new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_Process'");
            using(var watcher = new ManagementEventWatcher(query))
            {
                watcher.EventArrived += (sender, e) =>
                {
                    var newProcessInstance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
                    string[] processNames = { "EpicGamesLauncher", "EpicWebHelper" };

                    foreach(var processName in processNames)
                    {
                        var processes = Process.GetProcessesByName(processName);

                        foreach(var process in processes)
                        {
                            try
                            {
                                var startInfo = new ProcessStartInfo("taskkill", $"/F /IM {processName}.exe")
                                {
                                    CreateNoWindow = true,
                                    WindowStyle = ProcessWindowStyle.Hidden
                                };

                                Process.Start(startInfo); 
                                Console.WriteLine($"Killing process: {processName}.exe");
                            }
                            catch(Exception ex)
                            {
                                Console.WriteLine($"Error killing process {process.ProcessName}: {ex.Message}");
                            }
                        }
                    }

                    Console.WriteLine("All specified processes killed.");
                };

                watcher.Start();
                Console.WriteLine("Monitoring started...");
                Console.ReadLine();
                watcher.Stop();
            }
        }
    }
}
