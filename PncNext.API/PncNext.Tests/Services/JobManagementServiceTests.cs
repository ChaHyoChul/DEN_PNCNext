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
    public class JobManagementServiceTests
    {
        private readonly DbContextOptions<AppDbContext> _dbOptions;
        private readonly Mock<ISignalRService> _signalRMock;

        public JobManagementServiceTests()
        {
            _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _signalRMock = new Mock<ISignalRService>();
        }

        [Fact]
        public async Task StartJobAsync_ShouldCreateJob_And_UpdateNcFileStatus()
        {
            // Arrange
            using var context = new AppDbContext(_dbOptions);
            var disk = new DiskInventory { Seq = 1, DiskId = 1, DiskName = "D0001", IsDeleted = false };
            var ncFile = new NcFileInventory { Id = 1, FileName = "test.nc", Status = NcValidationStatus.Ready };
            
            context.DiskInventories.Add(disk);
            context.NcFileInventories.Add(ncFile);
            await context.SaveChangesAsync();

            var service = new JobManagementService(context, _signalRMock.Object);

            // Act
            var job = await service.StartJobAsync(1, 1);

            // Assert
            job.JobStatus.Should().Be("Running");
            job.NcFileId.Should().Be(1);
            job.DiskSeq.Should().Be(1);

            var updatedFile = await context.NcFileInventories.FindAsync(1);
            updatedFile!.Status.Should().Be(NcValidationStatus.Processing);
            updatedFile.DiskSeq.Should().Be(1);

            _signalRMock.Verify(s => s.BroadcastAsync("JobStarted", It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task StartJobAsync_ShouldThrowException_WhenMachineIsBusy()
        {
            // Arrange
            using var context = new AppDbContext(_dbOptions);
            context.NcFileInventories.Add(new NcFileInventory { Id = 1, Status = NcValidationStatus.Processing }); // Busy
            context.NcFileInventories.Add(new NcFileInventory { Id = 2, Status = NcValidationStatus.Ready });
            context.DiskInventories.Add(new DiskInventory { Seq = 10, DiskId = 10, DiskName = "D0010" });
            await context.SaveChangesAsync();

            var service = new JobManagementService(context, _signalRMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.StartJobAsync(2, 10));
            ex.Message.Should().Contain("다른 가공 작업이 진행 중");
        }

        [Fact]
        public async Task CompleteJobAsync_ShouldUpdateAllStatuses_And_DiskUsedArea()
        {
            // Arrange
            using var context = new AppDbContext(_dbOptions);
            var disk = new DiskInventory { Seq = 5, DiskId = 5, DiskName = "D0005", UsedAreaLayout = "{}" };
            var ncFile = new NcFileInventory { Id = 5, FileName = "job.nc", Status = NcValidationStatus.Processing };
            var job = new JobHistory { Id = 100, NcFileId = 5, DiskSeq = 5, JobStatus = "Running" };

            context.DiskInventories.Add(disk);
            context.NcFileInventories.Add(ncFile);
            context.JobHistories.Add(job);
            await context.SaveChangesAsync();

            var service = new JobManagementService(context, _signalRMock.Object);
            var newLayout = "{\"paths\":[1,2,3]}";

            // Act
            await service.CompleteJobAsync(100, newLayout);

            // Assert
            var updatedJob = await context.JobHistories.FindAsync(100);
            updatedJob!.JobStatus.Should().Be("Completed");
            updatedJob.EndTime.Should().NotBeNull();

            var updatedFile = await context.NcFileInventories.FindAsync(5);
            updatedFile!.Status.Should().Be(NcValidationStatus.Completed);

            var updatedDisk = await context.DiskInventories.FindAsync(5);
            updatedDisk!.UsedAreaLayout.Should().Be(newLayout);

            _signalRMock.Verify(s => s.BroadcastAsync("JobCompleted", It.IsAny<object>()), Times.Once);
        }
    }
}
