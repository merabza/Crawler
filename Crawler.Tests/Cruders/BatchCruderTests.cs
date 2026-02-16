using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters;
using Crawler.Cruders;
using Crawler.MenuCommands;
using CrawlerDb.Models;
using DoCrawler.Models;
using LibCrawlerRepositories;
using Microsoft.Extensions.Logging;
using Moq;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;
using Xunit;

namespace Crawler.Tests.Cruders;

public sealed class BatchCruderTests
{
    private readonly Mock<ICrawlerRepository> _crawlerRepositoryMock;
    private readonly BatchCruder _batchCruder;

    public BatchCruderTests()
    {
        var loggerMock = new Mock<ILogger>();
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        var crawlerRepositoryCreatorFactoryMock = new Mock<ICrawlerRepositoryCreatorFactory>();
        _crawlerRepositoryMock = new Mock<ICrawlerRepository>();
        var crawlerParameters = new CrawlerParameters();

        crawlerRepositoryCreatorFactoryMock
            .Setup(x => x.GetCrawlerRepository())
            .Returns(_crawlerRepositoryMock.Object);

        _batchCruder = new BatchCruder(
            loggerMock.Object,
            httpClientFactoryMock.Object,
            crawlerRepositoryCreatorFactoryMock.Object,
            crawlerParameters
        );
    }

    [Fact]
    public void Constructor_ShouldInitializeWithCorrectProperties()
    {
        // Assert
        Assert.Equal("Batch", _batchCruder.CrudName);
        Assert.Equal("Batches", _batchCruder.CrudNamePlural);
    }

    [Fact]
    public void GetCrudersDictionary_ShouldReturnDictionaryOfBatches()
    {
        // Arrange
        var batches = new List<Batch>
        {
            new() { BatchId = 1, BatchName = "Batch1", IsOpen = true, AutoCreateNextPart = false },
            new() { BatchId = 2, BatchName = "Batch2", IsOpen = false, AutoCreateNextPart = true }
        };

        _crawlerRepositoryMock
            .Setup(x => x.GetBatchesList())
            .Returns(batches);

        // Act
        var result = InvokeProtectedMethod<Dictionary<string, ItemData>>(_batchCruder, "GetCrudersDictionary");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.True(result.ContainsKey("Batch1"));
        Assert.True(result.ContainsKey("Batch2"));
        Assert.IsType<Batch>(result["Batch1"]);
    }

