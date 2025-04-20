namespace FluentySaga;

using FluentySaga.Interfaces;

public class SagaBuilder(ISagaContext context)
{
    private readonly List<SagaStep> steps = [];
    private readonly ISagaContext context = context;
    private string? currentStepName;

    public SagaBuilder AddStep<TInput, TResult>(
        string name,
        TInput input,
        Func<TInput, Task<TResult>> operation)
    {
        if (this.steps.Any(s => s.Name == name))
            throw new ArgumentException($"A step with name '{name}' already exists.", nameof(name));

        var step = SagaStep<TInput, TResult, Void, Void>
            .Create(name, input, operation);

        this.steps.Add(step);
        this.currentStepName = name;
        return this;
    }

    public SagaBuilder WithCompensation<TInput, TResult>(
        TInput input,
        Func<TInput, Task<TResult>> compensation)
    {
        if (string.IsNullOrEmpty(this.currentStepName))
            throw new InvalidOperationException("No step defined to add compensation.");

        var lastStep = this.steps.LastOrDefault();
        if (lastStep == null || lastStep.Name != this.currentStepName)
            throw new InvalidOperationException($"Step '{this.currentStepName}' not found.");

        this.steps.RemoveAt(this.steps.Count - 1);

        var step = SagaStep<object, object, TInput, TResult>
            .Create(
                this.currentStepName,
                lastStep,
                async x => await ((ISagaOperation)x).ExecuteOperation(),
                input,
                compensation);

        this.steps.Add(step);
        return this;
    }

    public SagaBuilder AddTransactionalStep<TInput, TResult>(
        string name,
        ITransactionalRepository repository,
        TInput input,
        Func<TInput, Task<TResult>> operation)
    {
        if (this.steps.Any(s => s.Name == name))
            throw new ArgumentException($"A step with name '{name}' already exists.", nameof(name));

        this.context.RegisterRepository(repository);
        return AddStep(name, input, operation);
    }

    public SagaHandler Build() => new([.. this.steps], this.context);
}