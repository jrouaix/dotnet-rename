using Shouldly;
using System;
using System.IO;
using Xunit;

namespace Dotnet.Rename.Tests
{
    public class ProgramTests
    {
        [Theory]
        [InlineData("src/test/test.csproj", "test2", null, "src/test/test.csproj", "test2", "src/test2/test2.csproj")]
        [InlineData("src/test/test.csproj", "test2", "src", "src/test/test.csproj", "test2", "src/test2/test2.csproj")]
        [InlineData("src/test/test.csproj", "test2", "tests", "src/test/test.csproj", "test2", "tests/test2/test2.csproj")]
        [InlineData("test/test.csproj", "test2", null, "test/test.csproj", "test2", "test2/test2.csproj")]
        [InlineData("test/test.csproj", "test2", "src", "test/test.csproj", "test2", "src/test2/test2.csproj")]
        public void PrepareParameters(string project, string target, string subfolderOptionValue, string expectedProject, string expectedTargetName, string expectedTargetPath)
        {
            var parameters = Program.PrepareParameters(project, target, subfolderOptionValue);

            parameters.Project.ShouldBe(expectedProject);
            parameters.TargetName.ShouldBe(expectedTargetName);
            Path.GetFullPath(parameters.TargetPath).ShouldBe(Path.GetFullPath(expectedTargetPath));
        }
    }
}
