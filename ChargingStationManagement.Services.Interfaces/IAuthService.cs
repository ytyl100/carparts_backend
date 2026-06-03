using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChargingStationManagement.Domain.Entities;

namespace ChargingStationManagement.Services.Interfaces
{
    /// <summary>
    /// 角色信息（用于登录响应）
    /// </summary>
    public record RoleInfo(Guid RoleId, string RoleName);

    public interface IAuthService
    {
        /// <summary>
        /// 认证并返回 JWT token 字符串
        /// </summary>
        Task<string> AuthenticateAsync(string username, string password);

        /// <summary>
        /// 认证并返回用户详情、角色列表与 token
        /// </summary>
        Task<(User user, IEnumerable<RoleInfo> roles, string token)> AuthenticateWithDetailsAsync(string username, string password);
    }
}