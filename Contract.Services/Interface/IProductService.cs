using Core.Base;
using ModelViews.ProductModelViews;
using ModelViews.SupplierModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;   

namespace Contract.Services.Interface
{
    public interface IProductService
    {
        Task<BaseResponse<BasePaginatedList<ProductModel>>> GetAllProductAsync(int pageNumber, int pageSize, int? productId = null, int? supplierId = null, int? brandId = null, int? categoryId = null, string? name = null);
        Task<BaseResponse<ProductModel>> CreateProductAsync(CreateProductModel model, int userId);
        Task<BaseResponse<bool>> DeleteProductAsync(int productId, int userId);
        Task<BaseResponse<ProductModel>> UpdateProductAsync(int productId, UpdateProductModel model, int userId);
    }
}
