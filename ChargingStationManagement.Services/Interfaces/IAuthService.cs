using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChargingStationManagement.Domain.Entities;

namespace ChargingStationManagement.Services.Interfaces
{
    public record RoleInfo(Guid RoleId, string RoleName);

    public interface IAuthService
    {
        Task<string> AuthenticateAsync(string username, string password);
        Task<(User user, IEnumerable<RoleInfo> roles, string token)> AuthenticateWithDetailsAsync(string username, string password);
    }
}