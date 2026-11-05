using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.Services.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ASI.Basecode.Services.Tests;

public class EventTypeTimestampTests
{
    [Fact]
    public async Task CreateUpdateAndToggleUseTheInjectedClock()
    {
        var clock = new FixedClock { Now = new DateTimeOffset(2026, 1, 1, 18, 30, 0, TimeSpan.Zero) };
        var repository = new MemoryEventTypes();
        var service = new EventTypeService(repository, new ImmediateUnitOfWork(), null!, clock);
        var model = new EventTypeViewModel { TypeId = 1, TypeName = "Workshop", Description = "Description" };

        await service.AddEventTypeAsync(model, 1);
        Assert.Equal(clock.Now.UtcDateTime, repository.Item!.CreatedAt);
        Assert.Equal(DateTimeKind.Utc, repository.Item.CreatedAt.Kind);

        clock.Now = clock.Now.AddHours(1);
        await service.UpdateEventTypeAsync(model, 1);
        Assert.Equal(clock.Now.UtcDateTime, repository.Item.UpdatedAt);

        clock.Now = clock.Now.AddHours(1);
        await service.ToggleActiveAsync(1, 1);
        Assert.Equal(clock.Now.UtcDateTime, repository.Item.UpdatedAt);
        Assert.False(repository.Item.IsActive);
    }

    private sealed class FixedClock : TimeProvider
    {
        public DateTimeOffset Now { get; set; }
        public override DateTimeOffset GetUtcNow() => Now;
    }

    private sealed class MemoryEventTypes : IEventTypeRepository
    {
        public EventType? Item { get; private set; }
        public IQueryable<EventType> GetEventTypes() => Item is null ? Array.Empty<EventType>().AsQueryable() : new[] { Item }.AsQueryable();
        public EventType GetEventType(int typeId) => Item!;
        public bool EventTypeExists(int typeId) => Item is not null;
        public bool TypeNameExists(string typeName, int? excludeId = null) => false;
        public Task AddEventTypeAsync(EventType eventType, CancellationToken cancellationToken = default) { Item = eventType; return Task.CompletedTask; }
        public Task UpdateEventTypeAsync(EventType eventType, CancellationToken cancellationToken = default) { Item = eventType; return Task.CompletedTask; }
    }

    private sealed class ImmediateUnitOfWork : IUnitOfWork
    {
        public DbContext Database => throw new NotSupportedException();
        public void SaveChanges() { }
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
        public Task ExecuteInTransactionAsync(Func<CancellationToken, Task> work, CancellationToken cancellationToken = default) => work(cancellationToken);
        public Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> work, CancellationToken cancellationToken = default) => work(cancellationToken);
    }
}
