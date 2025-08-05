using EzMediator;

namespace Demo;

internal class CreateUserCommand : IRequest<UserResponse>
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
}
