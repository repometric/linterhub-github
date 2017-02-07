using System;
using System.Text;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebApplication.Utils
{
    public class Helper
    {
        private IConfiguration Configuration { get; }
        private ILogger Log { get; }

        public Helper(IConfiguration configuration, ILogger log)
        {
            Configuration = configuration;
            Log = log;
        } 

        public string GetWorkingDirectory(string sha)
        {
            var workingDirectory = Path.Combine(Directory.GetCurrentDirectory(), Configuration["TempPath"], sha);
            if (!Directory.Exists(workingDirectory))
            {
                Directory.CreateDirectory(workingDirectory);
            }

            return workingDirectory;
        }

        public void DeleteWorkingDirectory(string workingDirectory)
        {
            try
            {
                Directory.Delete(workingDirectory, true);
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        public void RunGit(dynamic data, string workingDirectory)
        {
            string gitUrl = data.pull_request.head.repo.git_url;
            string branch = data.pull_request.head.@ref;
            var git = Configuration["GitPath"];
            var gitResult = new CmdWrapper().RunExecutable(git, $"clone {gitUrl} -b {branch} {workingDirectory}", workingDirectory);
            if (gitResult.ExitCode == -1)
            {
                throw new Exception("GIT clone error.", gitResult.RunException);
            }
        }

        public bool RunAnalysis(string workingDirectory)
        {
            var cliResult = new CmdWrapper().RunExecutable("/bin/bash", "-c \"jshint " + workingDirectory + "\"", workingDirectory);
            if (cliResult.ExitCode == -1)
            {
                throw new Exception("CLI analysis error.", cliResult.RunException);
            }

            var output = cliResult.Output.ToString();
            return !string.IsNullOrEmpty(output.Trim());
        }

        public async Task<string> PostStatus(string statusUrl, string status, string description)
        {
            var token = Configuration["GitHubToken"];
            var name = Configuration["GitHubName"];
            var url = Configuration["GitHubUrl"];
            var content = new
            {
                state = status,
                target_url = url,
                description = description,
                context = name
            };
            var json = JsonConvert.SerializeObject(content);
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", name);
                client.DefaultRequestHeaders.Add("Authorization", $"token {token}");
                var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(statusUrl, stringContent);

                Log.LogInformation($"Status update: {status}. Result: {response.StatusCode}");

                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}