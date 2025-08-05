using Demo;
using EzMediator;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddMediator(typeof(Program).Assembly);
//services.AddRequestHandlers(typeof(Program).Assembly);
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingPipelineBehavior<,>));

var serviceprovider = services.BuildServiceProvider();

IMediator mediator = serviceprovider.GetRequiredService<IMediator>();

var result = await mediator.SendAsync(new CreateUserCommand()
{
    FirstName = "Benjamin",
    LastName = "Pham"
});

Console.WriteLine($"{result.FirstName} {result.LastName}");