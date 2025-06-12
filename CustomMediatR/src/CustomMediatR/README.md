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

## Advanced Usage: Pipeline Behaviors

Pipeline behaviors allow you to add cross-cutting concerns to your request pipeline. This is perfect for implementing logging, validation, caching, or transaction management in a clean and reusable way.

A behavior wraps the actual request handler, allowing you to execute code before and after the request is handled.

1.  **Define your `behavior`:**
    Let's create a simple logging behavior that logs the request name before and after it's processed.

    ```csharp
    // A generic logging behavior
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            Console.WriteLine($"--> Handling request: {typeof(TRequest).Name}");

            // Call the next delegate in the pipeline.
            // This could be another behavior or the final request handler.
            var response = await next();

            Console.WriteLine($"<-- Handled request: {typeof(TRequest).Name}");

            return response;
        }
    }
    ```

2.  **Register the `behavior`:**
    Register the open generic IPipelineBehavior in your DI container. The Mediator will automatically resolve and apply it to all requests.

    ```csharp
    builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    ```

3.  **See it in action:**
    Now, when you call mediator.Send(), the LoggingBehavior will automatically execute. The console output from the previous example would now look like this:

    --> Handling request: MyRequest
    <-- Handled request: MyRequest
    Recieved: Hello World
    