namespace StudentCourseRegistration.Api.Application.Abstractions.Persistence;

/// <summary>Coordinates atomic multi-step persistence operations without leaking data-layer details.</summary>
public interface IUnitOfWork
{
    /// <summary>Begins a new ambient transaction and returns a handle that commits or rolls it back.</summary>
    Task<IAsyncTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
}

/// <summary>Handle to a started async transaction.</summary>
public interface IAsyncTransaction : IAsyncDisposable
{
    /// <summary>Commits the transaction.</summary>
    Task CommitAsync(CancellationToken cancellationToken);

    /// <summary>Rolls the transaction back.</summary>
    Task RollbackAsync(CancellationToken cancellationToken);
}
