using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

namespace WebApplication.Controllers
{
    public class HomeController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> Webhook([FromBody] dynamic data)
        {
            try
            {
                var statusUrl = data?.pull_request?.statuses_url;
                if (statusUrl != null) 
                {
                    var client = new HttpClient();
                    client.DefaultRequestHeaders.Add("User-Agent", "Repometric");
                    var content = "{state: \"success\", target_url: \"http://repometric.com\", description: \"Hello Integration\", context: \"Repometric\"}";
                    var contentPost = new StringContent(content, System.Text.Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(statusUrl.ToString(), contentPost);
                    return Json(await response.Content.ReadAsStringAsync());
                }

                return Json(data.ToString());
            }
            catch (Exception e)
            {
                return Json(e.ToString());
            }
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
