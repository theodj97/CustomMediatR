namespace CustomMediatR.tests.Mocks;

public class MockRequestHandler : IRequestHandler<MockRequest, MockResponse>
{
    public bool WasHandled { get; private set; } = false;

    public Task<MockResponse> Handle(MockRequest request, CancellationToken cancellationToken)
    {
        WasHandled = true;
        return Task.FromResult(new MockResponse { Result = $"Handled: {request.Message}" });
    }
}
