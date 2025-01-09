using App.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Services.Filters
{
    public class NotFoundFilter<T, TId>(IGenericRepository<T, TId> genericRepository) : Attribute, IAsyncActionFilter where T : class where TId : struct
    {

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {



            var idValue = context.ActionArguments.TryGetValue("id", out var idAsObject) ? idAsObject : null;

            if (idValue  is not TId id)
            {
                await next();
                return;

            }        
            if (await genericRepository.AnyAsync(id))
            {
                await next();
                return;
            }


            var entityName = typeof(T).Name;
            var actionName = context.ActionDescriptor.RouteValues["action"];

            var result = ServiceResult.Fail($"Data Bulunamamıştır.({entityName})({actionName}).");
            context.Result = new NotFoundObjectResult(result);


            await next();

        }
    }
}
