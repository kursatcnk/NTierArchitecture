using App.Repositories;
using App.Repositories.Categories;
using App.Repositories.Products;
using App.Services.Categories.Create;
using App.Services.Products.Create;
using App.Services.Products.Update;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using App.Services.Categories.Dto;

namespace App.Services.Categories
{
    public class CategoryService(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork, IMapper mapper) : ICategoryService
    {
        // Kategori CRUD (Create, Read, Update, Delete) operasyonları için gerekli iş mantığını burada tanımlıyorum.

        // Yeni bir kategori oluşturmak için kullanılan metot.


        public async Task<ServiceResult<CategoryWithProductsDto>> GetCategoryWithProducts(int categoryId)
        {
            var category = await categoryRepository.GetCategoryWithProductsAsync(categoryId);

            if (category is null)
            {
                return ServiceResult<CategoryWithProductsDto>.Fail("Kategori Bulunamadı", HttpStatusCode.NotFound);
            }
            var categoryAsDto = mapper.Map<CategoryWithProductsDto>(category);
            return ServiceResult<CategoryWithProductsDto>.Success(categoryAsDto);
        }

        public async Task<ServiceResult<List<CategoryWithProductsDto>>> GetCategoryWithProducts()
        {
            var category = await categoryRepository.GetCategoryWithProducts().ToListAsync();

            // Tüm listeyi dönüştürmek için Map<List<CategoryWithProductsDto>> çağrısını kullanıyorum.
            var categoryAsDto = mapper.Map<List<CategoryWithProductsDto>>(category);

            return ServiceResult<List<CategoryWithProductsDto>>.Success(categoryAsDto);
        }

        public async Task<ServiceResult<List<CategoryDto>>> GetAllListAsync()
        {
            var categories = await categoryRepository.GetAll().ToListAsync();
            var categoriesAsDto = mapper.Map<List<CategoryDto>>(categories);
            return ServiceResult<List<CategoryDto>>.Success(categoriesAsDto);
        }

        public async Task<ServiceResult<CategoryDto>> GetByIdAsync(int id)
        {
            var category = await categoryRepository.GetByIdAsync(id);
            if (category is null)
            {
                return ServiceResult<CategoryDto>.Fail("Kategori Bulunamadı", HttpStatusCode.NotFound);
            }
            var categoryAsDto = mapper.Map<CategoryDto>(category);
            return ServiceResult<CategoryDto>.Success(categoryAsDto);
        }
        public async Task<ServiceResult<int>> CreateAsync(CreateCategoryRequest request)
        {
            var anyCategory = await categoryRepository.Where(x => x.Name == request.Name).AnyAsync();

            if (anyCategory)
            {
                return ServiceResult<int>.Fail("Kategori Zaten Bulunmaktadır", HttpStatusCode.NotFound);
            }
            // Gelen talep (request) nesnesinden bir kategori oluşturuyorum.
            var newCategory = mapper.Map<Category>(request);

            // Yeni kategoriyi veri deposuna ekliyorum.
            await categoryRepository.AddAsync(newCategory);

            // Yapılan değişiklikleri kalıcı hale getirmek için UnitOfWork kullanıyorum.
            await unitOfWork.SaveChangesAsync();

            // İşlem sonucunda başarıyla oluşturulan kategorinin Id'sini döndürüyorum.
            return ServiceResult<int>.SuccessAsCreated(newCategory.Id, $"api/categories/{newCategory.Id}");
        }

        public async Task<ServiceResult> UpdateAsync(int id, UpdateCategoryRequest request)
        {
            //var category = await categoryRepository.GetByIdAsync(id);

            //if (category == null)
            //{
            //    return ServiceResult.Fail(errorMessage: "Güncellenecek kategori bulunamadı.", HttpStatusCode.NotFound);
            //}

            var isCategoryNameExist = await categoryRepository
                .Where(x => x.Name == request.Name && x.Id != id)
                .AnyAsync();

            if (isCategoryNameExist)
            {
                return ServiceResult.Fail(errorMessage: "Kategori ismi veritabanında bulunmaktadır.", HttpStatusCode.BadRequest);
            }

            var category = mapper.Map<Category>(request);
            category.Id = id;

            categoryRepository.Update(category);
            await unitOfWork.SaveChangesAsync();

            return ServiceResult.Success(HttpStatusCode.NoContent);
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            var category = await categoryRepository.GetByIdAsync(id);
            //if (category is null)
            //{
            //    return ServiceResult.Fail("Kategori Bulunamadı", HttpStatusCode.NotFound);
            //}
            categoryRepository.Delete(category);
            await unitOfWork.SaveChangesAsync();
            return ServiceResult.Success(HttpStatusCode.NoContent);

        }


    }
}