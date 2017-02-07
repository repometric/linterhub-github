using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebApplication.Utils;

namespace WebApplication.Controllers
{
	public class HomeController : Controller
	{
		private IConfiguration Configuration { get; }

		private ILogger Log { get; }

		public HomeController(IConfiguration configuration, ILogger<HomeController> log)
		{
			Configuration = configuration;
			Log = log;
		}

		public IActionResult Index()
		{
			return Json("Up and running.");
		}

		[HttpPost]
		public async Task<IActionResult> Webhook([FromBody] dynamic data)
		{
			try
			{
				return await ProcessWebhook(data);
			}
			catch (Exception exception)
			{
				Log.LogError(exception.ToString());
				return StatusCode(500, exception.Message);
			}
		}

		private async Task<IActionResult> ProcessWebhook(dynamic data)
		{
			var helper = new Helper(Configuration, Log);

			string action = data?.action;
			if (action != "opened" && action != "reopened")
			{
				return Json("No new pull requests.");
			}

			var token = Configuration["GitHubToken"];
			if (string.IsNullOrEmpty(token))
			{
				return Json("Can't find a token.");
			}

			if (data?.pull_request == null)
			{
				return Json("Can't find information about pull request.");
			}

			string statusUrl = data?.pull_request?.statuses_url;
			if (string.IsNullOrEmpty(statusUrl))
			{
				return Json("Unknown status url.");
			}

			await helper.PostStatus(statusUrl, "pending", "Running analysis");
			string sha = data.pull_request.head.sha;

			var workingDirectory = helper.GetWorkingDirectory(sha);
			helper.RunGit(data, workingDirectory);
			var haveErrors = helper.RunAnalysis(workingDirectory);
			helper.DeleteWorkingDirectory(workingDirectory);

			if (haveErrors)
			{
				await helper.PostStatus(statusUrl, "error", "There are some errors");
			}
			else
			{
				await helper.PostStatus(statusUrl, "success", "Check completed");
			}

			return Ok("Done");
		}
	}
}
