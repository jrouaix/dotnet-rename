using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Dotnet.Rename.Tests
{
    public static class FileSystemHelper
    {
        public static void Copy(string source, string target)
        {
            foreach (string dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(source, target));

            foreach (string newPath in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(source, target), true);
        }
    }
}
