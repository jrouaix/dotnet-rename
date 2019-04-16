using System;
using System.Collections.Generic;
using System.Text;

namespace Dotnet.Rename
{
    public class RunParameters
    {
        public RunParameters(string project, string targetName, string targetPath)
        {
            Project = project;
            TargetName = targetName;
            TargetPath = targetPath;
        }

        public string Project { get; }
        public string TargetName { get; }
        public string TargetPath { get; }
    }
}
