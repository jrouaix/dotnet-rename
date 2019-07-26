using Microsoft.Build.Construction;
using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static Dotnet.Rename.PathHelper;
using Console = Colorful.Console;

namespace Dotnet.Rename
{
    class Program
    {
        public const string PROJECT_ARGUMENT_NAME = "Project";
        public const string TARGET_ARGUMENT_NAME = "Target";
        public const string SUBFOLDER_OPTION = "-s | --subfolder";
        private const string HelpOptions = "-? | -h | --help";

        public static readonly Color ErrorColor = Color.OrangeRed;
        static void Log(string text) => Console.WriteLine(text);
        static void Error(string text) => Console.Error.WriteLine(text, ErrorColor);

        static void Main(string[] args)
        {
            //https://msdn.microsoft.com/fr-fr/magazine/mt763239.aspx
            var app = new CommandLineApplication();
            app.HelpOption(HelpOptions);

            var projectParam = app.Argument(PROJECT_ARGUMENT_NAME, "Enter the relative path to a project file. ex: MyProject/MyProject.csproj");
            var targetParam = app.Argument(TARGET_ARGUMENT_NAME, "Enter the new name of the project. ex: MyNewProjectName");
            var subfolderOption = app.Option(SUBFOLDER_OPTION, "Enter the relative (from current directory) path to a subfolder where the new project (and project folder) will be placed.", CommandOptionType.SingleValue);

            app.OnExecute(async () =>
            {
                var errors = new List<string>();
                if (projectParam.Value == null)
                    errors.Add($"{PROJECT_ARGUMENT_NAME} parameter is required.");
                if (targetParam.Value == null)
                    errors.Add($"{TARGET_ARGUMENT_NAME} parameter is required.");

                if (ShouldExit(app, errors)) return 1;

                if (!File.Exists(projectParam.Value))
                    errors.Add($"Unable to find file {projectParam.Value}.");

                if (!string.IsNullOrWhiteSpace(Path.GetDirectoryName(targetParam.Value)))
                    errors.Add($"{TARGET_ARGUMENT_NAME} should not be a path. It's just a name.");

                if (ShouldExit(app, errors)) return 1;

                var context = RunContext.Create(
                    ".",
                    projectParam.Value,
                    targetParam.Value,
                    subfolderOption.HasValue() ? subfolderOption.Value() : null,
                    Log
                    );

                await RunAsync(context);

                return 0;
            });

            try
            {
                app.Execute(args);
            }
            catch (Exception ex)
            {
                Error(ex.Message);
                Environment.Exit(1);
            }
        }

        public static async Task RunAsync(RunContext context)
        {
            context.Logger($"Moving '{context}'.");
            MoveProject(context);
            await ChangeProjectsReferencesAsync(context);
            await ChangeSolutionsReferencesAsync(context);
        }

        public static void MoveProject(RunContext context)
        {
            context.Logger($"Moving project file '{context.ProjectFullPath}' to '{context.TargetFullPath}'.");

            if (ShouldUseGit(context))
            {
                MoveFolderUsingGit(context);
            }
            else
            {
                StandardMoveFolder(context);
            }
        }

        public static async Task ChangeProjectsReferencesAsync(RunContext context)
        {
            var projects = Directory.GetFiles(context.RootPath, "*.csproj", SearchOption.AllDirectories);

            foreach (var projFile in projects)
            {
                context.Logger($"Updating '{projFile}'");

                var isTheRenamedProject = Path.GetFullPath(projFile) == Path.GetFullPath(context.TargetFullPath);
                var projDirectory = Path.GetDirectoryName(projFile);

                var xml = new XmlDocument();
                xml.Load(projFile);

                var changedSomething = false;

                var projectReferences = xml.SelectNodes("//ProjectReference").Cast<XmlNode>().ToArray();

                foreach (var pRef in projectReferences)
                {
                    var includeAtt = pRef.Attributes["Include"];
                    if (includeAtt == null) continue;
                    var includePath = P(includeAtt.Value);
                    if (includePath == null) continue;
                    if (Path.IsPathRooted(includePath)) continue;

                    var refFileName = Path.GetFileName(includePath);
                    if (!isTheRenamedProject && string.Compare(refFileName, context.ProjectFileName, StringComparison.InvariantCultureIgnoreCase) != 0)
                    {
                        continue;
                    }

                    var fullPath = Path.Combine(projDirectory, includePath);

                    if (!File.Exists(fullPath))
                    {
                        context.Logger($"Unable to find reference '{includePath}' ('{fullPath}') ... Updating ...");

                        var newIncludePath = isTheRenamedProject
                            ? context.GetRelativePathFromTarget(includePath)
                            : context.GetTargetPathFromPreviousPath(projFile, includePath)
                            ;

                        context.Logger($"... to '{newIncludePath}'.");

                        var newFullPath = Path.Combine(projDirectory, newIncludePath);
                        if (!File.Exists(newFullPath))
                            throw new InvalidProgramException($"Something is fishy, {newFullPath} file should exist.");

                        changedSomething = true;
                        includeAtt.Value = newIncludePath;
                    }
                }

                if (changedSomething)
                {
                    xml.Save(projFile);
                }
                else
                {
                    Log("... No change on this file.");
                }
            }
        }

