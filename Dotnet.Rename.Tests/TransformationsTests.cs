using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Dotnet.Rename.Tests
{
    public class TransformationsTests : IDisposable
    {
        private readonly string _sampleSolutionPath;

        public TransformationsTests()
        {
            _sampleSolutionPath = Path.GetRandomFileName();
            var originalSampleSolution = "../../../../_sample";

            foreach (string dirPath in Directory.GetDirectories(originalSampleSolution, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(originalSampleSolution, _sampleSolutionPath));

            foreach (string newPath in Directory.GetFiles(originalSampleSolution, "*", SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(originalSampleSolution, _sampleSolutionPath), true);
        }

        public void Dispose()
        {
            if (Directory.Exists(_sampleSolutionPath))
                Directory.Delete(_sampleSolutionPath, true);
        }

        [Fact]
        public async Task MoveProjectDirectory()
        {
            var parameters = RunParameters.Create(_sampleSolutionPath, "./SampleApp/SampleApp.csproj", "Sample.App", "src");

            await Program.MoveProjectAsync(parameters);
        }

        [Fact]
        public async Task MoveProjectsReferences()
        {
            var parameters = RunParameters.Create(_sampleSolutionPath, "./SampleApp/SampleApp.csproj", "Sample.App", "src");

            await Program.MoveProjectAsync(parameters);
            await Program.MoveProjectsReferencesAsync(parameters);
        }
    }
}
