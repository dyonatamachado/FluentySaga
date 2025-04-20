namespace FluentySaga.Interfaces;
using System;
using System.Threading.Tasks;

public interface ISagaContext : IAsyncDisposable
{
    public void RegisterRepository(ITransactionalRepository repository);
    public void SetResult<T>(string stepName, T value);
    public T GetStepResult<T>(string stepName);
    public Task BeginTransactionsAsync();
    public Task CommitTransactionsAsync();
    public Task RollbackTransactionsAsync();
}