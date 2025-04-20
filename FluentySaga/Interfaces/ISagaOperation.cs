namespace FluentySaga.Interfaces;

public interface ISagaOperation
{
    Task<object?> ExecuteOperation();
}