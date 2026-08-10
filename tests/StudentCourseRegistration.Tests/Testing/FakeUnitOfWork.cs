using StudentCourseRegistration.Api.Application.Abstractions.Persistence;

namespace StudentCourseRegistration.Tests.Testing;

/// <summary>An in-memory unit of work double that records transaction usage for assertions.</summary>
internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int BeginCount { get; private set; }
    public int CommitCount { get; private set; }
    public int RollbackCount { get; private set; }

    public Task<IAsyncTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        BeginCount++;
        return Task.FromResult<IAsyncTransaction>(new AsyncTransaction(this));
    }

    internal void Commit() => CommitCount++;
    internal void Rollback() => RollbackCount++;

    private sealed class AsyncTransaction : IAsyncTransaction
    {
        private readonly FakeUnitOfWork _owner;
        private bool _disposed;

        public AsyncTransaction(FakeUnitOfWork owner)
        {
            _owner = owner;
        }

        public Task CommitAsync(CancellationToken cancellationToken)
        {
            _owner.Commit();
            return Task.CompletedTask;
        }

        public Task RollbackAsync(CancellationToken cancellationToken)
        {
            _owner.Rollback();
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            _disposed = true;
            return ValueTask.CompletedTask;
        }
    }
}
