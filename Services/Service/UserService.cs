using Contract.Repositories.Entity;
using Contract.Repositories.Interface;
using Contract.Services.Interface;
using Core.Base;
using Core.Store;
using Core.Utils;
using Microsoft.EntityFrameworkCore;
using ModelViews.UserModelViews;

namespace Services.Service
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<User> GetUserByEmail(string email)
        {
            var user = await _unitOfWork.GetRepository<User>().Entities.FirstOrDefaultAsync(x => x.Email.Equals(email));
            return user;
        }

        public async Task<bool> AddRoleToAccountAsync(int userId, string roleName)
        {
            // Tìm tài khoản người dùng đã tồn tại
            var user = await _unitOfWork.GetRepository<User>().GetByIdAsync(userId);

            if (user == null)
            {
                throw new Exception("User not found");
            }

            // Kiểm tra vai trò có tồn tại không
            var role = await _unitOfWork.GetRepository<ApplicationRole>().Entities.FirstOrDefaultAsync(r => r.Name == roleName);

            if (role == null)
            {
                throw new Exception("Role does not exist");
            }

            // Kiểm tra nếu người dùng đã có vai trò này
            var userRoleRepository = _unitOfWork.GetRepository<ApplicationUserRoles>();
            var existingUserRole = await userRoleRepository.Entities
                .AsNoTracking()  // Không theo dõi thực thể này
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == role.Id);

            if (existingUserRole != null)
            {
                throw new Exception("User already has this role");
            }

            // Nếu không tồn tại, thêm vai trò cho người dùng
            var applicationUserRole = new ApplicationUserRoles
            {
                UserId = user.Id,
                RoleId = role.Id,
                CreatedBy = user.UserName,  // Ghi lại ai đã thêm vai trò này
                CreatedTime = CoreHelper.SystemTimeNow,
                LastUpdatedBy = user.UserName,
                LastUpdatedTime = CoreHelper.SystemTimeNow
            };

            // Lưu thông tin vào ApplicationUserRoles
            await userRoleRepository.InsertAsync(applicationUserRole);
            await _unitOfWork.SaveAsync();

            return true;

        }
        public async Task<BasePaginatedList<UserModelResponse>> GetAllAccounts(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var accountRepo = _unitOfWork.GetRepository<User>();
            var query = accountRepo.Entities.Where(a => !a.DeletedTime.HasValue).OrderByDescending(a => a.CreatedTime);

            int totalCount = await query.CountAsync();
            var accounts = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            List<UserModelResponse> result = new List<UserModelResponse>();
            foreach (var account in accounts)
            {
                var Role = await (
                                    from userRole in _unitOfWork.GetRepository<ApplicationUserRoles>().Entities
                                    join roleEntity in _unitOfWork.GetRepository<ApplicationRole>().Entities
                                    on userRole.RoleId equals roleEntity.Id
                                    where userRole.UserId == account.Id && userRole.DeletedTime == null
                                    select roleEntity.Name
                                 ).FirstOrDefaultAsync(); // get Role for user
                var AccountModel = new UserModelResponse
                {
                    Id = account.Id,
                    
                };
                result.Add(AccountModel);
            }

            return new BasePaginatedList<UserModelResponse>(result, totalCount, pageNumber, pageSize);
        }

        public async Task<BaseResponse<string>> UpdateAccount(int id, UserInfoModel model)
        {
            var account = await _unitOfWork.GetRepository<User>()
                .Entities
                .FirstOrDefaultAsync(a => a.Id == id && !a.DeletedTime.HasValue);
            if (account == null)
            {
                return new BaseResponse<string>(StatusCodeHelper.Notfound, "400", "Account not found.");
            }

    //        if (!string.IsNullOrEmpty(model.Role))
    //        {
    //            var role = await _unitOfWork.GetRepository<ApplicationRole>()
    //                .Entities.FirstOrDefaultAsync(n => n.Name.Equals(model.Role));

    //            if (role == null)
    //            {
    //                return new BaseResponse<string>(StatusCodeHelper.Notfound, "400", "Role does not exist.");
    //            }

    //            // Lấy role hiện tại của user (đang active)
    //            var currentUserRole = await _unitOfWork.GetRepository<ApplicationUserRoles>()
    //.Entities.FirstOrDefaultAsync(ur => ur.UserId == account.Id && !ur.DeletedTime.HasValue);

    //            // Lấy role đã bị xóa (soft delete)
    //            var deletedUserRole = await _unitOfWork.GetRepository<ApplicationUserRoles>()
    //.Entities.FirstOrDefaultAsync(ur => ur.UserId == account.Id && ur.DeletedTime.HasValue && ur.RoleId == role.Id);

    //            if (deletedUserRole != null)
    //            {
    //                // Nếu cập nhật về role cũ đã bị xóa, khôi phục lại
    //                deletedUserRole.DeletedTime = null;
    //                deletedUserRole.DeletedBy = null;

    //                if (currentUserRole != null)
    //                {
    //                    // Xóa role hiện tại (soft delete)
    //                    currentUserRole.DeletedTime = DateTimeOffset.UtcNow;
    //                    currentUserRole.DeletedBy = "System";
    //                }
    //            }
    //            else if (currentUserRole != null && currentUserRole.RoleId != role.Id)
    //            {
    //                // Nếu cập nhật về role mới, xóa role hiện tại và thêm role mới
    //                currentUserRole.DeletedTime = DateTimeOffset.UtcNow;
    //                currentUserRole.DeletedBy = "System";

    //                var newUserRole = new ApplicationUserRoles
    //                {
    //                    UserId = account.Id,
    //                    RoleId = role.Id,
    //                    CreatedBy = "System",
    //                    CreatedTime = DateTimeOffset.UtcNow
    //                };
    //                await _unitOfWork.GetRepository<ApplicationUserRoles>().InsertAsync(newUserRole);
    //            }
    //        }


            if (!string.IsNullOrEmpty(model.PhoneNumber))
            {
                account.PhoneNumber = model.PhoneNumber;
            }

            account.LastUpdatedBy = "System";
            account.LastUpdatedTime = DateTimeOffset.UtcNow;

            try
            {
                //await _unitOfWork.GetRepository<Account>().UpdateAsync(account);
                await _unitOfWork.SaveAsync();
                return BaseResponse<string>.OkResponse("Account updated successfully.");
            }
            catch (Exception ex)
            {
                return new BaseResponse<string>(StatusCodeHelper.ServerError, StatusCodeHelper.ServerError.Name(), $"Internal server error: {ex.Message}");
            }
        }
        public async Task<BaseResponse<string>> AddUserInfoAsync(int id, UserInfoModel model)
        {
            var account = await _unitOfWork.GetRepository<User>().Entities.FirstOrDefaultAsync(a => a.Id == id);

            if (account == null)
            {
                return new BaseResponse<string>(StatusCodeHelper.Notfound, "404", "Account not found.");
            }
            var patientInfo = new UserInfo
            {
                Gender = model.Gender,
                DateOfBirth = model.DateOfBirth,
            };

            account.UserInfo= patientInfo;

            try
            {
                await _unitOfWork.SaveAsync();
                return BaseResponse<string>.OkResponse("Patient information added successfully.");
            }
            catch (Exception ex)
            {
                return new BaseResponse<string>(StatusCodeHelper.ServerError, "500", $"Internal server error: {ex.Message}");
            }
        }
        public async Task<bool> DeleteAccount(int id)
        {
            var accountRepo = _unitOfWork.GetRepository<User>();
            var account = await accountRepo.Entities.FirstOrDefaultAsync(a => a.Id == id && !a.DeletedTime.HasValue);

            if (account == null)
            {
                return false;
            }

            account.DeletedTime = DateTimeOffset.Now;
            account.DeletedBy = "System";

            await accountRepo.UpdateAsync(account);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<UserModelResponse> GetAccountById(int Id)
        {
            var account = _unitOfWork.GetRepository<User>().Entities.FirstOrDefault(n => n.Id == Id);
            var Role = await (
                    from userRole in _unitOfWork.GetRepository<ApplicationUserRoles>().Entities
                    join roleEntity in _unitOfWork.GetRepository<ApplicationRole>().Entities
                    on userRole.RoleId equals roleEntity.Id
                    where userRole.UserId == account.Id && userRole.DeletedTime == null

                    select roleEntity.Name
                 ).FirstOrDefaultAsync(); // get Role for user
            var AccountModel = new UserModelResponse
            {
                Id = account.Id,               
                Email = account.Email,
                
            };
            return AccountModel;
        }
        public Task<bool> AddClaimToUserAsync(int userId, string claimType, string claimValue, string createdBy)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ApplicationUserClaims>> GetUserClaimsAsync(int userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateClaimAsync(int claimId, string claimType, string claimValue, string updatedBy)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SoftDeleteClaimAsync(int claimId, string deletedBy)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AddClaimToRoleAsync(int roleId, string claimType, string claimValue, string createdBy)
        {
            throw new NotImplementedException();
        }
    }
}
