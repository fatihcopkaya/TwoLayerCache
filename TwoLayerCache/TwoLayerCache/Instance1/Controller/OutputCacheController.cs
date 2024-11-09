using Instance1.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Instance1.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class OutputCacheController : ControllerBase
    {
        [TypeFilter(typeof(OutPutCacheActionFilter))]
        [HttpGet("OutputCache")]
        public IActionResult OutputCache(int Id)
        {
            var value = Guid.NewGuid().ToString();
            return Ok(value);
        }
    }
}
