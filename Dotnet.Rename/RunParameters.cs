using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Dotnet.Rename
{
    public class RunParameters
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rootFolder">Root folder of the repository</param>
        /// <param name="project"></param>
        /// <param name="target"></param>
        /// <param name="subfolderOptionValue"></param>
        /// <returns></returns>
        public static RunParameters Create(string rootFolder, string project, string target, string subfolderOptionValue)
        {
            if (string.Compare(Path.GetExtension(target), Path.GetExtension(project), StringComparison.InvariantCultureIgnoreCase) != 0)
                target += Path.GetExtension(project);

            var targetName = Path.GetFileNameWithoutExtension(target);

            var projectUpperFolder = Path.GetDirectoryName(Path.GetDirectoryName(project));

            var targetPath = Path.Combine(".", subfolderOptionValue ?? projectUpperFolder, targetName, target);

            return new RunParameters(rootFolder, project, targetName, targetPath);
        }

        private RunParameters(string rootPath, string project, string targetName, string targetPath)
        {
            RootPath = rootPath;
            Project = project;
            TargetName = targetName;
            TargetPath = targetPath;

            Move = Path.GetRelativePath(Path.GetDirectoryName(Project), Path.GetDirectoryName(TargetPath));
            InvertedMove = Path.GetRelativePath(Path.GetDirectoryName(TargetPath), Path.GetDirectoryName(Project));
        }

        /// <summary>
        /// Root path of the execution
        /// </summary>
        public string RootPath { get; }
        /// <summary>
        /// Original csproj file path (relative to the RootPath)
        /// </summary>
        public string Project { get; }
        public string ProjectFullPath { get => Path.Combine(RootPath, Project); }
        /// <summary>
        /// New project name
        /// </summary>
        public string TargetName { get; }
        /// <summary>
        /// New csproj file path (relative to the RootPath)
        /// </summary>
        public string TargetPath { get; }
        public string TargetFullPath { get => Path.Combine(RootPath, TargetPath); }

        public string Move { get; }
        public string InvertedMove { get; }

        public string GetRelativePathFromTarget(string relativePathFromProject)
        {
            var movedRelative = Path.Combine(InvertedMove, relativePathFromProject);
            string targetDirectory = Path.GetDirectoryName(TargetPath);
            var fullyAppliedPath = Path.Combine(targetDirectory, movedRelative);

            var newRelative = Path.GetRelativePath(targetDirectory, fullyAppliedPath);
            return newRelative;
        }


        public override string ToString() => $"{Project} => {TargetPath}";
    }
}
