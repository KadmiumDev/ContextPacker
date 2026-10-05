// Copyright (c) 2026 Kadmium (Emil Fredrik SjÃ¶stedt). All rights reserved.
// Licensed under the Kadmium Software & Source Code License Agreement.
// See LICENSE.txt in the project root for full license terms.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Packer.Core.Interfaces;
using Packer.Core.Models;
using Packer.Core.Services;
using Xunit;

namespace Packer.Core.Tests
{
    public class TestPackerConfig : IPackerConfig
    {
        public string AllowedExtensions { get; set; } = ".cs,.txt";
        public string IgnoredFolders { get; set; } = "bin,obj";
        public string ModuleFolderAnchors { get; set; } = "Plugins,repos,Source,Projects";
        public bool RedactPrivateInformation { get; set; } = true;
        public string CacheLocation { get; set; } = Path.Combine(Path.GetTempPath(), "PackerTestsCache");
        public int LargeFileWarningThreshold { get; set; } = 500;
        public int MaxCharsPerFile { get; set; } = 3000000;

        public string GetActualCachePath() => CacheLocation;
    }

    public class PackerEngineTests : IDisposable
    {
        private readonly PackerEngine _engine = new();
        private readonly TestPackerConfig _config = new();
        private readonly string _tempDir;

        public PackerEngineTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "PackerEngineTests_" + Guid.NewGuid());
            Directory.CreateDirectory(_tempDir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);

            if (Directory.Exists(_config.CacheLocation))
                Directory.Delete(_config.CacheLocation, true);
        }

        [Theory]
        [InlineData(@"C:\Git\repos\CoreModule\File.cs", "Plugins,repos,Projects", "CoreModule")]
        [InlineData(@"C:\Engine\Source\Runtime\Actor.cpp", "Source,Plugins", "Engine")] // Source tar parent
        [InlineData(@"C:\RandomFolder\OtherFolder\Simple.cs", "Plugins", "OtherFolder")] // Fallback
        public void GetModuleName_ExtractsCorrectModule(string path, string anchors, string expectedModule)
        {
            string module = _engine.GetModuleName(path, anchors);
            Assert.Equal(expectedModule, module);
        }

        [Fact]
        public async Task PackFilesAsync_GeneratesTxtFile_WithExpectedMarkdown()
        {
            // Arrange: Skapa en testfil
            string filePath = Path.Combine(_tempDir, "TestClass.cs");
            await File.WriteAllTextAsync(filePath, "public class TestClass {}");

            var files = new List<FileItemModel>
            {
                new() { FileName = "TestClass.cs", FilePath = filePath }
            };

            // Act
            var result = await _engine.PackFilesAsync(
                files,
                baseName: "UnitTest_Context",
                config: _config,
                missionContext: "Test run",
                isObsidianFormat: false);

            // Assert
            Assert.NotEmpty(result.Files);
            string generatedContent = await File.ReadAllTextAsync(result.Files[0].FullPath);

            Assert.Contains("## Mission Context", generatedContent);
            Assert.Contains("Test run", generatedContent);
            Assert.Contains("```csharp", generatedContent);
            Assert.Contains("public class TestClass {}", generatedContent);
        }

        [Fact]
        public async Task PackFilesAsync_WarnsWhenThresholdExceeded()
        {
            // Arrange: Skapa en fil som överskrider tröskeln (LargeFileWarningThreshold = 500)
            string filePath = Path.Combine(_tempDir, "LargeFile.cs");
            await File.WriteAllTextAsync(filePath, new string('A', 600));

            var files = new List<FileItemModel>
            {
                new() { FileName = "LargeFile.cs", FilePath = filePath }
            };

            // Act
            var result = await _engine.PackFilesAsync(
                files,
                baseName: "Warning_Context",
                config: _config,
                missionContext: string.Empty,
                isObsidianFormat: false);

            // Assert
            Assert.Single(result.Warnings);
            Assert.Contains("is very large", result.Warnings[0]);
        }
    }
}