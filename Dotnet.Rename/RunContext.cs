using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static Dotnet.Rename.PathHelper;

namespace Dotnet.Rename
{
    public class RunContext
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rootFolder">Root folder of the repository</param>
        /// <param name="project"></param>
        /// <param name="target"></param>
        /// <param name="subfolderOptionValue"></param>
        public static RunContext Create(string rootFolder, string project, string target, string subfolderOptionValue = null, Action<string> logger = null)
        {
            var projectFileName = Path.GetFileName(project);

            if (string.Compare(Path.GetExtension(target), Path.GetExtension(project), StringComparison.InvariantCultureIgnoreCase) != 0)
                target += Path.GetExtension(project);

            var targetFileName = target;
            var targetName = Path.GetFileNameWithoutExtension(target);

            var projectUpperFolder = Path.GetDirectoryName(Path.GetDirectoryName(P(project)));

            var targetPath = Path.Combine(P(subfolderOptionValue ?? projectUpperFolder), targetName, target);

            return new RunContext(rootFolder, project, projectFileName, targetName, targetFileName, targetPath, logger ?? (s => { }));
        }

        private RunContext(string rootPath, string project, string projectFileName, string targetName, string targetFileName, string targetPath, Action<string> logger)
        {
            RootPath = P(rootPath);

            Project = P(project);
            ProjectFileName = projectFileName;

            TargetName = targetName;
            TargetFileName = targetFileName;

            TargetPath = P(targetPath);
            Logger = logger;

            Move = P(Path.GetRelativePath(Path.GetDirectoryName(P(Project)), Path.GetDirectoryName(P(TargetPath))));
            InvertedMove = P(Path.GetRelativePath(Path.GetDirectoryName(P(TargetPath)), Path.GetDirectoryName(P(Project))));
        }

        /// <summary>
        /// Root path of the execution
        /// </summary>
        public string RootPath { get; }
        /// <summary>
        /// Original csproj file path (relative to the RootPath)
        /// </summary>
        public string Project { get; }
        /// <summary>
        /// Project file name only
        /// </summary>
        public string ProjectFileName { get; }
        public string ProjectFullPath { get => P(Path.Combine(RootPath, Project)); }
        /// <summary>
        /// New project name
        /// </summary>
        public string TargetName { get; }
        /// <summary>
        /// New project file name
        /// </summary>
        public string TargetFileName { get; }
        /// <summary>
        /// New csproj file path (relative to the RootPath)
        /// </summary>
        public string TargetPath { get; }
        public string TargetFullPath { get => P(Path.Combine(RootPath, TargetPath)); }

        public string Move { get; }
        public string InvertedMove { get; }

        public Action<string> Logger { get; }

        public string GetRelativePathFromTarget(string relativePathFromProject)
        {
            var movedRelative = Path.Combine(InvertedMove, relativePathFromProject);
            var targetDirectory = Path.GetDirectoryName(TargetPath);
            var fullyAppliedPath = Path.Combine(targetDirectory, movedRelative);

            var newRelative = P(Path.GetRelativePath(targetDirectory, fullyAppliedPath));
            return newRelative;
        }

        public string GetTargetPathFromPreviousPath(string projectpath, string relativePathFromProject)
        {
            var projectDirectory = Path.GetDirectoryName(projectpath);
            var relativeDirectory = Path.Combine(projectDirectory, Path.GetDirectoryName(relativePathFromProject));
            var fullyAppliedPath = Path.Combine(relativeDirectory, Move, TargetFileName);

            if (string.IsNullOrWhiteSpace(projectDirectory))
                projectDirectory = ".";

            var newRelative = P(Path.GetRelativePath(projectDirectory, fullyAppliedPath));
            return newRelative;
        }



        public override string ToString() => $"{Project} => {TargetPath}";
    }
}
