﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Dotnet.Rename.Tests
{
    public class TransformationsTests : IDisposable
    {
        private readonly string _sampleSolutionPath;
        private readonly ITestOutputHelper _output;

        public TransformationsTests(ITestOutputHelper output)
        {
            _output = output;

            _sampleSolutionPath = Path.GetRandomFileName();
            var originalSampleSolution = "../../../../_sample";
            FileSystemHelper.Copy(originalSampleSolution, _sampleSolutionPath);
        }

        public void Dispose()
        {
            if (Directory.Exists(_sampleSolutionPath))
                Directory.Delete(_sampleSolutionPath, true);
        }

        [Fact]
        public async Task MoveProject()
        {
            var parameters = RunContext.Create(_sampleSolutionPath, "./SampleApp/SampleApp.csproj", "Sample.App", "src", logger: _output.WriteLine);

            Program.MoveProject(parameters);
        }

        [Fact]
        public async Task ChangeProjectsReferences()
        {
            var parameters = RunContext.Create(_sampleSolutionPath, "./SampleApp/SampleApp.csproj", "Sample.App", "src", logger: _output.WriteLine);

            Program.MoveProject(parameters);
            await Program.ChangeProjectsReferencesAsync(parameters);
        }

        [Fact]
        public async Task ChangeSolutionsReferencesAsync()
        {
            var parameters = RunContext.Create(_sampleSolutionPath, "./SampleApp/SampleApp.csproj", "Sample.App", "src", logger: _output.WriteLine);

            Program.MoveProject(parameters);
            await Program.ChangeSolutionsReferencesAsync(parameters);
        }

        [Fact]
        public async Task RunAsync()
        {
            var parameters = RunContext.Create(_sampleSolutionPath, "./SampleApp/SampleApp.csproj", "Sample.App", "src", logger: _output.WriteLine);
            await Program.RunAsync(parameters);
        }

        [Fact]
        public async Task RunAsync_MultipleChanges()
        {
            await Program.RunAsync(RunContext.Create(_sampleSolutionPath, "./SampleApp/SampleApp.csproj", "Sample.App", "src", logger: _output.WriteLine));
            await Program.RunAsync(RunContext.Create(_sampleSolutionPath, "./SampleLib/SampleLib.csproj", "SampleLib", "src", logger: _output.WriteLine));
            await Program.RunAsync(RunContext.Create(_sampleSolutionPath, "./src/SampleLib/SampleLib.csproj", "Sample.Lib.csproj", logger: _output.WriteLine));
            await Program.RunAsync(RunContext.Create(_sampleSolutionPath, "./tests/SampleTests/SampleTests.csproj", "Sample.Tests", logger: _output.WriteLine));
            await Program.RunAsync(RunContext.Create(_sampleSolutionPath, "./src/Sample.App/Sample.App.csproj", "Sample2.App", "src", logger: _output.WriteLine));
        }
    }
}
