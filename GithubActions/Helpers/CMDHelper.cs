using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GithubActions.Helpers
{
    public class CMDHelper
    {


        public Task Execute(string repoUrl, string workingDir)
        {
            return Task.Run(() =>
            {
                var psi = new ProcessStartInfo("cmd.exe", $"/C git clone {repoUrl}");
                psi.UseShellExecute = false;
                psi.WorkingDirectory = workingDir;
                psi.CreateNoWindow = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;

                var osArchitecture = System.Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");


                using (var proc = new Process { StartInfo = psi, EnableRaisingEvents = true })
                {
                    proc.OutputDataReceived += (sender, argsx) => Console.WriteLine(argsx.Data);
                    proc.Start();
                    proc.BeginOutputReadLine();
                    proc.WaitForExit();
                }
            });                                                                     
        }

        private static void EventHandler(object sender, EventArgs e)
        {
            var p = sender as Process;
            if (p == null)
                return;
            var stdErr = p.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(stdErr))
            {
                Console.WriteLine(string.Format("Log:({0}) {1} ", p.ExitCode, stdErr));
                throw new Exception(stdErr);
            }
                

        }
    }
}