        public static async Task ChangeSolutionsReferencesAsync(RunContext context)
        {
            var solutions = Directory.GetFiles(context.RootPath, "*.sln", SearchOption.AllDirectories);

            foreach (var solutionFile in solutions)
            {
                var solution = SolutionFile.Parse(Path.GetFullPath(solutionFile));
                foreach (var project in solution.ProjectsInOrder)
                {
                    var isTheRenamedProject = project.AbsolutePath == Path.GetFullPath(context.ProjectFullPath);
                    if (!isTheRenamedProject) continue;

                    var file = await File.ReadAllLinesAsync(solutionFile);
                    for (int i = 0; i < file.Length; i++)
                    {
                        var line = file[i];
                        var thisIsTheLine = line.StartsWith("Project(\"") && line.Contains(project.RelativePath);
                        if (thisIsTheLine)
                        {
                            var projectNewPath = context.GetTargetPathFromPreviousPath(Path.GetRelativePath(context.RootPath, solutionFile), project.RelativePath);

                            file[i] = line
                                .Replace($"\"{project.ProjectName}\"", $"\"{context.TargetName}\"")
                                .Replace($"\"{project.RelativePath}\"", $"\"{projectNewPath}\"")
                                ;
                            break;
                        }
                    }

                    await File.WriteAllLinesAsync(solutionFile, file);

                    break;
                }
            }
        }

        static bool ShouldExit(CommandLineApplication app, List<string> errors)
        {
            if (errors.Count == 0) return false;

            foreach (var err in errors)
                Error(err);
            app.ShowHelp();

            return true;
        }

        private static bool ShouldUseGit(RunContext context)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = $"ls-files --error {context.ProjectFullPath}",
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
                    context.Logger($"Cannot use git : {error}");
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// Move the source directory to the target directory using : "git mv" command
        /// </summary>
        static void MoveFolderUsingGit(RunContext context)
        {
            var source = Path.GetDirectoryName(context.ProjectFullPath);
            var target = Path.GetDirectoryName(context.TargetFullPath);
            context.Logger($"Moving directory '{source}' to '{target}'.");

            Directory.CreateDirectory(Path.GetDirectoryName(target));

            var arguments = context.HasFileNameChanged()
                ? $"mv \"{Path.Combine(target, context.ProjectFileName)}\" \"{Path.Combine(target, context.TargetFileName)}\" "
                : $"mv \"{source}\" \"{target}\" ";

            using (var gitProcess = Process.Start(new ProcessStartInfo
            {
                FileName = "git",
                Arguments = arguments,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
            }))
            {
                gitProcess.WaitForExit();
                if (gitProcess.ExitCode != 0)
                {
                    var error = gitProcess.StandardError.ReadToEnd();
                    throw new Exception($"Git command failed: '{error}'");
                }
            }
        }

        /// <summary>
        /// Move the source directory to the target directory
        /// </summary>
        static void StandardMoveFolder(RunContext context)
        {
            var source = Path.GetDirectoryName(context.ProjectFullPath);
            var target = Path.GetDirectoryName(context.TargetFullPath);
            context.Logger($"Moving directory '{source}' to '{target}'.");

            foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(source, file);
                var newPath = Path.Combine(target, relativePath);

                Directory.CreateDirectory(Path.GetDirectoryName(newPath));
                File.Move(file, newPath);
            }

            Directory.Delete(source, true);

            if (context.HasFileNameChanged())
            {
                File.Move(Path.Combine(target, context.ProjectFileName), Path.Combine(target, context.TargetFileName));
            }
        }
    }
}
