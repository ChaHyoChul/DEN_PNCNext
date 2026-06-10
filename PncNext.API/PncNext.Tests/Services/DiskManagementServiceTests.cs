using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using PncNext.Domain.Entities;
using PncNext.Domain.Interfaces;
using PncNext.Infrastructure.Persistence;
using PncNext.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PncNext.Tests.Services
{
    public class DiskManagementServiceTests
    {
        private readonly DbContextOptions<AppDbContext> _dbOptions;
        private readonly Mock<ISignalRService> _signalRMock;

        public DiskManagementServiceTests()
        {
            _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _signalRMock = new Mock<ISignalRService>();
        }

        [Fact]
        public async Task RegisterDiskAsync_ShouldAssignNextDiskId_WhenInputIsZero()
        {
            // Arrange
            using var context = new AppDbContext(_dbOptions);
            context.DiskInventories.Add(new DiskInventory { DiskId = 1, DiskName = "D0001", IsDeleted = false });
            context.DiskInventories.Add(new DiskInventory { DiskId = 5, DiskName = "D0005", IsDeleted = false });
            await context.SaveChangesAsync();

            var service = new DiskManagementService(context, _signalRMock.Object);
            var newDisk = new DiskInventory { DiskId = 0, DiskName = "D9001" };

            // Act
            var result = await service.RegisterDiskAsync(newDisk);

            // Assert
            result.DiskId.Should().Be(6); // Max(1, 5) + 1
            result.DiskName.Should().Be("D9001");
        }

        [Fact]
        public async Task RegisterDiskAsync_ShouldThrowException_WhenDiskNameIsEmpty()
        {
            // Arrange
            using var context = new AppDbContext(_dbOptions);
            var service = new DiskManagementService(context, _signalRMock.Object);
            var newDisk = new DiskInventory { DiskId = 10, DiskName = "" };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.RegisterDiskAsync(newDisk));
        }

        [Fact]
        public async Task RegisterDiskAsync_ShouldThrowException_WhenDuplicateDiskIdExists()
        {
            // Arrange
            using var context = new AppDbContext(_dbOptions);
            context.DiskInventories.Add(new DiskInventory { DiskId = 1, DiskName = "D0001", IsDeleted = false });
            await context.SaveChangesAsync();

            var service = new DiskManagementService(context, _signalRMock.Object);
            var newDisk = new DiskInventory { DiskId = 1, DiskName = "D9999" };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.RegisterDiskAsync(newDisk));
            ex.Message.Should().Contain("이미 등록된 활성 자재");
        }

        [Fact]
        public async Task DeleteDiskAsync_ShouldSetIsDeleted_And_DisconnectReadyFiles()
        {
            // Arrange
            using var context = new AppDbContext(_dbOptions);
            var disk = new DiskInventory { Seq = 1, DiskId = 1, DiskName = "D0001", IsDeleted = false };
            context.DiskInventories.Add(disk);

            var ncFile = new NcFileInventory 
            { 
                Id = 10, 
                FileName = "test.nc", 
                DiskSeq = 1, 
                Status = NcValidationStatus.Ready 
            };
            context.NcFileInventories.Add(ncFile);
            await context.SaveChangesAsync();

            var service = new DiskManagementService(context, _signalRMock.Object);

            // Act
            await service.DeleteDiskAsync(1);

            // Assert
            var updatedDisk = await context.DiskInventories.FindAsync(1);
            updatedDisk!.IsDeleted.Should().BeTrue();

            var updatedFile = await context.NcFileInventories.FindAsync(10);
            updatedFile!.DiskSeq.Should().BeNull();
            updatedFile.Status.Should().Be(NcValidationStatus.InvalidDiskId);

            _signalRMock.Verify(s => s.BroadcastAsync("DiskSoftDeleted", It.IsAny<object>()), Times.Once);
        }
    }
}
