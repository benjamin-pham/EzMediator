using EzMediator;

namespace Demo;

internal class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserResponse>
{
    public Task<UserResponse> Handle(CreateUserCommand request, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Handler CreateUserCommandHandler");
        return Task.FromResult(new UserResponse()
        {
            FirstName = request.FirstName,
            LastName = request.LastName
        });
    }
}
