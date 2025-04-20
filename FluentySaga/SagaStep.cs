namespace FluentySaga;

using FluentySaga.Interfaces;

public abstract class SagaStep
{
    public string Name { get; set; } = string.Empty;

}

public class SagaStep<TOperationInput, TOperationOutput, TCompensationInput, TCompensationOutput>
    : SagaStep, ISagaOperation, ISagaCompensation
{
    private readonly TOperationInput operationInput;
    private readonly Func<TOperationInput, Task<TOperationOutput>> operation;
    private readonly TCompensationInput? compensationInput;
    private readonly Func<TCompensationInput, Task<TCompensationOutput>>? compensation;

    private SagaStep(
        string name,
        TOperationInput operationInput,
        Func<TOperationInput, Task<TOperationOutput>> operation,
        TCompensationInput? compensationInput = default,
        Func<TCompensationInput, Task<TCompensationOutput>>? compensation = null)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(operation);

        this.Name = name;
        this.operationInput = operationInput;
        this.operation = operation;
        this.compensationInput = compensationInput;
        this.compensation = compensation;
    }

    public async Task<object?> ExecuteOperation()
    {
        var result = await this.operation(this.operationInput);
        return result;
    }

    public async Task ExecuteCompensation()
    {
        if (this.compensation != null && this.compensationInput != null)
        {
            await this.compensation(this.compensationInput);
        }
    }

    public object? GetCompensationInput() => this.compensationInput;

    public static SagaStep<TOperationInput, TOperationOutput, TCompensationInput, TCompensationOutput> Create(
        string name,
        TOperationInput operationInput,
        Func<TOperationInput, Task<TOperationOutput>> operation) => new(name, operationInput, operation);

    public static SagaStep<TOperationInput, TOperationOutput, TCompensationInput, TCompensationOutput> Create(
        string name,
        TOperationInput operationInput,
        Func<TOperationInput, Task<TOperationOutput>> operation,
        TCompensationInput compensationInput,
        Func<TCompensationInput, Task<TCompensationOutput>> compensation) => new(name, operationInput, operation, compensationInput, compensation);
}