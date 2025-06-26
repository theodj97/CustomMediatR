namespace CustomMediatR.tests.Mocks;

public class MockPipelineBehavior : IPipelineBehavior<MockRequest, MockResponse>
{
    public string Name { get; set; } = string.Empty;
    public List<string> ExecutionOrder { get; set; } = [];

    public async Task<MockResponse> Handle(MockRequest request, RequestHandlerDelegate<MockResponse> next, CancellationToken cancellationToken)
    {
        ExecutionOrder.Add($"{Name}:Start");
        var response = await next();
        if (string.IsNullOrEmpty(Name) is false)
            response.Result += $":{Name}";
        ExecutionOrder.Add($"{Name}:End");
        return response;
    }
}