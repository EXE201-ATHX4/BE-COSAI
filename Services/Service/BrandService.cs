using AutoMapper;
using Contract.Repositories.Entity;
using Contract.Repositories.Interface;
using Contract.Services.Interface;
using Core.Base;
using Core.Store;
using Microsoft.EntityFrameworkCore;
using ModelViews.BrandModelViews;
using ModelViews.SupplierModelViews;

namespace Services.Service
{
    public class BrandService : IBrandService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public BrandService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<BaseResponse<BrandModel>> CreateBrandAsync(CreateBrandModel model, int userId)
        {
            try
            {
                var user = await _unitOfWork.GetRepository<User>().Entities.FirstOrDefaultAsync(c => c.Id == userId && !c.DeletedTime.HasValue);
                if (user == null)
                {
                    return new BaseResponse<BrandModel>(StatusCodeHelper.Notfound, "400", "User not found");
                }
                var brand = new Brand
                {
                    Name = model.Name
                };
                await _unitOfWork.GetRepository<Brand>().InsertAsync(brand);
                await _unitOfWork.SaveAsync();
                var brandDto = _mapper.Map<BrandModel>(brand);
                return new BaseResponse<BrandModel>(StatusCodeHelper.OK, "200", brandDto);
            }
            catch (Exception ex)
            {
                return new BaseResponse<BrandModel>(StatusCodeHelper.BadRequest, "400", "An error occurred while creating the brand");
            }
        }

        public async Task<BaseResponse<bool>> DeleteBrandAsync(int brandId, int userId)
        {
            try
            {
                var brand = await _unitOfWork.GetRepository<Brand>().Entities.FirstOrDefaultAsync(c => c.Id == brandId);
                var user = await _unitOfWork.GetRepository<User>().Entities.FirstOrDefaultAsync(c => c.Id == userId && !c.DeletedTime.HasValue);
                if (brand == null)
                {
                    return new BaseResponse<bool>(StatusCodeHelper.Notfound, "404", "brand not found");
                }

                // Kiểm tra khóa ngoại ở bảng Product
                var hasProducts = await _unitOfWork.GetRepository<Product>()
                    .Entities.AnyAsync(p => p.Brand.Id == brandId);

                if (hasProducts)
                {
                    return new BaseResponse<bool>(StatusCodeHelper.BadRequest, "409", "Cannot delete brand because it is referenced by products.");
                }
                //brand.DeletedBy = user.UserName;
                //brand.DeletedTime = CoreHelper.SystemTimeNows;
                await _unitOfWork.GetRepository<Brand>().UpdateAsync(brand);
                await _unitOfWork.SaveAsync();
                return new BaseResponse<bool>(StatusCodeHelper.OK, "200", "Deleted successfully");
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool>(StatusCodeHelper.Notfound, "400", "An error occured while retrieving the brand");
            }
        }

        public async Task<BaseResponse<BasePaginatedList<BrandModel>>> GetAllBrandAsync(int pageNumber, int pageSize)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;
                var brand = await _unitOfWork.GetRepository<Brand>().Entities.Where(a => !a.DeletedTime.HasValue).ToListAsync();
                var paginatedItems = brand.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                var brandDtos = _mapper.Map<List<BrandModel>>(paginatedItems);
                int total = brandDtos.Count();
                var brandResults = new BasePaginatedList<BrandModel>(brandDtos, total, pageNumber, pageSize);
                return new BaseResponse<BasePaginatedList<BrandModel>>(StatusCodeHelper.OK, "200", brandResults, "Success");
            }
            catch (Exception ex)
            {
                return new BaseResponse<BasePaginatedList<BrandModel>>(StatusCodeHelper.Notfound, "400", null, "An error occured while retrieving the supplier");
            }
        }

        public Task<BaseResponse<BrandModel>> UpdateBrandAsync(int brandId, UpdateBrandModel model, int userId)
        {
            throw new NotImplementedException();
        }
    }
}
