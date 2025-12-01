using Xunit;
using Spoke.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Spoke.Tests.Services;

public class SecureStorageServiceTests
{
    private const string TestEncryptionKey = "test-encryption-key-32-characters!!";

    [Fact]
    public void SecureStorageService_Constructor_RequiresEncryptionKey()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SecureStorageService(null!));
    }

    [Fact]
    public void SecureStorageService_Constructor_AcceptsValidKey()
    {
        // Arrange & Act
        var service = new SecureStorageService(TestEncryptionKey);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public async Task SecureStorageService_StoreAsync_ReturnsTrue_ForValidData()
    {
        // Arrange
        var testStorage = new SecureStorageService.TestSecureStorage();
        var service = new SecureStorageService(TestEncryptionKey, testStorage);
        var testData = new TestData { Name = "Test", Value = 42 };

        // Act
        var result = await service.StoreAsync("test_key", testData);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task SecureStorageService_RetrieveAsync_ReturnsStoredData()
    {
        // Arrange
        var testStorage = new SecureStorageService.TestSecureStorage();
        var service = new SecureStorageService(TestEncryptionKey, testStorage);
        var originalData = new TestData { Name = "Test", Value = 42 };
        var key = "test_retrieve_key";

        // Act
        await service.StoreAsync(key, originalData);
        var retrievedData = await service.RetrieveAsync<TestData>(key);

        // Assert
        Assert.NotNull(retrievedData);
        Assert.Equal(originalData.Name, retrievedData.Name);
        Assert.Equal(originalData.Value, retrievedData.Value);
    }

    [Fact]
    public async Task SecureStorageService_RetrieveAsync_ReturnsNull_ForNonexistentKey()
    {
        // Arrange
        var testStorage = new SecureStorageService.TestSecureStorage();
        var service = new SecureStorageService(TestEncryptionKey, testStorage);

        // Act
        var result = await service.RetrieveAsync<TestData>("nonexistent_key");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SecureStorageService_StoreAsync_HandlesComplexObjects()
    {
        // Arrange
        var testStorage = new SecureStorageService.TestSecureStorage();
        var service = new SecureStorageService(TestEncryptionKey, testStorage);
        var complexData = new ComplexTestData
        {
            Id = Guid.NewGuid(),
            Items = new List<string> { "item1", "item2", "item3" },
            Metadata = new Dictionary<string, int> { ["key1"] = 1, ["key2"] = 2 }
        };

        // Act
        await service.StoreAsync("complex_key", complexData);
        var retrievedData = await service.RetrieveAsync<ComplexTestData>("complex_key");

        // Assert
        Assert.NotNull(retrievedData);
        Assert.Equal(complexData.Id, retrievedData.Id);
        Assert.Equal(complexData.Items.Count, retrievedData.Items.Count);
        Assert.Equal(complexData.Metadata.Count, retrievedData.Metadata.Count);
    }

    // Test data classes
    private class TestData
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    private class ComplexTestData
    {
        public Guid Id { get; set; }
        public List<string> Items { get; set; } = new();
        public Dictionary<string, int> Metadata { get; set; } = new();
    }
}