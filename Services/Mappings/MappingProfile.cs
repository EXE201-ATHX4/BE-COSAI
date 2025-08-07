using AutoMapper;
using Contract.Repositories.Entity;
using ModelViews.BrandModelViews;
using ModelViews.ProductModelViews;
using ModelViews.SupplierModelViews;
using ModelViews.UserModelViews;


namespace Services.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {
            CreateMap<Supplier, SupplierModel>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber));
            CreateMap<Product, ProductModel>()
            .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand != null ? src.Brand.Name : string.Empty))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : string.Empty))
            .ForMember(dest => dest.ProductImages, opt => opt.MapFrom(src => src.ProductImages.Select(pi => pi.ImageUrl).ToList()))
            .ReverseMap()
            .ForMember(dest => dest.Brand, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.Supplier, opt => opt.Ignore());
            CreateMap<UserInfo, UserInfoModel>().ReverseMap();
            CreateMap<UserInfo, CreateUserInfo>().ReverseMap();
            CreateMap<Brand, BrandModel>().ReverseMap();
        }
    }
}
