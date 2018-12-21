using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Andoromeda.CleosNet.Agent.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return Content("Cleos.Net is running...");
        }
    }
}
