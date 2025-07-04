using AutoMapper;
using Contract.Repositories.Entity;
using Contract.Repositories.Interface;
using Contract.Services.Interface;
using Core.Base;
using Core.Store;
using Core.Utils;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ModelViews.OrderDetailModel;
using ModelViews.OrderModelViews;
using ModelViews.ProductModelViews;
using ModelViews.ShippingAddressModel;
using ModelViews.ProductModelViews;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Service
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<BaseResponse<ProductModel>> CreateProductAsync(CreateProductModel model, int userId)
        {
            try
            {
                var user = await _unitOfWork.GetRepository<User>().Entities.FirstOrDefaultAsync(c => c.Id == userId && !c.DeletedTime.HasValue);
                if (user == null)
                {
                    return new BaseResponse<ProductModel>(StatusCodeHelper.Notfound, "400", "User not found");
                }
                var brand = await _unitOfWork.GetRepository<Brand>().Entities.FirstOrDefaultAsync(c => c.Id == model.BrandId);
                var category = await _unitOfWork.GetRepository<Category>().Entities.FirstOrDefaultAsync(c => c.Id == model.CategoryId);
                var supplier = await _unitOfWork.GetRepository<Supplier>().Entities.FirstOrDefaultAsync(c => c.Id == model.SupplierId);
                var product = new Product
                {
                    Name = model.Name,
                    SKU = model.SKU,
                    OriginalPrice = model.OriginalPrice,
                    SalePrice = model.SalePrice,
                    IsOnSale = model.IsOnSale,
                    Description = model.Description,
                    ShortDescription = model.ShortDescription,
                    Ingredients = model.Ingredients,
                    Usage = model.Usage,
                    Brand = brand,
                    Category = category,
                    Supplier = supplier,
                    CreatedTime = CoreHelper.SystemTimeNow,
                    CreatedBy = user.UserName
                };
                await _unitOfWork.GetRepository<Product>().InsertAsync(product);
                await _unitOfWork.SaveAsync();
                var ProductDto = _mapper.Map<ProductModel>(product);
                return new BaseResponse<ProductModel>(StatusCodeHelper.OK, "200", ProductDto);
            }
            catch (Exception ex)
            {
                return new BaseResponse<ProductModel>(StatusCodeHelper.BadRequest, "400", "An error occurred while creating the Product");
            }
        }

        public async Task<BaseResponse<bool>> DeleteProductAsync(int productId, int userId)
        {
            try
            {
                var product = await _unitOfWork.GetRepository<Product>().Entities.FirstOrDefaultAsync(c => c.Id == productId);
                var user = await _unitOfWork.GetRepository<User>().Entities.FirstOrDefaultAsync(c => c.Id == userId && !c.DeletedTime.HasValue);
                if (product == null)
                {
                    return new BaseResponse<bool>(StatusCodeHelper.Notfound, "404", "Product not found");
                }

                // Kiểm tra khóa ngoại ở bảng orderdetail
                var hasProducts = await _unitOfWork.GetRepository<OrderDetail>()
                    .Entities.AnyAsync(p => p.Product.Id == productId);

                if (hasProducts)
                {
                    return new BaseResponse<bool>(StatusCodeHelper.BadRequest, "409", "Cannot delete product because it is referenced by orderdetails.");
                }
                product.DeletedBy = user.UserName;
                product.DeletedTime = CoreHelper.SystemTimeNows;
                await _unitOfWork.GetRepository<Product>().UpdateAsync(product);
                await _unitOfWork.SaveAsync();
                return new BaseResponse<bool>(StatusCodeHelper.OK, "200", "Deleted successfully");
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool>(StatusCodeHelper.Notfound, "400", "An error occured while retrieving the Product");
            }
        }

        public async Task<BaseResponse<BasePaginatedList<ProductModel>>> GetAllProductAsync(int pageNumber, int pageSize, int? productId = null, int? supplierId = null, int? brandId = null, int? categoryId = null, string? name = null)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;

                var ordersQuery = _unitOfWork.GetRepository<Product>()
                    .Entities.Include(o => o.Brand).Include(o => o.Supplier).Include(o => o.Category).Where(c => !c.DeletedTime.HasValue)
                    .AsQueryable();
                // Áp dụng bộ lọc
                if (productId.HasValue)
                {
                    ordersQuery = ordersQuery.Where(b => b.Id == productId);
                }
                if (supplierId.HasValue)
                {
                    ordersQuery = ordersQuery.Where(b => b.Supplier.Id == supplierId);
                }
                if (brandId.HasValue) 
                {
                    ordersQuery = ordersQuery.Where(b => b.Brand.Id == brandId);
                }
                if (categoryId.HasValue) 
                {
                    ordersQuery = ordersQuery.Where(b => b.Category.Id == categoryId);
                }
                if (!string.IsNullOrEmpty(name)) 
                {
                    ordersQuery= ordersQuery.Where(b => b.Name.Contains(name));
                }
                // Áp dụng sắp xếp


                var paginatedItems = ordersQuery.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                var ProductDtos = _mapper.Map<List<ProductModel>>(paginatedItems);
                int total = ProductDtos.Count();
                var ProductResults = new BasePaginatedList<ProductModel>(ProductDtos, total, pageNumber, pageSize);
                return new BaseResponse<BasePaginatedList<ProductModel>>(StatusCodeHelper.OK, "200", ProductResults);
            }
            catch (Exception ex)
            {
                return new BaseResponse<BasePaginatedList<ProductModel>>(StatusCodeHelper.Notfound, "400", "An error occured while retrieving the product");
            }
        }

        public async Task<BaseResponse<ProductModel>> UpdateProductAsync(int productId, UpdateProductModel model, int userId)
        {
            try
            {
                var product = await _unitOfWork.GetRepository<Product>().Entities.Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.Supplier).FirstOrDefaultAsync(c => c.Id == productId);
                var user = await _unitOfWork.GetRepository<User>().Entities.FirstOrDefaultAsync(c => c.Id == userId && !c.DeletedTime.HasValue);
                if (product == null)
                {
                    return new BaseResponse<ProductModel>(StatusCodeHelper.Notfound, "400", "Product not found");
                }
                if (!string.IsNullOrWhiteSpace(model.Name))
                {
                    product.Name = model.Name;
                }
                if (!string.IsNullOrWhiteSpace(model.SKU))
                {
                    product.SKU = model.SKU;
                }
                if (model.OriginalPrice.HasValue)
                {
                    product.OriginalPrice = model.OriginalPrice.Value;
                }
                if (model.SalePrice.HasValue)
                {
                    product.SalePrice = model.SalePrice.Value;

                }
                if (model.IsOnSale.HasValue) 
                {
                    product.IsOnSale = model.IsOnSale.Value;
                }
                if (!string.IsNullOrEmpty(model.Description)) 
                {
                    product.Description = model.Description;
                }
                if (!string.IsNullOrEmpty(model.ShortDescription))
                {
                    product.ShortDescription = model.ShortDescription;
                }
                if (!string.IsNullOrEmpty(model.Ingredients))
                {
                    product.Ingredients = model.Ingredients;
                }
                if (!string.IsNullOrEmpty(model.Usage))
                {
                    product.Usage = model.Usage;
                }
                if (model.BrandId.HasValue && (product.Brand == null || model.BrandId.Value != product.Brand.Id))
                {
                    var brand = await _unitOfWork.GetRepository<Brand>().GetByIdAsync(model.BrandId.Value);
                    if (brand != null)
                        product.Brand = brand;
                }

                // Category
                if (model.CategoryId.HasValue && (product.Category == null || model.CategoryId.Value != product.Category.Id))
                {
                    var category = await _unitOfWork.GetRepository<Category>().GetByIdAsync(model.CategoryId.Value);
                    if (category != null)
                        product.Category = category;
                }

                // Supplier
                if (model.SupplierId > 0 && (product.Supplier == null || model.SupplierId != product.Supplier.Id))
                {
                    var supplier = await _unitOfWork.GetRepository<Supplier>().GetByIdAsync(model.SupplierId);
                    if (supplier != null)
                        product.Supplier = supplier;
                }

                product.LastUpdatedBy = user.UserName;
                product.LastUpdatedTime = CoreHelper.SystemTimeNow;
                await _unitOfWork.GetRepository<Product>().UpdateAsync(product);
                await _unitOfWork.SaveAsync();
                var productDto = _mapper.Map<ProductModel>(product);
                return new BaseResponse<ProductModel>(StatusCodeHelper.OK, "200", productDto, "Product updated successfully");
            }
            catch (Exception ex)
            {
                return new BaseResponse<ProductModel>(StatusCodeHelper.Notfound, "400", "An error occured while retrieving the Product");
            }
        }
    }
}
