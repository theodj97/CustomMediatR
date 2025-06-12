namespace CustomMediatR.tests.Mocks;

public class MockPipelineBehavior(string name, List<string> executionOrder) : IPipelineBehavior<MockRequest, MockResponse>
{
    private readonly string name = name;
    private readonly List<string> executionOrder = executionOrder;

    public async Task<MockResponse> Handle(MockRequest request, RequestHandlerDelegate<MockResponse> next, CancellationToken cancellationToken)
    {
        executionOrder.Add($"{name}:Start");
        var response = await next();
        response.Result += $":{name}";
        executionOrder.Add($"{name}:End");
        return response;
    }
}