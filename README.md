# SimpleMediator

A simple and lightweight implementation of the Mediator pattern in .NET, inspired by MediatR.

## What is it?

This package provides the basic abstractions to implement the Mediator pattern, helping to decouple business logic and create a customizable request processing pipeline.

## Installation

Install the package via the .NET CLI:
```bash
dotnet add package theodj97.CustomMediatR
```

Or via the NuGet Package Manager:
```powershell
Install-Package theodj97.CustomMediatR
```

## Basic Usage

1.  **Define your request and handler:**

    ```csharp
    // Request
    public record MyRequest(string Message) : IRequest<string>;

    // Handler
    public class MyRequestHandler : IRequestHandler<MyRequest, string>
    {
        public Task<string> Handle(MyRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult($"Recieved: {request.Message}");
        }
    }
    ```

2.  **Register services in your `Program.cs` o `Startup.cs`:**

    ```csharp
    // Add the mediator.
    builder.Services.AddSingleton<IMediator, Mediator>();

    // Add your handlers. Opt 1, manually:
    builder.Services.AddScoped<IRequestHandler<MyRequest, string>, MyRequestHandler>();

    // Add your handlers. Opt 2, assembly reflection:
    builder.Services.AddMediatR(theAssembly);
    ```

3.  **Inject and use `IMediator`:**

    ```csharp
    public class YourService(IMediator mediator)
    {
        public async Task DoSomething()
        {
            var response = await mediator.Send(new MyRequest("Hello World"));
            Console.WriteLine(response); // Output: Recieved: Hello World
        }
    }
    ```