using App.Repositories.Categories;
using App.Repositories.Products;
using App.Services.Categories.Dto;
using App.Services.Products.Create;
using App.Services.Products.Update;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Services.Products
{
    public class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {


            CreateMap<CategoryDto, Category>().ReverseMap();
            // Bir ürün (Product) nesnesini bir ürün DTO'suna (ProductDto) ve tam tersine dönüştürmek için bir haritalama oluşturuyorum.
            CreateMap<Product, ProductDto>().ReverseMap();

            // CreateProductRequest nesnesini Product nesnesine dönüştürmek için özel bir haritalama tanımlıyorum.
            // Özellikle, hedef nesnedeki Name özelliğini, kaynak nesnedeki Name özelliğinin küçük harfe dönüştürülmüş hâlinden alıyorum.
            CreateMap<CreateProductRequest, Product>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.ToLowerInvariant()));

            CreateMap<UpdateProductRequest, Product>()
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.ToLowerInvariant()));

        }
    }
}
