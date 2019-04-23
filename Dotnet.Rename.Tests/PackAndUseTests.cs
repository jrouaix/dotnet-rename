using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Dotnet.Rename.Tests
{
    public class PackAndUseTests : IDisposable
    {
        const string PACKAGE_NAME = "Dotnet.Rename";
        const string COMMAND_NAME = "dotnet-rename";
        private readonly ITestOutputHelper _output;
        private readonly string _tmpOutput;
        private readonly string _solutionFolder;

        public PackAndUseTests(ITestOutputHelper output)
        {
            _output = output;
            _solutionFolder = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "../../../.."));
            _tmpOutput = Path.Combine(_solutionFolder, "_testsnugetpakage." + Path.GetRandomFileName());
        }

        public void Dispose()
        {
            if (Directory.Exists(_tmpOutput))
                Directory.Delete(_tmpOutput, true);

            UninstallTool();
        }

        [Fact]
        public void Should_pack_install_tool_execute_and_uninstall()
        {
            UninstallTool();
            PackTool();
            InstallTool();

            var sampleExecutionPath = Path.Combine(_tmpOutput, "sample");
            FileSystemHelper.Copy(Path.Combine(_solutionFolder, "_sample"), sampleExecutionPath);

            using (var process = Process.Start(new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"tool list -g",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WorkingDirectory = sampleExecutionPath
            }))
            {
                process.WaitForExit();
                if (process.ExitCode != 0)
                    throw new Exception($"Failed : '{process.StandardError.ReadToEnd()}'");

                _output.WriteLine($"Tools list : '{process.StandardOutput.ReadToEnd()}'");
            }

            using (var process = Process.Start(new ProcessStartInfo
            {
                FileName = COMMAND_NAME,
                Arguments = $"SampleLib/SampleLib.csproj Sample.Lib -s src",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WorkingDirectory = sampleExecutionPath
            }))
            {
                process.WaitForExit();
                if (process.ExitCode != 0)
                    throw new Exception($"Failed : '{process.StandardError.ReadToEnd()}'");
            }
        }



        private void InstallTool()
        {
            using (var process = Process.Start(new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"tool install -g {PACKAGE_NAME} --add-source {_tmpOutput}",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WorkingDirectory = _solutionFolder
            }))
            {
                process.WaitForExit();
                if (process.ExitCode != 0)
                    throw new Exception($"Failed : '{process.StandardError.ReadToEnd()}'");
            }
        }

        private void UninstallTool()
        {
            using (var process = Process.Start(new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"tool uninstall -g {PACKAGE_NAME}",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WorkingDirectory = _solutionFolder
            }))
            {
                process.WaitForExit();
                // Errors are ignored
            }
        }

        private void PackTool()
        {
            using (var process = Process.Start(new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"pack ./{PACKAGE_NAME}/{PACKAGE_NAME}.csproj --configuration Release --output {_tmpOutput}",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WorkingDirectory = _solutionFolder
            }))
            {
                process.WaitForExit();
                if (process.ExitCode != 0)
                    throw new Exception($"Failed : '{process.StandardError.ReadToEnd()}'");
            }
        }
    }
}
