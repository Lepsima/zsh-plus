using System.Diagnostics;

internal static class ZSH {
	public static void Run(string cmd) {
		ProcessStartInfo startInfo = new() {  
			FileName = "/bin/zsh",
			Arguments = $"-c '{cmd}'",
		};  
 
		using Process process = Process.Start(startInfo);
		process?.WaitForExit();
	}
}