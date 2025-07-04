using Core.Base;
using ModelViews.SupplierModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Services.Interface
{
    public interface ISupplierService
    {
        Task<BaseResponse<BasePaginatedList<SupplierModel>>> GetAllSupplierAsync(int pageNumber, int pageSize);
        Task<BaseResponse<SupplierModel>> CreateSupplierAsync(CreateSupplierModel model, int userId);
        Task<BaseResponse<bool>> DeleteSuppilerAsync(int supplierId, int userId);
        Task<BaseResponse<SupplierModel>> UpdateSupplierAsync(int supplierId, UpdateSupplierModel model, int userId);
    }
}
