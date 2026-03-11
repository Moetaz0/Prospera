using System.Security.Claims;
using Prospera.Application.Common.Interfaces;

namespace Prospera.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    public Guid UserId => Guid.Empty;

    public string Email => string.Empty;

    public bool IsAuthenticated => false;
}