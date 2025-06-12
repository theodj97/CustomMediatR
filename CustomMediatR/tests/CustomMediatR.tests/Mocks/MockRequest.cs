namespace CustomMediatR.tests.Mocks;

public class MockRequest : IRequest<MockResponse>
{
    public string Message { get; set; } = string.Empty;
}
