using System;
using System.IO;
using System.Linq;

namespace linterhub
{
	public enum GitSettingsEnum
	{
		GitPath,
		GitHubToken
	}

	public static class GitSettings
	{
		private static string path;
		private static string gitHubToken;
		public static string Path
		{
			get
			{
				if (string.IsNullOrEmpty(path))
				{
					if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GIT_PATH")))
					{
						var enviromentPaths = Environment.GetEnvironmentVariable("path") ?? string.Empty;
						var paths = enviromentPaths.Split(';');
						path = paths.Select(x => System.IO.Path.Combine(x, "git.exe")).FirstOrDefault(File.Exists);
					}
					else
					{
						path = Environment.GetEnvironmentVariable("GIT_PATH");
					}
				}

				return path;
			}
		}

		public static string GitHubToken
		{
			get
			{
				if (string.IsNullOrEmpty(gitHubToken)) {
					gitHubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
				}

				return gitHubToken;
			}
		}
	}
}
