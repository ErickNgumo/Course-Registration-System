using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using StudentCourseRegistration.Api.Application.Abstractions.Persistence;

namespace StudentCourseRegistration.Api.Infrastructure.Persistence;

/// <summary>Wraps the EF Core DbContext transaction in a data-leak-free handle.</summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly RegistrationDbContext _dbContext;

    public UnitOfWork(RegistrationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IAsyncTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        return new AsyncTransaction(transaction);
    }

    private sealed class AsyncTransaction : IAsyncTransaction
    {
        private readonly IDbContextTransaction _transaction;
        private bool _disposed;

        public AsyncTransaction(IDbContextTransaction transaction)
        {
            _transaction = transaction;
        }

        public Task CommitAsync(CancellationToken cancellationToken) =>
            _transaction.CommitAsync(cancellationToken);

        public Task RollbackAsync(CancellationToken cancellationToken) =>
            _transaction.RollbackAsync(cancellationToken);

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            await _transaction.DisposeAsync();
        }
    }
}
