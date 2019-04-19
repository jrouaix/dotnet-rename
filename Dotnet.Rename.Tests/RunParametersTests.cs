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
        [InlineData("src/test/test.csproj", "test2", "src2", "src/test/test.csproj", "test2", "src2/test2/test2.csproj")]
        [InlineData("src/test/test.csproj", "test2", "src/subfolder", "src/test/test.csproj", "test2", "src/subfolder/test2/test2.csproj")]
        [InlineData("src/test/test.csproj", "test2", "tests", "src/test/test.csproj", "test2", "tests/test2/test2.csproj")]
        [InlineData("test/test.csproj", "test2", null, "test/test.csproj", "test2", "test2/test2.csproj")]
        [InlineData("test/test.csproj", "test2", "src", "test/test.csproj", "test2", "src/test2/test2.csproj")]
        public void Create(string project, string target, string subfolderOptionValue, string expectedProject, string expectedTargetName, string expectedTargetPath)
        {
            var parameters = RunParameters.Create(".", project, target, subfolderOptionValue);

            parameters.RootPath.ShouldBe(".");
            parameters.Project.ShouldBe(expectedProject);
            parameters.TargetName.ShouldBe(expectedTargetName);
            Path.GetFullPath(parameters.TargetPath).ShouldBe(Path.GetFullPath(expectedTargetPath));
        }


        [Theory]
        [InlineData("src/test/test.csproj", "test2", null, "./src/test/test.csproj")]
        [InlineData("src/test/test.csproj", "test2", "src", "./src/test/test.csproj")]
        [InlineData("src/test/test.csproj", "test2", "src2", "./src/test/test.csproj")]
        [InlineData("src/test/test.csproj", "test2", "src/subfolder", "./src/test/test.csproj")]
        [InlineData("src/test/test.csproj", "test2", "tests", "./src/test/test.csproj")]
        [InlineData("test/test.csproj", "test2", null, "./test/test.csproj")]
        [InlineData("test/test.csproj", "test2", "src", "./test/test.csproj")]
        public void GetProjectPathFromRoot(string project, string target, string subfolderOptionValue, string expected)
        {
            var parameters = RunParameters.Create(".", project, target, subfolderOptionValue);
            var path = parameters.Project;

            Path.GetFullPath(path).ShouldBe(Path.GetFullPath(expected));
        }

        [Theory]
        [InlineData("src/test/test.csproj", "test2", null, "./src/test2/test2.csproj")]
        [InlineData("src/test/test.csproj", "test2", "src", "./src/test2/test2.csproj")]
        [InlineData("src/test/test.csproj", "test2", "src2", "./src2/test2/test2.csproj")]
        [InlineData("src/test/test.csproj", "test2", "src/subfolder", "./src/subfolder/test2/test2.csproj")]
        [InlineData("src/test/test.csproj", "test2", "tests", "./tests/test2/test2.csproj")]
        [InlineData("test/test.csproj", "test2", null, "./test2/test2.csproj")]
        [InlineData("test/test.csproj", "test2", "src", "./src/test2/test2.csproj")]
        public void GetTargetPathFromRoot(string project, string target, string subfolderOptionValue, string expected)
        {
            var parameters = RunParameters.Create(".", project, target, subfolderOptionValue);
            var path = parameters.TargetPath;

            Path.GetFullPath(path).ShouldBe(Path.GetFullPath(expected));
        }

        [Theory]
        [InlineData("src/test/test.csproj", "test2", null, "../test2")]
        [InlineData("src/test/test.csproj", "test2", "src", "../test2")]
        [InlineData("src/test/test.csproj", "test2", "src2", "../../src2/test2")]
        [InlineData("src/test/test.csproj", "test2", "src/subfolder", "../subfolder/test2")]
        [InlineData("src/test/test.csproj", "test2", "tests", "../../tests/test2")]
        [InlineData("test/test.csproj", "test2", null, "../test2")]
        [InlineData("test/test.csproj", "test2", "src", "../src/test2")]
        public void GetMove(string project, string target, string subfolderOptionValue, string expected)
        {
            var parameters = RunParameters.Create(".", project, target, subfolderOptionValue);
            var move = parameters.Move;

            Path.GetFullPath(move).ShouldBe(Path.GetFullPath(expected));
        }

        [Theory]
        [InlineData("src/test/test.csproj", "test2", null, "../test")]
        [InlineData("src/test/test.csproj", "test2", "src", "../test")]
        [InlineData("src/test/test.csproj", "test2", "src2", "../../src/test")]
        [InlineData("src/test/test.csproj", "test2", "src/subfolder", "../../test")]
        [InlineData("src/test/test.csproj", "test2", "tests", "../../src/test")]
        [InlineData("test/test.csproj", "test2", null, "../test")]
        [InlineData("test/test.csproj", "test2", "src", "../../test")]
        public void GetInvertedMove(string project, string target, string subfolderOptionValue, string expected)
        {
            var parameters = RunParameters.Create(".", project, target, subfolderOptionValue);
            var move = parameters.InvertedMove;

            Path.GetFullPath(move).ShouldBe(Path.GetFullPath(expected));
        }

        [Theory]
        [InlineData("src/test/test.csproj", "test2", null, "../my/lib.csproj", "../my/lib.csproj")]
        [InlineData("src/test/test.csproj", "test2", "src", "../my/lib.csproj", "../my/lib.csproj")]
        [InlineData("src/test/test.csproj", "test2", "src2", "../my/lib.csproj", "../../src/my/lib.csproj")]
        [InlineData("src/test/test.csproj", "test2", "src/subfolder", "../my/lib.csproj", "../../my/lib.csproj")]
        [InlineData("src/test/test.csproj", "test2", "tests", "../my/lib.csproj", "../../src/my/lib.csproj")]
        [InlineData("test/test.csproj", "test2", null, "../my/lib.csproj", "../my/lib.csproj")]
        [InlineData("test/test.csproj", "test2", "src", "../my/lib.csproj", "../../my/lib.csproj")]
        public void GetRelativePathFromTarget(string project, string target, string subfolderOptionValue, string relativePathFromProject, string expected)
        {
            var parameters = RunParameters.Create(".", project, target, subfolderOptionValue);
            var move = parameters.GetRelativePathFromTarget(relativePathFromProject);

            Path.GetFullPath(move).ShouldBe(Path.GetFullPath(expected));
        }

        [Theory]
        [InlineData("src/test/test.csproj", "test2", null, "../test/test.csproj", "../test2/test2.csproj")]
        [InlineData("src/test/test.csproj", "test2", "src", "../test/test.csproj", "../test2/test2.csproj")]
        [InlineData("src/test/test.csproj", "test2", "src2", "../test/test.csproj", "../../src2/test2/test2.csproj")]
        [InlineData("src/test/test.csproj", "test2", "src/subfolder", "../test/test.csproj", "../subfolder/test2/test2.csproj")]
        [InlineData("src/test/test.csproj", "test2", "tests", "../test/test.csproj", "../../tests/test2/test2.csproj")]
        [InlineData("test/test.csproj", "test2", null, "../../test/test.csproj", "../../test2/test2.csproj")]
        [InlineData("test/test.csproj", "test2", "src", "../../test/test.csproj", "../test2/test2.csproj")]
        public void GetTargetPathFromPreviousPath(string project, string target, string subfolderOptionValue, string relativePathFromProject, string expected)
        {
            var parameters = RunParameters.Create(".", project, target, subfolderOptionValue);
            var move = parameters.GetTargetPathFromPreviousPath("src/project/project.csproj", relativePathFromProject);

            Path.GetFullPath(move).ShouldBe(Path.GetFullPath(expected));
        }
    }
}
