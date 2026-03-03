internal abstract class FetchInfo {
	public static string GetDistro() {
		string[] lines = File.ReadAllLines("/etc/os-release");
		string pretty = lines.FirstOrDefault(l => l.StartsWith("PRETTY_NAME="));
		return pretty?.Split('=')[1].Trim('"') ?? "Unknown";
	}
	
	public static string GetCPU() {
		string[] lines = File.ReadAllLines("/proc/cpuinfo");
		string model = lines.FirstOrDefault(l => l.StartsWith("model name"));
		return model?.Split(':')[1].Trim() ?? "Unknown CPU";
	}
	
	public static (long totalMb, long usedMb) GetMemory() {
		string[] lines = File.ReadAllLines("/proc/meminfo");

		long total = long.Parse(lines.First(l => l.StartsWith("MemTotal"))
			.Split(':')[1]
			.Trim()
			.Split(' ')[0]);

		long available = long.Parse(lines.First(l => l.StartsWith("MemAvailable"))
			.Split(':')[1]
			.Trim()
			.Split(' ')[0]);

		long used = total - available;

		return (total / 1024, used / 1024);
	}
	
	public static string GetKernel() {
		const string path = "/proc/sys/kernel/osrelease";
		return File.Exists(path) ? "Linux " + File.ReadAllText(path).Trim() : "Unknown";
	}
	
	public static List<string> GetGPUs() {
		List<string> result = [];
		const string basePath = "/proc/driver/nvidia/gpus";

		if (!Directory.Exists(basePath))
			return result;

		result.AddRange(Directory.GetDirectories(basePath)
			.Select(dir => Path.Combine(dir, "information"))
			.Where(File.Exists)
			.Select(File.ReadAllLines)
			.Select(lines => lines.FirstOrDefault(l => l.StartsWith("Model:")))
			.Where(modelLine => modelLine != null)
			.Select(modelLine => modelLine.Split(':')[1].Trim()));

		return result;
	}
	
	public static string GetDesktopEnvironment() {
		string[] vars = {
			"XDG_CURRENT_DESKTOP",
			"DESKTOP_SESSION",
			"GDMSESSION"
		};

		foreach (string v in vars) {
			string value = Environment.GetEnvironmentVariable(v);
			if (!string.IsNullOrWhiteSpace(value)) 
				return value;
		}

		return "Unknown";
	}
}