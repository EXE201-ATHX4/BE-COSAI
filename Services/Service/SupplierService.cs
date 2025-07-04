using AutoMapper;
using Castle.Components.DictionaryAdapter.Xml;
using Contract.Repositories.Entity;
using Contract.Repositories.Interface;
using Contract.Services.Interface;
using Core.Base;
using Core.Store;
using Core.Utils;
using Microsoft.EntityFrameworkCore;
using ModelViews.SupplierModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Service
{
    public class SupplierService : ISupplierService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public SupplierService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }
        public async Task<BaseResponse<SupplierModel>> CreateSupplierAsync(CreateSupplierModel model, int userId)
        {
            try
            {
                var user = await _unitOfWork.GetRepository<User>().Entities.FirstOrDefaultAsync(c => c.Id == userId && !c.DeletedTime.HasValue);
                if (user == null )
                {
                    return new BaseResponse<SupplierModel>(StatusCodeHelper.Notfound, "400", "User not found");
                }
                var supplier = new Supplier
                {
                    Name = model.Name,
                    PhoneNumber = model.PhoneNumber,
                    CreatedTime = CoreHelper.SystemTimeNow,
                    CreatedBy = user.UserName
                };
                await _unitOfWork.GetRepository<Supplier>().InsertAsync(supplier);
                await _unitOfWork.SaveAsync();
                var supplierDto = _mapper.Map<SupplierModel>(supplier);
                return new BaseResponse<SupplierModel>(StatusCodeHelper.OK, "200", supplierDto);
            }
            catch (Exception ex) 
            {
                return new BaseResponse<SupplierModel>(StatusCodeHelper.BadRequest, "400", "An error occurred while creating the supplier");
            }
        }

        public async Task<BaseResponse<bool>> DeleteSuppilerAsync(int supplierId, int userId)
        {
            try
            {
                var supplier = await _unitOfWork.GetRepository<Supplier>().Entities.FirstOrDefaultAsync(c => c.Id == supplierId);
                var user = await _unitOfWork.GetRepository<User>().Entities.FirstOrDefaultAsync(c => c.Id == userId && !c.DeletedTime.HasValue);
                if (supplier == null)
                {
                    return new BaseResponse<bool>(StatusCodeHelper.Notfound, "404", "Supplier not found");
                }

                // Kiểm tra khóa ngoại ở bảng Product
                var hasProducts = await _unitOfWork.GetRepository<Product>()
                    .Entities.AnyAsync(p => p.Supplier.Id == supplierId);

                if (hasProducts)
                {
                    return new BaseResponse<bool>(StatusCodeHelper.BadRequest, "409", "Cannot delete supplier because it is referenced by products.");
                }
                supplier.DeletedBy = user.UserName;
                supplier.DeletedTime = CoreHelper.SystemTimeNows;
                await _unitOfWork.GetRepository<Supplier>().UpdateAsync(supplier);
                await _unitOfWork.SaveAsync();
                return new BaseResponse<bool>(StatusCodeHelper.OK, "200", "Deleted successfully");
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool>(StatusCodeHelper.Notfound, "400", "An error occured while retrieving the supplier");
            }
        }

        public async Task<BaseResponse<BasePaginatedList<SupplierModel>>> GetAllSupplierAsync(int pageNumber, int pageSize)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;
                var supplier = _unitOfWork.GetRepository<Supplier>().Entities.Where(a => !a.DeletedTime.HasValue).AsQueryable();
                var paginatedItems = supplier.Skip((pageNumber -1) * pageSize).Take(pageSize).ToList();
                var supplierDtos = _mapper.Map<List<SupplierModel>>(paginatedItems);
                int total = supplierDtos.Count();
                var supplierResults = new BasePaginatedList<SupplierModel>(supplierDtos, total, pageNumber, pageSize);
                return new BaseResponse<BasePaginatedList<SupplierModel>>(StatusCodeHelper.OK, "200", supplierResults, "Success");
            }
            catch (Exception ex) 
            {
                return new BaseResponse<BasePaginatedList<SupplierModel>>(StatusCodeHelper.Notfound, "400", null, "An error occured while retrieving the supplier");
            }
        }
        public async Task<BaseResponse<SupplierModel>> UpdateSupplierAsync(int supplierId, UpdateSupplierModel model, int userId)
        {
            try
            {
                var supplier = await _unitOfWork.GetRepository<Supplier>().Entities.FirstOrDefaultAsync(c => c.Id == supplierId);
                var user = await _unitOfWork.GetRepository<User>().Entities.FirstOrDefaultAsync(c => c.Id == userId && !c.DeletedTime.HasValue);
                if (supplier == null)
                {
                    return new BaseResponse<SupplierModel>(StatusCodeHelper.Notfound, "400","Supplier not found");
                }
                if (!string.IsNullOrWhiteSpace(model.Name)) {
                    supplier.Name = model.Name;
                }
                if (!string.IsNullOrWhiteSpace(model.PhoneNumber))
                {
                    supplier.PhoneNumber = model.PhoneNumber;
                }
                supplier.LastUpdatedBy = user.UserName;
                supplier.LastUpdatedTime = CoreHelper.SystemTimeNow;
                await _unitOfWork.GetRepository<Supplier>().UpdateAsync(supplier);
                await _unitOfWork.SaveAsync();
                var supplierDto = _mapper.Map<SupplierModel>(supplier);
                return new BaseResponse<SupplierModel>(StatusCodeHelper.OK, "200", supplierDto, "Supplier updated successfully");
            }
            catch (Exception ex)
            {
                return new BaseResponse<SupplierModel>(StatusCodeHelper.Notfound, "400", "An error occured while retrieving the supplier");
            }
         }
    }
}
