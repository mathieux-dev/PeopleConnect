using PeopleConnect.Application.Contracts.Infrastructure;
using PeopleConnect.Application.Contracts.Persistence;
using PeopleConnect.Domain.Entities;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace PeopleConnect.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserRepository _userRepository;
    private User? _currentUser;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, IUserRepository userRepository)
    {
        _httpContextAccessor = httpContextAccessor;
        _userRepository = userRepository;
    }

    public Guid? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }

    public string? Username => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public bool IsAdmin
    {
        get
        {
            var roleClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;
            return roleClaim == UserRole.Admin.ToString();
        }
    }

    public bool CanEditPerson(Guid personId)
    {
        if (!IsAuthenticated || !UserId.HasValue)
            return false;

        if (IsAdmin)
            return true;

        
        var user = GetCurrentUserAsync().Result;
        return user?.PersonId == personId;
    }

    public bool CanDeletePerson(Guid personId)
    {
        return CanEditPerson(personId); 
    }

    private async Task<User?> GetCurrentUserAsync()
    {
        if (_currentUser == null && UserId.HasValue)
        {
            _currentUser = await _userRepository.GetByIdWithPersonAsync(UserId.Value);
        }
        return _currentUser;
    }
}
