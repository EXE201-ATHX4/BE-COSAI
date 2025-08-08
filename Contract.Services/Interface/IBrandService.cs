using Core.Base;
using ModelViews.BrandModelViews;
using ModelViews.SupplierModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Services.Interface
{
    public interface IBrandService
    {
        Task<BaseResponse<BasePaginatedList<BrandModel>>> GetAllBrandAsync(int pageNumber, int pageSize);
        Task<BaseResponse<BrandModel>> CreateBrandAsync(CreateBrandModel model, int userId);
        Task<BaseResponse<bool>> DeleteBrandAsync(int supplierId, int userId);
        Task<BaseResponse<BrandModel>> UpdateBrandAsync(int brandId, UpdateBrandModel model, int userId);
    }
}
