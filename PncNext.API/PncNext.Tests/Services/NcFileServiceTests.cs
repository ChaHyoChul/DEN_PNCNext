using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Moq;
using PncNext.Domain.Entities;
using PncNext.Infrastructure.Persistence;
using PncNext.Infrastructure.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PncNext.Tests.Services
{
    public class NcFileServiceTests : IDisposable
    {
        private readonly DbContextOptions<AppDbContext> _dbOptions;
        private readonly Mock<IConfiguration> _configMock;
        private readonly string _testStoragePath;

        public NcFileServiceTests()
        {
            _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _testStoragePath = Path.Combine(Path.GetTempPath(), "PncNextTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testStoragePath);

            _configMock = new Mock<IConfiguration>();
            _configMock.Setup(c => c["NcFileStoragePath"]).Returns(_testStoragePath);
        }

        public void Dispose()
        {
            if (Directory.Exists(_testStoragePath))
            {
                Directory.Delete(_testStoragePath, true);
            }
        }

        [Fact]
        public async Task ResetToReadyAsync_ShouldResetStatusAndClearLastErrorLine()
        {
            // Arrange
            using var context = new AppDbContext(_dbOptions);
            var ncFile = new NcFileInventory
            {
                Id = 1,
                FileName = "test.nc",
                FilePath = Path.Combine(_testStoragePath, "test.nc"),
                Status = NcValidationStatus.Completed,
                LastErrorLine = 150,
                DiskSeq = 1
            };
            context.NcFileInventories.Add(ncFile);
            context.DiskInventories.Add(new DiskInventory { Seq = 1, DiskId = 1, DiskName = "D0001", IsDeleted = false });
            
            // Create dummy physical file
            File.WriteAllText(ncFile.FilePath, "G0 X0 Y0");
            await context.SaveChangesAsync();

            var service = new NcFileService(context, _configMock.Object);

            // Act
            var result = await service.ResetToReadyAsync(1);

            // Assert
            result.Should().BeTrue();
            var updated = await context.NcFileInventories.FindAsync(1);
            updated!.Status.Should().Be(NcValidationStatus.Ready);
            updated.LastErrorLine.Should().BeNull();
        }

        [Fact]
        public async Task ResetErrorAsync_ShouldKeepLastErrorLine()
        {
            // Arrange
            using var context = new AppDbContext(_dbOptions);
            var ncFile = new NcFileInventory
            {
                Id = 2,
                FileName = "error.nc",
                FilePath = Path.Combine(_testStoragePath, "error.nc"),
                Status = NcValidationStatus.Error,
                LastErrorLine = 99,
                DiskSeq = 1
            };
            context.NcFileInventories.Add(ncFile);
            context.DiskInventories.Add(new DiskInventory { Seq = 1, DiskId = 1, DiskName = "D0001", IsDeleted = false });
            
            File.WriteAllText(ncFile.FilePath, "G0 X10");
            await context.SaveChangesAsync();

            var service = new NcFileService(context, _configMock.Object);

            // Act
            var result = await service.ResetErrorAsync(2);

            // Assert
            result.Should().BeTrue();
            var updated = await context.NcFileInventories.FindAsync(2);
            updated!.Status.Should().Be(NcValidationStatus.Ready);
            updated.LastErrorLine.Should().Be(99); // Should NOT be cleared
        }
    }
}
