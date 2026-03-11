using MediatR;
using AutoMapper;
using Prospera.Domain.Entities;
using Prospera.Domain.Interfaces;
using Prospera.Application.DTOs;

namespace Prospera.Application.Features.Users.Commands;

/// <summary>
/// Handler for CreateUserCommand - creates a new user
/// </summary>
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Create new user entity
        var user = new User(request.FullName, request.Email);

        // Add to repository
        await _userRepository.AddAsync(user);

        // Map and return
        return _mapper.Map<UserDto>(user);
    }
}
