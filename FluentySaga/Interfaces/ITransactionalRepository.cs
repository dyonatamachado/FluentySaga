namespace FluentySaga.Interfaces;

public interface ITransactionalRepository : IAsyncDisposable
{
    public Task BeginTransactionAsync();
    public Task CommitAsync();
    public Task RollbackAsync();
    public bool HasActiveTransaction { get; }
}