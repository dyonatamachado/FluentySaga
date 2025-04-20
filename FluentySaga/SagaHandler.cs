namespace FluentySaga;

using FluentySaga.Interfaces;

public class SagaHandler : ISagaHandler, IAsyncDisposable
{
    private readonly List<SagaStep> steps;
    private readonly Stack<SagaStep> executedSteps;
    private readonly object @lock = new();
    private readonly ISagaContext context;
    private bool disposed;

    public SagaHandler(List<SagaStep> steps, ISagaContext context)
    {
        this.steps = steps ?? throw new ArgumentNullException(nameof(steps));
        this.context = context ?? throw new ArgumentNullException(nameof(context));
        this.executedSteps = new Stack<SagaStep>();
    }

    public async Task ExecuteAsync()
    {
        if (this.disposed)
        {
            throw new ObjectDisposedException(nameof(SagaHandler), "Cannot execute saga: the handler was disposed.");
        }

        try
        {
            await this.context.BeginTransactionsAsync();

            foreach (var sagaStep in this.steps)
            {
                if (sagaStep is ISagaOperation operation)
                {
                    var result = await operation.ExecuteOperation();
                    if (result != null && result is not Void)
                    {
                        this.context.SetResult(sagaStep.Name, result);
                    }
                    lock (this.@lock)
                    {
                        this.executedSteps.Push(sagaStep);
                    }
                }
            }

            await this.context.CommitTransactionsAsync();
        }
        catch
        {
            await this.context.RollbackTransactionsAsync();
            await this.CompensateSteps();
            throw;
        }
    }

    private async Task CompensateSteps()
    {
        while (true)
        {
            SagaStep? sagaStep;
            lock (this.@lock)
            {
                if (!this.executedSteps.TryPop(out sagaStep))
                    break;
            }

            try
            {
                if (sagaStep is ISagaCompensation compensation)
                {
                    await compensation.ExecuteCompensation();
                }
            }
            catch (Exception ex)
            {
                if (sagaStep is ISagaCompensation compensation)
                {
                    
                }
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (this.disposed)
            return;

        await this.context.DisposeAsync();
        this.disposed = true;
        GC.SuppressFinalize(this);
    }
}
