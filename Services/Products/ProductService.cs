using App.Repositories;
using App.Repositories.Products;
using App.Services.ExceptionHandlers;
using App.Services.Products.Create;
using App.Services.Products.Update;
using App.Services.Products.UpdateStock;
using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace App.Services.Products
{
    public class ProductService(IProductRepository productRepository, IUnitOfWork unitOfWork, IMapper mapper) : IProductService
    {
        public async Task<ServiceResult<List<ProductDto>>> GetTopPriceProductsAsync(int count)
        {
            var products = await productRepository.GetTopPriceProductsAsync(count);

            //var productsAsDto = products.Select(p => new ProductDto(p.Id, p.Name, p.Price, p.Stock)).ToList();

            var productsAsDto = mapper.Map<List<ProductDto>>(products);

            return new ServiceResult<List<ProductDto>>()
            {
                Data = productsAsDto
            };
        }


        public async Task<ServiceResult<List<ProductDto>>> GetAllListAsync()
        {
            var products = await productRepository.GetAll().ToListAsync();
            //mapper üzerinden dto nesnesi ile productsı geçiyorum, databasedeki isimleri ile propertyler aynı olduğu için ekstra konfigurasyon ihtiyacı yok, otomatik mapliyor.
            var productsAsDto = mapper.Map<List<ProductDto>>(products);
            return ServiceResult<List<ProductDto>>.Success(productsAsDto);
        }

        public async Task<ServiceResult<List<ProductDto>>> GetPagedAllListAsync(int pageNumber, int pageSize)
        {

            var products = await productRepository.GetAll().Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            //Bu bloğu manuel mapping yaparken kullanmıştım, automapper ile revize ettiğim için kullanmıyorum.
            //var productsAsDto = products.Select(p => new ProductDto(p.Id, p.Name, p.Price, p.Stock)).ToList();


            //mapper üzerinden dto nesnesi ile productsı geçiyorum, databasedeki isimleri ile propertyler aynı olduğu için ekstra konfigurasyon ihtiyacı yok, otomatik mapliyor.
            var productsAsDto = mapper.Map<List<ProductDto>>(products);
            return ServiceResult<List<ProductDto>>.Success(productsAsDto);
        }

        public async Task<ServiceResult<ProductDto?>> GetByIdAsync(int id)
        {
            var product = await productRepository.GetByIdAsync(id);

            if (product is null)
            {
                return ServiceResult<ProductDto?>.Fail("Ürün Bulunamadı", HttpStatusCode.NotFound);
            }

            //var productAsDto = new ProductDto(product!.Id, product.Name, product.Price, product.Stock);
            var productAsDto = mapper.Map<ProductDto>(product);

            return ServiceResult<ProductDto>.Success(productAsDto)!;

        }
        public async Task<ServiceResult<CreateProductResponse>> CreateAsync(CreateProductRequest request)
        {
            //Kontrol amacıyla exception fırlattım
            //throw new CriticalException("Kritik Seviye Hata Meydana geldi");

            //Validasyonu doldurmadan önce yapıyorum ki önce kontrol, sonra işlem olsun.

            var anyProduct = await productRepository.Where(x => x.Name == request.Name).AnyAsync();

            if (anyProduct)
            {
                return ServiceResult<CreateProductResponse>.Fail("Ürün Zaten Bulunmaktadır", HttpStatusCode.NotFound);
            }
            var product = mapper.Map<Product>(request);


            await productRepository.AddAsync(product);
            await unitOfWork.SaveChangesAsync();
            return ServiceResult<CreateProductResponse>.SuccessAsCreated(new CreateProductResponse(product.Id), $"api/product/{product.Id}");



        }
        public async Task<ServiceResult> UpdateAsync(int id, UpdateProductRequest request)
        {
            //Kontrolü filterdan sağladdığım için burada gerek yok
            //var product = await productRepository.GetByIdAsync(id);

            //if (product == null)
            //{
            //    return ServiceResult.Fail("Ürün Bulunamadı", HttpStatusCode.NotFound);
            //}


            var isProductNameExist = await productRepository.Where(x => x.Name == request.Name && x.Id != id).AnyAsync();

            if (isProductNameExist)
            {
                return ServiceResult.Fail("Ürün Zaten Bulunmaktadır", HttpStatusCode.BadRequest);
            }


            //product.Name = request.Name;
            //product.Price = request.Price;
            //product.Stock = request.Stock;


            var product = mapper.Map<Product>(request);
            product.Id = id;


            productRepository.Update(product);
            await unitOfWork.SaveChangesAsync();

            return ServiceResult.Success(HttpStatusCode.NoContent);
        }

        public async Task<ServiceResult> UpdateStockAsync(UpdateProductStockRequest request)
        {
            var product = await productRepository.GetByIdAsync(request.ProductId);
            if (product is null)
            {
                return ServiceResult.Fail("Ürün Bulunamadı", HttpStatusCode.NotFound);
            }
            product.Stock = request.Quantity;
            productRepository.Update(product);
            await unitOfWork.SaveChangesAsync();
            return ServiceResult.Success(HttpStatusCode.NoContent);

        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            var product = await productRepository.GetByIdAsync(id);

            //Product değeri filterdan geleceği için burada kontrol etme gereği duymuyorum.
            //if (product == null)
            //{
            //    return ServiceResult.Fail("Ürün Bulunamadı", HttpStatusCode.NotFound);
            //}

            productRepository.Delete(product);
            await unitOfWork.SaveChangesAsync();
            return ServiceResult.Success(HttpStatusCode.NoContent);

        }


    }
}
