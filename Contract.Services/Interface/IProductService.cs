using ModelViews.ProductModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;   

namespace Contract.Services.Interface
{
    public interface IProductService
    {
        Task<bool> CreateProduct(CreateProductModel model);
    }
}
