namespace Deveel;

public class TestCancellationTokenSource : IOperationCancellationSource
{
    public CancellationToken Token => CancellationToken.None;
}

