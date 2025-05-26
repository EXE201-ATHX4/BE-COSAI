using Contract.Repositories.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Services.Interface
{
    public interface IUserService
    {
        Task<User> GetUserByEmail(string email);
        Task<bool> AddRoleToAccountAsync(int userId, string roleName);
        Task<bool> AddClaimToUserAsync(int userId, string claimType, string claimValue, string createdBy);
        Task<bool> AddClaimToRoleAsync(int roleId, string claimType, string claimValue, string createdBy);
        Task<IEnumerable<ApplicationUserClaims>> GetUserClaimsAsync(int userId);
        Task<bool> UpdateClaimAsync(int claimId, string claimType, string claimValue, string updatedBy);
        Task<bool> SoftDeleteClaimAsync(int claimId, string deletedBy);
    }
}
