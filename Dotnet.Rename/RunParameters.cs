using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Dotnet.Rename
{
    public class RunParameters
    {
        public RunParameters(string rootPath, string project, string targetName, string targetPath)
        {
            RootPath = rootPath;
            Project = project;
            TargetName = targetName;
            TargetPath = targetPath;
        }

        public string RootPath { get; }
        public string Project { get; }
        public string TargetName { get; }
        public string TargetPath { get; }


        public string GetTargetPathFromRoot() => Path.Combine(RootPath, TargetPath);
        public string GetProjectPathFromRoot() => Path.Combine(RootPath, Project);


        public override string ToString() => $"{GetProjectPathFromRoot()} => {GetTargetPathFromRoot()}";
    }
}
