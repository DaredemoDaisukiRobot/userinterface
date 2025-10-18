using Microsoft.AspNetCore.Mvc;
using userinterface.Services;
using userinterface.Models;

namespace userinterface.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MemoryController : ControllerBase
    {
        private readonly IMemoryService _memoryService;

        public MemoryController(IMemoryService memoryService)
        {
            _memoryService = memoryService;
        }

        [HttpGet("hello")]
        public IActionResult Hello()
        {
            return Ok("helloword");
        }

        // 新增 /Memory/Search/{Start}/{Quantity}
        [HttpGet("Search/{Start}/{Quantity}")]
        public async Task<IActionResult> Search(int Start, int Quantity)
        {
            var result = await _memoryService.SearchAsync(Start, Quantity);
            return Ok(result);
        }
        
    }
}
