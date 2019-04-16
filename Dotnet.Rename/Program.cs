using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.IO;

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
            var subfolderOption = app.Option(SUBFOLDER_OPTION, "Enter the path (relative or absolute) to a subfolder where the new project (and project folder) will be placed.", CommandOptionType.SingleValue);


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

                var parameters = PrepareParameters(
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

        public static RunParameters PrepareParameters(string project, string target, string subfolderOptionValue)
        {
            if (string.Compare(Path.GetExtension(target), Path.GetExtension(project), StringComparison.InvariantCultureIgnoreCase) != 0)
                target += Path.GetExtension(project);

            var targetName = Path.GetFileNameWithoutExtension(target);

            var rootFolder = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetFullPath(project)));

            var targetPath = Path.Combine(subfolderOptionValue ?? rootFolder, targetName, target);

            return new RunParameters(project, targetName, targetPath);
        }

        static bool ShouldExit(CommandLineApplication app, List<string> errors)
        {
            if (errors.Count == 0) return true;

            foreach (var err in errors)
                Console.Error.WriteLine(err);
            app.ShowHelp();

            return false;
        }
    }
}
