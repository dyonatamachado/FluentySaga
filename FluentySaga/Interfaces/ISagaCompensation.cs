namespace FluentySaga.Interfaces;

public interface ISagaCompensation
{
    Task ExecuteCompensation();
    object? GetCompensationInput();
}