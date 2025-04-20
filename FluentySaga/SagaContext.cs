namespace FluentySaga;

using FluentySaga.Interfaces;

public class SagaContext : ISagaContext
{
    private readonly Dictionary<string, object> results = [];
    private readonly List<ITransactionalRepository> repositories = [];
    private bool disposed;

    public void RegisterRepository(ITransactionalRepository repository)
    {
        if (!this.repositories.Contains(repository))
            this.repositories.Add(repository);
    }

    public void SetResult<T>(string stepName, T value) => this.results[stepName] = value!;

    public T GetStepResult<T>(string stepName) => (T)this.results[stepName];

    public async Task BeginTransactionsAsync()
    {
        foreach (var repository in this.repositories)
        {
            if (!repository.HasActiveTransaction)
                await repository.BeginTransactionAsync();
        }
    }

    public async Task CommitTransactionsAsync()
    {
        foreach (var repository in this.repositories)
        {
            if (repository.HasActiveTransaction)
                await repository.CommitAsync();
        }
    }

    public async Task RollbackTransactionsAsync()
    {
        foreach (var repository in this.repositories)
        {
            if (repository.HasActiveTransaction)
                await repository.RollbackAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (this.disposed)
            return;

        foreach (var repository in this.repositories)
        {
            await repository.DisposeAsync();
        }

        this.disposed = true;
        GC.SuppressFinalize(this);
    }
}