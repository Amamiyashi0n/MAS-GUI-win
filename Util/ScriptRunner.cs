using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace MAS_GUI.Util
{
    public static class ScriptRunner
    {
        private const string ResourceName = "MAS_GUI.Util.MAS_AIO.cmd";
        private static string _tempScriptPath;
        public static event Action<string> OnLog;

        private static void Log(string text)
        {
            if (OnLog != null)
            {
                OnLog(text);
            }
        }

        public static void ExtractScript()
        {
            string tempPath = Path.Combine(Path.GetTempPath(), "MAS_AIO.cmd");
            
            try
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }

                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName))
                {
                    if (stream == null)
                    {
                        throw new Exception("Could not find embedded resource: " + ResourceName);
                    }

                    using (FileStream fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
                    {
                        byte[] buffer = new byte[8192];
                        int bytesRead;
                        while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            fileStream.Write(buffer, 0, bytesRead);
                        }
                    }
                }
                _tempScriptPath = tempPath;
                Log("Extracted MAS_AIO.cmd to " + _tempScriptPath);
            }
            catch (Exception ex)
            {
                Log("Error extracting script: " + ex.Message);
                throw;
            }
        }

        public static void Run(string arguments, string taskName, StringBuilder outputCapture = null)
        {
            try
            {
                ExtractScript();

                ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/c \"" + _tempScriptPath + "\" " + arguments);
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.WindowStyle = ProcessWindowStyle.Hidden;

                Log(string.Format("Executing {0} with args: {1}", taskName, arguments));

                using (Process process = new Process())
                {
                    process.StartInfo = psi;
                    process.OutputDataReceived += (sender, e) => { 
                        if (e.Data != null) {
                            Log(e.Data);
                            if (outputCapture != null) outputCapture.AppendLine(e.Data);
                        }
                    };
                    process.ErrorDataReceived += (sender, e) => { 
                        if (e.Data != null) {
                            Log("ERR: " + e.Data);
                            if (outputCapture != null) outputCapture.AppendLine("ERR: " + e.Data);
                        }
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();

                    Log(string.Format("{0} finished with exit code {1}", taskName, process.ExitCode));
                }
            }
            catch (Exception ex)
            {
                Log("Error running script: " + ex.Message);
                throw;
            }
        }
    }
}
