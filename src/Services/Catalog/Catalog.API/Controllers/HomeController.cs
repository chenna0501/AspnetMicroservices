using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class HomeController : ControllerBase
    {
        /*
        [HttpGet(Name ="GetInt")]
        public int Get()
        {
            return 100;
        }
        */

        [HttpGet(Name = "GetbyId")]
        public int GetValue(int Id)
        {
            return Id;
        }
    }
}
