using AutoMapper;
using WebApplication2.Data.Entities;
using WebApplication2.Models.Category;
using WebApplication2.Models.Product;

namespace WebApplication2.Mapper
{
    public class AppMapperProfile : Profile
    {
        public AppMapperProfile()
        {
            CreateMap<CategoryEntity, CategoryItemViewModel>();
            CreateMap<CategoryCreateViewModel,CategoryEntity>();
            CreateMap<ProductEntity, ProductItemViewModel>()
                .ForMember(x => x.Images, opt => opt.MapFrom(p => p.ProductImages.Select(x => x.Image).ToList()))
                .ForMember(x => x.CategoryName, opt => opt.MapFrom(c => c.Category.Name));
        }
    }
}
