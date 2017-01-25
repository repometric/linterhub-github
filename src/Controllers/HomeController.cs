using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using src.Utils;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.IO;

namespace WebApplication.Controllers
{
	public class HomeController : Controller
	{
		private IConfiguration Configuration { get; set; }

		public HomeController(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		[HttpPost]
		public async Task<IActionResult> Webhook([FromBody] dynamic data)
		{
			string action = data?.action;
			string git_url = string.Empty;
			string branch = string.Empty;
			string pullRequestFullName = string.Empty;
			int pullRequestNumber = 0;

			if (action == "opened" || action == "reopened")
			{
				try
				{
					var token = Configuration["GitHubToken"];
					if (string.IsNullOrEmpty(token))
					{
						return Json("Can't find a token");
					}

					if (data?.pull_request != null)
					{
						git_url = data.pull_request.head.repo.git_url;
						branch = data.pull_request.head.@ref;
						pullRequestFullName = data.pull_request.head.repo.full_name;
						pullRequestFullName = pullRequestFullName.Replace("/", "_");
						pullRequestNumber = data.pull_request.number;
					}

					var statusUrl = data?.pull_request?.statuses_url;
					if (statusUrl != null)
					{
						var cmdWrapper = new CmdWrapper();
						var workingDirectory = $@"E:\dotNet\repometric\testing\{pullRequestFullName}_pr_{pullRequestNumber}";
						if (!Directory.Exists(workingDirectory))
						{
							Directory.CreateDirectory(workingDirectory);
						}

						var gitResult = cmdWrapper.RunExecutable(Configuration["GitPath"], $"clone {git_url} -b {branch} {workingDirectory}", workingDirectory);

						if (gitResult.ExitCode == -1)
						{
							return Json(gitResult.RunException);
						}

						//Put here cli call
						await PostStatus(statusUrl.ToString(), token, "pending", "Running analysis");
						Thread.Sleep(TimeSpan.FromSeconds(30));
						await PostStatus(statusUrl.ToString(), token, "success", "Sanity check completed");
						return Json(gitResult.ToString());
					}

					return Json("Unknown status url");
				}
				catch (Exception e)
				{
					return Json(e.ToString());
				}
			}

			return Json("No new pull requestes");
		}

		private async Task<string> PostStatus(string url, string token, string status, string description)
		{
			var content = new
			{
				state = status,
				target_url = "http://repometric.com",
				description = description,
				// context = "Prontsevich"
				context = "Repometric"
			};
			var json = JsonConvert.SerializeObject(content);
			using (var client = new HttpClient())
			{
				// client.DefaultRequestHeaders.Add("User-Agent", "Prontsevich");
				client.DefaultRequestHeaders.Add("User-Agent", "Repometric");
				client.DefaultRequestHeaders.Add("Authorization", $"token {token}");
				var contentPost = new StringContent(json, Encoding.UTF8, "application/json");
				var response = await client.PostAsync(url, contentPost);
				return await response.Content.ReadAsStringAsync();
			}
		}
	}
}
