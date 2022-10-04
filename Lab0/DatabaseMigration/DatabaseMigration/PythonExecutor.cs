using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace DatabaseMigration
{
    public class PythonExecutor
    {
        private string PythonPath { get; }
        private string ScriptPath { get; }
        private List<string> Arguments {get; } 
        
        public PythonExecutor(string pythonPath, string scriptPath, List<string> arguments)
        {
            PythonPath = pythonPath;
            ScriptPath = scriptPath;
            Arguments = arguments;
        }
        
        public void RunScript()
        {
            StringBuilder argumentsStringBuilder = new();
            foreach (var argument in Arguments)
            {
                argumentsStringBuilder.Append($" {argument}");
            }

            var processStartInfo = new ProcessStartInfo
            {
                FileName = PythonPath,
                Arguments = $"{ScriptPath} {argumentsStringBuilder}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            string output;
            string error;
            
            using (var process = Process.Start(processStartInfo))
            {
                output = process.StandardOutput.ReadToEnd();
                error = process.StandardError.ReadToEnd();
            }
            
            Console.WriteLine(error);
            Console.WriteLine(output);
        }
    }
}