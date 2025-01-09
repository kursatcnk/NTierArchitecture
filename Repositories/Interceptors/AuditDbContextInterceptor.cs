using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Repositories.Interceptors
{
    public class AuditDbContextInterceptor : SaveChangesInterceptor
    {
        // EntityState'e göre davranışları tanımlayan bir sözlük oluşturuyorum.
        private static readonly Dictionary<EntityState, Action<DbContext, IAuditEntity>> Behaviors = new()
        {
            {EntityState.Added, AddBehavior }, // Yeni bir entity eklendiğinde AddBehavior uygulanır.
            {EntityState.Modified, ModifiedBehavior } // Mevcut bir entity güncellendiğinde ModifiedBehavior uygulanır.
        };

        // Yeni eklenen bir entity için Created tarihini ayarlıyorum.
        private static void AddBehavior(DbContext context, IAuditEntity auditEntity)
        {
            auditEntity.Crated = DateTime.Now; // Entity'nin oluşturulma zamanını ayarlıyorum.
            context.Entry(auditEntity).Property(x => x.Updated).IsModified = false; // Updated alanını değiştirilmemiş olarak işaretliyorum.
        }

        // Güncellenen bir entity için Updated tarihini ayarlıyorum.
        private static void ModifiedBehavior(DbContext context, IAuditEntity auditEntity)
        {
            context.Entry(auditEntity).Property(x => x.Crated).IsModified = false; // Created alanının değişmemesi gerektiğini belirtiyorum.
            auditEntity.Updated = DateTime.Now; // Entity'nin güncellenme zamanını ayarlıyorum.
        }

        // Veritabanına değişiklikleri kaydetmeden önce tetiklenen metod.
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            // ChangeTracker'daki tüm entity girişlerini dolaşıyorum.
            foreach (var entityEntry in eventData.Context.ChangeTracker.Entries().ToList())
            {
                // Entity IAuditEntity değilse devam ediyorum.
                if (entityEntry.Entity is not IAuditEntity auditEntity) continue;


                if (entityEntry.State is not (EntityState.Added or EntityState.Modified)) continue;
                Behaviors[entityEntry.State](eventData.Context, auditEntity);



            }

            // Kaydetme işlemini üst sınıfa devrediyorum.
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
