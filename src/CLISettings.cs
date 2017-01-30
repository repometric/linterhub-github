using System;
using System.IO;
using System.Linq;

namespace linterhub
{
	public enum CLISettingsEnum
	{
		CLIPath
	}

	public static class CLISettings
	{
		private static string path;

		public static string Path
		{
			get
			{
				if (string.IsNullOrEmpty(path))
				{
					path = Environment.GetEnvironmentVariable("CLI_PATH");
				}

				return path;
			}
		}
	}
}
