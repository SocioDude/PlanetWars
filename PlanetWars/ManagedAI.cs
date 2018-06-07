using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/**
 * TODOs:
 *   * Find some way to clean this up if things crash.  Don't want to litter processes around people's
 *     machines.
 *   * Deal with AIs that take too long.
 */

namespace PlanetWars
{
    public class ManagedAI
    {
        private Process AIProcess;

        public ManagedAI(string pathToExe)
        {
            AIProcess = new Process();
            AIProcess.StartInfo.FileName = pathToExe;
            AIProcess.StartInfo.UseShellExecute = false;
            AIProcess.StartInfo.RedirectStandardInput = true;
            AIProcess.StartInfo.RedirectStandardOutput = true;
            AIProcess.StartInfo.RedirectStandardError = true;
            AIProcess.Start();
        }

        public List<string> GetOrders(List<string> gameState)
        {
            StreamWriter writer = AIProcess.StandardInput;
            AIProcess.StandardInput.NewLine = "\n";
            gameState.ForEach(x => writer.WriteLine(x));
            writer.WriteLine("go");


            StreamReader reader = AIProcess.StandardOutput;
            string line = reader.ReadLine();
            List<string> orders = new List<string>();
            while (line != "go")
            {
                orders.Add(line);
                line = reader.ReadLine();
                if (line == null)
                {
                    // The process is toast if this happens.
                    return null;
                }
            }

            return orders;
        }

        public List<string> GetAllErrorLines()
        {
            StreamReader reader = AIProcess.StandardError;
            List<string> results = new List<string>();
            while (reader.Peek() != -1)
            {
                results.Add(reader.ReadLine());
            }

            return results;
        }

        public void EndProcess()
        {
            MurderMachine(AIProcess.Id);
        }

        private static void MurderMachine(int pid)
        {
            ManagementObjectSearcher processSearcher =
                new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection processCollection = processSearcher.Get();

            try
            {
                Process proc = Process.GetProcessById(pid);
                if (!proc.HasExited) proc.Kill();
            }
            catch (Exception)
            {
                // Process already exited.
            }

            if (processCollection != null)
            {
                foreach (ManagementObject mo in processCollection)
                {
                    MurderMachine(Convert.ToInt32(mo["ProcessID"])); //kill child processes(also kills childrens of childrens etc.)
                }
            }
        }
    }
}
