using Instance1.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Instance1.Attributes
{
    public class OutPutCacheActionFilter : IActionFilter
    {
        private readonly ICacheService _cacheService;

        public OutPutCacheActionFilter([FromKeyedServices("CacheProxyService")] ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public async void OnActionExecuted(ActionExecutedContext context)
        {
            var path = context.HttpContext.Request.Path;
            var queryString = context.HttpContext.Request.QueryString.ToString();
            var fullPath = $"{path}{queryString}";

            if (context.Result is ObjectResult objectResult && objectResult.Value != null)
            {
  
                await _cacheService.SetAsync<string>(fullPath, objectResult.Value.ToString(), x =>
                {
                    x.ExpiryTime = 5;
                });
            }
        }

        public async void OnActionExecuting(ActionExecutingContext context)
        {
            var path = context.HttpContext.Request.Path;
            var queryString = context.HttpContext.Request.QueryString.ToString();
            var fullPath = $"{path}{queryString}";
            
            var value = await _cacheService.GetAsync<string>(fullPath);
            if (value != null)
            {
                context.Result = new ContentResult
                {
                    Content = value,
                    ContentType = "application/json",
                    StatusCode = 200
                };
                return; 
            }
                                 
            
        }
    }
}
