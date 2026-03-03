using System.Diagnostics;

internal static class ZSH {
	public static void Run(string cmd) {
		Process process = new();
		process.StartInfo.FileName = "/bin/zsh";
		process.StartInfo.Arguments = $"-c \"{cmd}\"";

		process.Start();
		process.WaitForExit();
	}
}