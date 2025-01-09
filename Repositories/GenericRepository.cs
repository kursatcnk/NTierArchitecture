using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace App.Repositories
{
    // Bu sınıfta, GenericRepository adında bir sınıf yazdım. 
    // Bu sınıf, belirli bir veri türü (T) üzerinde CRUD işlemleri yapmak için kullanılacak.
    // `T` bir sınıf olmalı ve AppDbContext üzerinden DbSet'e erişim sağlıyorum.

    public class GenericRepository<T, TId>(AppDbContext context) : IGenericRepository<T, TId> where T : BaseEntity<TId> where TId : struct
    {

        protected AppDbContext Context = context;

        // T tipinde bir DbSet oluşturuyorum. context üzerinden Set<T> metoduyla belirli tabloya erişim sağlıyorum.
        private readonly DbSet<T> _dbSet = context.Set<T>();

        // Tüm kayıtları sorgulamak için bir yöntem yazdım. IQueryable döndürerek LINQ sorgularına uygun hale getiriyorum.
        // Ayrıca .AsNoTracking() ekledim ki sorgu, Entity Framework'ün takip mekanizmasını kullanmasın.

        public Task<bool> AnyAsync(TId id) => _dbSet.AnyAsync(x => x.Id!.Equals(id));

        public IQueryable<T> GetAll() => _dbSet.AsQueryable().AsNoTracking();

        // Filtreli sorgular için bir yöntem. LINQ Expression kullanarak istediğim koşula göre kayıtları sorgulayabiliyorum.
        // Yine .AsNoTracking() ile performansı artırmaya çalışıyorum.
        public IQueryable<T> Where(Expression<Func<T, bool>> predicate) => _dbSet.Where(predicate).AsNoTracking();

        // Yeni bir kayıt eklemek için bir asenkron metod yazdım. 
        // DbSet'in AddAsync metodunu çağırarak, Entity Framework'ün veritabanına kayıt eklemesini sağlıyorum.
        public async ValueTask AddAsync(T entity) => await _dbSet.AddAsync(entity);

        // Mevcut bir kaydı silmek için bir metod. Kaldırılacak nesneyi DbSet.Remove metoduna veriyorum.
        public void Delete(T entity) => _dbSet.Remove(entity);

        // Belirli bir ID ile kayıt aramak için bir asenkron metod yazdım. 
        // DbSet'in FindAsync metodunu çağırarak, belirttiğim ID ile eşleşen kaydı getiriyorum.
        public ValueTask<T?> GetByIdAsync(int id) => _dbSet.FindAsync(id);

        // Var olan bir kaydı güncellemek için bir metod. 
        // DbSet'in Update metodunu çağırıyorum ve Entity Framework güncellemeyi yapıyor.
        public void Update(T entity) => _dbSet.Update(entity);

    }

}
