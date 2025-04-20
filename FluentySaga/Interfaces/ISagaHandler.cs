namespace FluentySaga.Interfaces;

public interface ISagaHandler : IAsyncDisposable
{
    public Task ExecuteAsync();
}