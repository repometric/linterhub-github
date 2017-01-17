using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;

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
                    await PostStatus(statusUrl.ToString(), "success", "Sanity check");
                    return Json("Status posted");
                }

                return Json("Unknown status url");
            }
            catch (Exception e)
            {
                return Json(e.ToString());
            }
        }

        private async Task<string> PostStatus(string url, string status, string description)
        {
            var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
            var content = new {
                state = status, 
                target_url = "http://repometric.com", 
                description = description, 
                context = "Repometric"
            };
            var json = JsonConvert.SerializeObject(content);
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Repometric");
                client.DefaultRequestHeaders.Add("Authorization", $"token {token}");
                var contentPost = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, contentPost);
                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}
