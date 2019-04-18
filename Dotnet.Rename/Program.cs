using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace Dotnet.Rename
{
    class Program
    {
        public const string PROJECT_ARGUMENT = "-p | --project";
        public const string TARGET_ARGUMENT = "-t | --target";
        public const string SUBFOLDER_OPTION = "-s | --subfolder";
        private const string HelpOptions = "-? | -h | --help";

        static void Main(string[] args)
        {
            //https://msdn.microsoft.com/fr-fr/magazine/mt763239.aspx
            var app = new CommandLineApplication();
            app.HelpOption(HelpOptions);

            var projectParam = app.Argument(PROJECT_ARGUMENT, "Enter the relative path to a project file. ex: MyProject/MyProject.csproj");
            var targetParam = app.Argument(TARGET_ARGUMENT, "Enter the new name of the project. ex: MyNewProjectName");
            var subfolderOption = app.Option(SUBFOLDER_OPTION, "Enter the relative (from current directory) path to a subfolder where the new project (and project folder) will be placed.", CommandOptionType.SingleValue);


            app.OnExecute(() =>
            {
                var errors = new List<string>();
                if (projectParam.Value == null)
                    errors.Add($"{PROJECT_ARGUMENT} parameter is required.");
                if (targetParam.Value == null)
                    errors.Add($"{TARGET_ARGUMENT} parameter is required.");

                if (ShouldExit(app, errors)) return 1;

                if (!File.Exists(projectParam.Value))
                    errors.Add($"Unable to find file {projectParam.Value}.");

                if (!string.IsNullOrWhiteSpace(Path.GetDirectoryName(targetParam.Value)))
                    errors.Add($"{TARGET_ARGUMENT} should not be a path. It's just a name.");

                if (ShouldExit(app, errors)) return 1;

                var parameters = RunParameters.Create(
                    ".",
                    projectParam.Value,
                    targetParam.Value,
                    subfolderOption.HasValue() ? subfolderOption.Value() : null
                    );

                return 0;
            });

            try
            {
                app.Execute(args);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Environment.Exit(1);
            }
        }



        public static async Task MoveProjectAsync(RunParameters parameters)
        {
            await MoveProjectFileAsync(parameters.ProjectFullPath, parameters.TargetFullPath);

            var sourceDirectory = Path.GetDirectoryName(parameters.ProjectFullPath);
            var targetDirectory = Path.GetDirectoryName(parameters.TargetFullPath);
            await MoveDirectoryAsync(sourceDirectory, targetDirectory);
        }

        public static async Task MoveProjectsReferencesAsync(RunParameters parameters)
        {
            var projects = Directory.GetFiles(parameters.RootPath, "*.csproj", SearchOption.AllDirectories);

            foreach (var projFile in projects)
            {
                var isTheRenamedProject = Path.GetFullPath(projFile) == Path.GetFullPath(parameters.TargetPath);
                var projDirectory = Path.GetDirectoryName(projFile);

                var xml = new XmlDocument();
                xml.Load(projFile);

                foreach (var pRef in xml.SelectNodes("//ProjectReference").Cast<XmlNode>())
                {
                    var includeAtt = pRef.Attributes["Include"];
                    var includePath = includeAtt?.Value;
                    if (includePath == null) continue;

                    var fullPath = Path.IsPathRooted(includePath) ? includePath : Path.Combine(projDirectory, includePath);
                    
                    if (!File.Exists(fullPath))
                    {
                        if (isTheRenamedProject)
                        {
                            includeAtt.Value = parameters.GetRelativePathFromTarget(includePath);
                        }

                        // NOTE : projFile IS A RELATIVE PATH
                        //else if(Path.Combine(projDirectory, includePath)
                        //{
                        //    if ()
                        //        includeAtt.Value = parameters.GetTargetPathFrom;
                        //}
                    }
                }

                xml.Save(projFile);
            }


            var solutions = Directory.GetFiles(parameters.RootPath, "*.sln", SearchOption.AllDirectories);
        }


        static bool ShouldExit(CommandLineApplication app, List<string> errors)
        {
            if (errors.Count == 0) return true;

            foreach (var err in errors)
                Console.Error.WriteLine(err);
            app.ShowHelp();

            return false;
        }

        /// <summary>
        /// Move the source project to the target project fil
        /// using : "git mv" command and falling back to
        /// simple directory move if no source controlled files
        /// </summary>
        private static Task MoveProjectFileAsync(string source, string target)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = $"mv \"{source}\" \"{target}\" ",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
            };

            using (var gitProcess = Process.Start(startInfo))
            {
                gitProcess.WaitForExit();
                if (gitProcess.ExitCode != 0)
                {
                    var error = gitProcess.StandardError.ReadToEnd();
                    if (!error.Contains("not under version control"))
                    {
                        throw new Exception($"Git command failed: '{error}'");
                    }
                    else
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(target));
                        File.Move(source, target);
                    }
                }
            }

            return Task.CompletedTask;
        }


        /// <summary>
        /// Move the source directory to the target directory
        /// using : "git mv" command and falling back to
        /// simple directory move if no source controlled files
        /// </summary>
        static Task MoveDirectoryAsync(string source, string target)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = $"mv \"{source}\" \"{target}\" ",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
            };

            using (var gitProcess = Process.Start(startInfo))
            {
                gitProcess.WaitForExit();
                if (gitProcess.ExitCode != 0)
                {
                    var error = gitProcess.StandardError.ReadToEnd();
                    if (!error.Contains("source directory is empty"))
                    {
                        throw new Exception($"Git command failed: '{error}'");
                    }
                    else
                    {
                        var newFolder = Path.GetDirectoryName(target);
                        Directory.CreateDirectory(newFolder);
                        foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
                        {
                            var relativePath = Path.GetRelativePath(source, file);
                            var newPath = Path.Combine(target, relativePath);

                            Directory.CreateDirectory(Path.GetDirectoryName(newPath));
                            File.Move(file, newPath);
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