    [Fact]
    public void ContainsRecordWithKey_WhenKeyExists_ShouldReturnTrue()
    {
        // Arrange
        var batches = new List<Batch>
        {
            new() { BatchId = 1, BatchName = "TestBatch", IsOpen = true, AutoCreateNextPart = false }
        };

        _crawlerRepositoryMock
            .Setup(x => x.GetBatchesList())
            .Returns(batches);

        // Act
        var result = _batchCruder.ContainsRecordWithKey("TestBatch");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ContainsRecordWithKey_WhenKeyDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var batches = new List<Batch>
        {
            new() { BatchId = 1, BatchName = "TestBatch", IsOpen = true, AutoCreateNextPart = false }
        };

        _crawlerRepositoryMock
            .Setup(x => x.GetBatchesList())
            .Returns(batches);

        // Act
        var result = _batchCruder.ContainsRecordWithKey("NonExistentBatch");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void UpdateRecordWithKey_WithValidBatch_ShouldUpdateBatchName()
    {
        // Arrange
        var existingBatch = new Batch
        {
            BatchId = 1,
            BatchName = "OldBatchName",
            IsOpen = true,
            AutoCreateNextPart = false
        };

        var updatedBatch = new Batch
        {
            BatchId = 1,
            BatchName = "NewBatchName",
            IsOpen = false,
            AutoCreateNextPart = true
        };

        _crawlerRepositoryMock
            .Setup(x => x.GetBatchByName("OldBatchName"))
            .Returns(existingBatch);

        _crawlerRepositoryMock
            .Setup(x => x.UpdateBatch(It.IsAny<Batch>()))
            .Returns(existingBatch);

        // Act
        _batchCruder.UpdateRecordWithKey("OldBatchName", updatedBatch);

        // Assert
        _crawlerRepositoryMock.Verify(x => x.GetBatchByName("OldBatchName"), Times.Once);
        _crawlerRepositoryMock.Verify(x => x.UpdateBatch(It.Is<Batch>(b => b.BatchName == "NewBatchName")), Times.Once);
        _crawlerRepositoryMock.Verify(x => x.SaveChanges(), Times.Once);
    }

    [Fact]
    public void UpdateRecordWithKey_WithNullBatch_ShouldThrowException()
    {
        // Arrange
        var updatedBatch = new Batch
        {
            BatchId = 1,
            BatchName = "NewBatchName",
            IsOpen = false,
            AutoCreateNextPart = true
        };

        _crawlerRepositoryMock
            .Setup(x => x.GetBatchByName("NonExistent"))
            .Returns((Batch?)null);

        // Act & Assert
        Assert.Throws<Exception>(() => _batchCruder.UpdateRecordWithKey("NonExistent", updatedBatch));
    }

    [Fact]
    public void UpdateRecordWithKey_WithNonBatchItemData_ShouldNotUpdate()
    {
        // Arrange
        var nonBatchItem = new TextItemData { Text = "NotABatch" };

        // Act
        _batchCruder.UpdateRecordWithKey("SomeKey", nonBatchItem);

        // Assert
        _crawlerRepositoryMock.Verify(x => x.GetBatchByName(It.IsAny<string>()), Times.Never);
        _crawlerRepositoryMock.Verify(x => x.UpdateBatch(It.IsAny<Batch>()), Times.Never);
        _crawlerRepositoryMock.Verify(x => x.SaveChanges(), Times.Never);
    }

    [Fact]
    public void AddRecordWithKey_WithValidBatch_ShouldCreateNewBatch()
    {
        // Arrange
        var newBatch = new Batch
        {
            BatchId = 0,
            BatchName = "NewBatch",
            IsOpen = true,
            AutoCreateNextPart = false
        };

        _crawlerRepositoryMock
            .Setup(x => x.CreateBatch(It.IsAny<Batch>()))
            .Returns(newBatch);

        // Act
        InvokeProtectedMethod(_batchCruder, "AddRecordWithKey", "NewBatch", newBatch);

        // Assert
        _crawlerRepositoryMock.Verify(x => x.CreateBatch(It.Is<Batch>(b => b.BatchName == "NewBatch")), Times.Once);
        _crawlerRepositoryMock.Verify(x => x.SaveChanges(), Times.Once);
    }

    [Fact]
    public void AddRecordWithKey_WithNonBatchItemData_ShouldNotCreate()
    {
        // Arrange
        var nonBatchItem = new TextItemData { Text = "NotABatch" };

        // Act
        InvokeProtectedMethod(_batchCruder, "AddRecordWithKey", "SomeKey", nonBatchItem);

        // Assert
        _crawlerRepositoryMock.Verify(x => x.CreateBatch(It.IsAny<Batch>()), Times.Never);
        _crawlerRepositoryMock.Verify(x => x.SaveChanges(), Times.Never);
    }

    [Fact]
    public void RemoveRecordWithKey_WhenBatchExists_ShouldDeleteBatch()
    {
        // Arrange
        var existingBatch = new Batch
        {
            BatchId = 1,
            BatchName = "BatchToDelete",
            IsOpen = true,
            AutoCreateNextPart = false
        };

        _crawlerRepositoryMock
            .Setup(x => x.GetBatchByName("BatchToDelete"))
            .Returns(existingBatch);

        _crawlerRepositoryMock
            .Setup(x => x.DeleteBatch(It.IsAny<Batch>()))
            .Returns(existingBatch);

        // Act
        InvokeProtectedMethod(_batchCruder, "RemoveRecordWithKey", "BatchToDelete");

        // Assert
        _crawlerRepositoryMock.Verify(x => x.GetBatchByName("BatchToDelete"), Times.Once);
        _crawlerRepositoryMock.Verify(x => x.DeleteBatch(existingBatch), Times.Once);
        _crawlerRepositoryMock.Verify(x => x.SaveChanges(), Times.Once);
    }

    [Fact]
    public void RemoveRecordWithKey_WhenBatchDoesNotExist_ShouldNotDelete()
    {
        // Arrange
        _crawlerRepositoryMock
            .Setup(x => x.GetBatchByName("NonExistent"))
            .Returns((Batch?)null);

        // Act
        InvokeProtectedMethod(_batchCruder, "RemoveRecordWithKey", "NonExistent");

        // Assert
        _crawlerRepositoryMock.Verify(x => x.GetBatchByName("NonExistent"), Times.Once);
        _crawlerRepositoryMock.Verify(x => x.DeleteBatch(It.IsAny<Batch>()), Times.Never);
        _crawlerRepositoryMock.Verify(x => x.SaveChanges(), Times.Never);
    }

    [Fact]
    public void CreateNewItem_WithRecordKey_ShouldReturnBatchWithKey()
    {
        // Arrange
        const string recordKey = "TestBatchKey";

        // Act
        var result = InvokeProtectedMethod<ItemData>(_batchCruder, "CreateNewItem", recordKey, null);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<Batch>(result);
        var batch = (Batch)result;
        Assert.Equal(recordKey, batch.BatchName);
    }

    [Fact]
    public void CreateNewItem_WithNullRecordKey_ShouldReturnBatchWithEmptyName()
    {
        // Act
        var result = InvokeProtectedMethod<ItemData>(_batchCruder, "CreateNewItem", null, null);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<Batch>(result);
        var batch = (Batch)result;
        Assert.Equal(string.Empty, batch.BatchName);
    }

    [Fact]
    public void FillDetailsSubMenu_ShouldAddMenuItems()
    {
        // Arrange
        var batch = new Batch
        {
            BatchId = 1,
            BatchName = "TestBatch",
            IsOpen = true,
            AutoCreateNextPart = false
        };

        var batches = new List<Batch> { batch };

        _crawlerRepositoryMock
            .Setup(x => x.GetBatchesList())
            .Returns(batches);

        _crawlerRepositoryMock
            .Setup(x => x.GetHostStartUrlNamesByBatch(It.IsAny<Batch>()))
            .Returns(["host1.com", "host2.com"]);

        var menuSet = new CliMenuSet("Test Menu");

        // Act
        _batchCruder.FillDetailsSubMenu(menuSet, "TestBatch");

        // Assert
        // Verify that the method completed without throwing exceptions
        // The menu is populated internally, we can't inspect it directly
        _crawlerRepositoryMock.Verify(x => x.GetBatchesList(), Times.Once);
        _crawlerRepositoryMock.Verify(x => x.GetHostStartUrlNamesByBatch(It.IsAny<Batch>()), Times.Once);
    }

    // Helper method to invoke protected methods using reflection
    private T? InvokeProtectedMethod<T>(object obj, string methodName, params object?[] parameters)
    {
        var method = obj.GetType().GetMethod(methodName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (method is null)
        {
            throw new InvalidOperationException($"Method '{methodName}' not found");
        }

        return (T?)method.Invoke(obj, parameters);
    }

    private void InvokeProtectedMethod(object obj, string methodName, params object?[] parameters)
    {
        var method = obj.GetType().GetMethod(methodName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (method is null)
        {
            throw new InvalidOperationException($"Method '{methodName}' not found");
        }

        method.Invoke(obj, parameters);
    }
}
