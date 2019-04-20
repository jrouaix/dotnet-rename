using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Dotnet.Rename
{
    public static class PathHelper
    {
        public static string P(string path) => path?.Replace('\\', '/');
    }
}
