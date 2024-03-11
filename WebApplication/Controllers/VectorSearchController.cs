using Microsoft.AspNetCore.Mvc;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    public class VectorSearchController : Controller
    {
        private readonly ILogger<VectorSearchController> _logger;
        private readonly IConfiguration configuration;

        public VectorSearchController(ILogger<VectorSearchController> logger, IConfiguration configuration)
        {
            _logger = logger;
            this.configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }


        [HttpPost()]
        public async Task<IActionResult> DoSearch(string searchPrompt)
        {
            var res = await VectorRepository.DoVectorSearch(searchPrompt, this.configuration);
            return View(res);
        }
    }
}
