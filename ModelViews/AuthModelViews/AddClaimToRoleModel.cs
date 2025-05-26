using Core.Base;

namespace ModelViews.AuthModelViews
{
    public class AddClaimToRoleModel : BaseClaim
    {
        public int RoleId { get; set; }          // ID của vai trò (Role)
       
    }
}
