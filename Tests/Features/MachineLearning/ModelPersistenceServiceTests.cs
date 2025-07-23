using FluentAssertions;
using Microsoft.ML;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.MachineLearning.Services.Filesystem;
using System.IO.Abstractions.TestingHelpers;
using Tests.TestHelpers;

namespace Tests.Features.MachineLearning.Services.Filesystem
{
    [TestFixture]
    public class ModelPersistenceServiceTests : TestBase
    {
        private ModelPersistenceService _service;
        private MockFileSystem _mockFileSystem;
        private MLContext _mlContext;
        private ITransformer _mockModel;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _mockFileSystem = new MockFileSystem();
            _service = new ModelPersistenceService(_mockFileSystem);
            _mlContext = new MLContext(seed: 42);
            _mockModel = Substitute.For<ITransformer>();
        }

        #region Constructor Tests

        [Test]
        public void Constructor_WithValidFileSystem_InitializesCorrectly()
        {
            _service.Should().NotBeNull();
        }

        #endregion

        #region ModelExists Tests

        [Test]
        public void ModelExists_WhenFileExists_ReturnsTrue()
        {
            // Arrange
            var modelPath = @"c:\models\test-model.zip";
            _mockFileSystem.AddFile(modelPath, new MockFileData("fake model data"));

            // Act
            var result = _service.ModelExists(modelPath);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void ModelExists_WhenFileDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var modelPath = @"c:\models\non-existent-model.zip";

            // Act
            var result = _service.ModelExists(modelPath);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void ModelExists_WithNullPath_ReturnsFalse()
        {
            // Act
            var result = _service.ModelExists(null);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void ModelExists_WithEmptyPath_ReturnsFalse()
        {
            // Act
            var result = _service.ModelExists("");

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void ModelExists_WithDirectory_ReturnsFalse()
        {
            // Arrange
            var directoryPath = @"c:\models\";
            _mockFileSystem.AddDirectory(directoryPath);

            // Act
            var result = _service.ModelExists(directoryPath);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region GetModelFileInfo Tests

        [Test]
        public void GetModelFileInfo_WhenFileExists_ReturnsCorrectInfo()
        {
            // Arrange
            var modelPath = @"c:\models\test-model.zip";
            var fileContent = "fake model data with some content to test file size";
            var lastWriteTime = new DateTime(2023, 10, 15, 14, 30, 0);
            
            var mockFileData = new MockFileData(fileContent)
            {
                LastWriteTime = lastWriteTime
            };
            _mockFileSystem.AddFile(modelPath, mockFileData);

            // Act
            var result = _service.GetModelFileInfo(modelPath);

            // Assert
            result.Should().NotBeNull();
            result.Exists.Should().BeTrue();
            result.LastModified.Should().Be(lastWriteTime);
            result.FileSize.Should().Be(fileContent.Length);
        }

        [Test]
        public void GetModelFileInfo_WhenFileDoesNotExist_ReturnsNonExistentInfo()
        {
            // Arrange
            var modelPath = @"c:\models\non-existent-model.zip";

            // Act
            var result = _service.GetModelFileInfo(modelPath);

            // Assert
            result.Should().NotBeNull();
            result.Exists.Should().BeFalse();
            result.LastModified.Should().BeNull();
            result.FileSize.Should().Be(0);
        }

        [Test]
        public void GetModelFileInfo_WithEmptyFile_ReturnsZeroFileSize()
        {
            // Arrange
            var modelPath = @"c:\models\empty-model.zip";
            _mockFileSystem.AddFile(modelPath, new MockFileData(""));

            // Act
            var result = _service.GetModelFileInfo(modelPath);

            // Assert
            result.Should().NotBeNull();
            result.Exists.Should().BeTrue();
            result.FileSize.Should().Be(0);
        }

        [Test]
        public void GetModelFileInfo_WithLargeFile_ReturnsCorrectFileSize()
        {
            // Arrange
            var modelPath = @"c:\models\large-model.zip";
            var largeContent = new string('x', 1024 * 1024); // 1MB of data
            _mockFileSystem.AddFile(modelPath, new MockFileData(largeContent));

            // Act
            var result = _service.GetModelFileInfo(modelPath);

            // Assert
            result.Should().NotBeNull();
            result.Exists.Should().BeTrue();
            result.FileSize.Should().Be(1024 * 1024);
        }

        #endregion

        #region LoadModelAsync Tests

        [Test]
        public async Task LoadModelAsync_WhenFileExists_ReturnsModel()
        {
            // Arrange
            var modelPath = @"c:\models\test-model.zip";
            
            // Create a simple model to serialize
            var dataView = _mlContext.Data.LoadFromEnumerable(new[]
            {
                new { Value = 1.0f },
                new { Value = 2.0f }
            });
            
            var pipeline = _mlContext.Transforms.Concatenate("Features", "Value");
            var model = pipeline.Fit(dataView);
            
            // Save the model to get real model data
            using var memoryStream = new MemoryStream();
            _mlContext.Model.Save(model, dataView.Schema, memoryStream);
            var modelData = memoryStream.ToArray();
            
            _mockFileSystem.AddFile(modelPath, new MockFileData(modelData));

            // Act
            var result = await _service.LoadModelAsync(modelPath, _mlContext);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<ITransformer>();
        }

        [Test]
        public async Task LoadModelAsync_WhenFileDoesNotExist_ReturnsNull()
        {
            // Arrange
            var modelPath = @"c:\models\non-existent-model.zip";

            // Act
            var result = await _service.LoadModelAsync(modelPath, _mlContext);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task LoadModelAsync_WithNullPath_ReturnsNull()
        {
            // Act
            var result = await _service.LoadModelAsync(null, _mlContext);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task LoadModelAsync_WithEmptyFile_ThrowsException()
        {
            // Arrange
            var modelPath = @"c:\models\empty-model.zip";
            _mockFileSystem.AddFile(modelPath, new MockFileData(""));

            // Act & Assert
            var act = async () => await _service.LoadModelAsync(modelPath, _mlContext);
            await act.Should().ThrowAsync<Exception>();
        }

        [Test]
        public async Task LoadModelAsync_WithInvalidModelData_ThrowsException()
        {
            // Arrange
            var modelPath = @"c:\models\invalid-model.zip";
            _mockFileSystem.AddFile(modelPath, new MockFileData("invalid model data"));

            // Act & Assert
            var act = async () => await _service.LoadModelAsync(modelPath, _mlContext);
            await act.Should().ThrowAsync<Exception>();
        }

        #endregion

        #region SaveModelAsync Tests

        [Test]
        public async Task SaveModelAsync_WithValidModel_CreatesFile()
        {
            // Arrange
            var modelPath = @"c:\models\new-model.zip";
            
            // Create a simple model
            var dataView = _mlContext.Data.LoadFromEnumerable(new[]
            {
                new { Value = 1.0f },
                new { Value = 2.0f }
            });
            
            var pipeline = _mlContext.Transforms.Concatenate("Features", "Value");
            var model = pipeline.Fit(dataView);

            // Act
            await _service.SaveModelAsync(model, modelPath, _mlContext);

            // Assert
            _mockFileSystem.File.Exists(modelPath).Should().BeTrue();
            var fileData = _mockFileSystem.GetFile(modelPath);
            fileData.Contents.Length.Should().BeGreaterThan(0);
        }

        [Test]
        public async Task SaveModelAsync_WhenDirectoryDoesNotExist_CreatesDirectory()
        {
            // Arrange
            var modelPath = @"c:\new-folder\subdir\model.zip";
            
            var dataView = _mlContext.Data.LoadFromEnumerable(new[]
            {
                new { Value = 1.0f }
            });
            
            var pipeline = _mlContext.Transforms.Concatenate("Features", "Value");
            var model = pipeline.Fit(dataView);

            // Act
            await _service.SaveModelAsync(model, modelPath, _mlContext);

            // Assert
            _mockFileSystem.Directory.Exists(@"c:\new-folder\subdir").Should().BeTrue();
            _mockFileSystem.File.Exists(modelPath).Should().BeTrue();
        }

        [Test]
        public async Task SaveModelAsync_WhenFileAlreadyExists_OverwritesFile()
        {
            // Arrange
            var modelPath = @"c:\models\existing-model.zip";
            _mockFileSystem.AddFile(modelPath, new MockFileData("old model data"));
            
            var dataView = _mlContext.Data.LoadFromEnumerable(new[]
            {
                new { Value = 1.0f }
            });
            
            var pipeline = _mlContext.Transforms.Concatenate("Features", "Value");
            var model = pipeline.Fit(dataView);

            var originalSize = _mockFileSystem.GetFile(modelPath).Contents.Length;

            // Act
            await _service.SaveModelAsync(model, modelPath, _mlContext);

            // Assert
            _mockFileSystem.File.Exists(modelPath).Should().BeTrue();
            var newFileData = _mockFileSystem.GetFile(modelPath);
            newFileData.Contents.Length.Should().NotBe(originalSize);
            newFileData.Contents.Length.Should().BeGreaterThan(0);
        }

        [Test]
        public async Task SaveModelAsync_WithComplexPath_HandlesProperly()
        {
            // Arrange
            var modelPath = @"c:\models\deep\nested\folder\structure\model.zip";
            
            var dataView = _mlContext.Data.LoadFromEnumerable(new[]
            {
                new { Value = 1.0f }
            });
            
            var pipeline = _mlContext.Transforms.Concatenate("Features", "Value");
            var model = pipeline.Fit(dataView);

            // Act
            await _service.SaveModelAsync(model, modelPath, _mlContext);

            // Assert
            _mockFileSystem.Directory.Exists(@"c:\models\deep\nested\folder\structure").Should().BeTrue();
            _mockFileSystem.File.Exists(modelPath).Should().BeTrue();
        }

        [Test]
        public async Task SaveModelAsync_SaveAndLoad_RoundTripSucceeds()
        {
            // Arrange
            var modelPath = @"c:\models\roundtrip-model.zip";
            
            var dataView = _mlContext.Data.LoadFromEnumerable(new[]
            {
                new { Value = 1.0f },
                new { Value = 2.0f },
                new { Value = 3.0f }
            });
            
            var pipeline = _mlContext.Transforms.Concatenate("Features", "Value");
            var originalModel = pipeline.Fit(dataView);

            // Act
            await _service.SaveModelAsync(originalModel, modelPath, _mlContext);
            var loadedModel = await _service.LoadModelAsync(modelPath, _mlContext);

            // Assert
            loadedModel.Should().NotBeNull();
            loadedModel.Should().BeAssignableTo<ITransformer>();
            
            // Verify the file exists and has content
            _mockFileSystem.File.Exists(modelPath).Should().BeTrue();
            var fileInfo = _service.GetModelFileInfo(modelPath);
            fileInfo.Exists.Should().BeTrue();
            fileInfo.FileSize.Should().BeGreaterThan(0);
        }

        #endregion

        #region Integration Tests

        [Test]
        public async Task FullWorkflow_SaveLoadAndGetInfo_WorksCorrectly()
        {
            // Arrange
            var modelPath = @"c:\models\workflow-test-model.zip";
            
            var dataView = _mlContext.Data.LoadFromEnumerable(new[]
            {
                new { Input = 1.0f, Label = true },
                new { Input = 2.0f, Label = false },
                new { Input = 3.0f, Label = true }
            });
            
            var pipeline = _mlContext.Transforms.Concatenate("Features", "Input")
                .Append(_mlContext.BinaryClassification.Trainers.SdcaLogisticRegression());
            var model = pipeline.Fit(dataView);

            // Act & Assert

            // 1. Initially model doesn't exist
            _service.ModelExists(modelPath).Should().BeFalse();
            var initialInfo = _service.GetModelFileInfo(modelPath);
            initialInfo.Exists.Should().BeFalse();

            // 2. Save the model
            await _service.SaveModelAsync(model, modelPath, _mlContext);

            // 3. Now model should exist
            _service.ModelExists(modelPath).Should().BeTrue();
            var savedInfo = _service.GetModelFileInfo(modelPath);
            savedInfo.Exists.Should().BeTrue();
            savedInfo.FileSize.Should().BeGreaterThan(0);
            savedInfo.LastModified.Should().NotBeNull();

            // 4. Load the model
            var loadedModel = await _service.LoadModelAsync(modelPath, _mlContext);
            loadedModel.Should().NotBeNull();
            loadedModel.Should().BeAssignableTo<ITransformer>();
        }

        #endregion
    }
} 