using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Prospera.Application.Common.Interfaces;

namespace Prospera.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor?.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }

    public string Email
    {
        get
        {
            return _httpContextAccessor?.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        }
    }

    public bool IsAuthenticated
    {
        get
        {
            return _httpContextAccessor?.HttpContext?.User.Identity?.IsAuthenticated ?? false;
        }
    }
}