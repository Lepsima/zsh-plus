using System.Reflection;

const string FastFetch_Config = "/fastfetch.conf";
const string FastFetch_Icon = "/fastfetch.txt";

int w = Console.WindowWidth;
Console.WriteLine("I am " + w + " characters wide.");

Console.WriteLine(FetchInfo.GetDistro());
Console.WriteLine(FetchInfo.GetCPU());
foreach (string gpu in FetchInfo.GetGPUs()) {
	Console.WriteLine(gpu);
}
Console.WriteLine(FetchInfo.GetDesktopEnvironment());

Console.WriteLine(FetchInfo.GetKernel());

Console.WriteLine(RamPercent(FetchInfo.GetMemory()));

List<Display> displays = DisplayInfo.GetDisplays();
foreach (Display d in displays) {
	Console.WriteLine($"{d.Resolution}" + $" ({d.Connector})");
}

string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
Console.WriteLine(dir + "");

PrintDiskInfo();
ApplyFastfetchConfig();
ZSH.Run($"source {dir}/main.zsh");

return;

void PrintDiskInfo() {
	string[] values = new string[9];
	for (int i = 0; i < 9; i++) values[i] = "??";

	List<Disk> disks = DiskInfo.GetDisks();
	Disk disk = disks.FirstOrDefault(d => d.Name == "sdc");
	if (disk == null) return;
	
	values[0] = BytesToGigaBytes(disk.SizeBytes).ToString().PadLeft(3);
	
	Partition rootPart = disk.Partitions.FirstOrDefault(p => p.MountPoint == "/");
	if (rootPart != null) {
		int percent = PercentUsed(rootPart.FreeBytes, rootPart.TotalBytes);
		values[1] = ("[" + percent).PadLeft(4);
		values[2] = BytesToGigaBytes(rootPart.TotalBytes - rootPart.FreeBytes).ToString().PadLeft(3);
		values[3] = BytesToGigaBytes(rootPart.TotalBytes).ToString().PadLeft(3);
		values[4] = CreateBar(percent, 20);
	}

	Partition homePart = disk.Partitions.FirstOrDefault(p => p.MountPoint == "/home");
	if (homePart != null) {
		int percent = PercentUsed(homePart.FreeBytes, homePart.TotalBytes);
		values[5] = ("[" + percent).PadLeft(4);
		values[6] = BytesToGigaBytes(homePart.TotalBytes - homePart.FreeBytes).ToString().PadLeft(3);
		values[7] = BytesToGigaBytes(homePart.TotalBytes).ToString().PadLeft(3);
		values[8] = CreateBar(percent, 20);
	}

	string sedCommand = "sed -i";
	for (int i = 0; i < 9; i++) sedCommand += $" -e 's/&{i + 1}/{values[i]}/g'";
	sedCommand += $" {dir}{FastFetch_Icon}";
	
	ZSH.Run(sedCommand);
}

void ApplyFastfetchConfig() { 
	ZSH.Run($"sudo cp {dir}{FastFetch_Config} ~/.config/fastfetch/config.jsonc");
	ZSH.Run($"sudo cp {dir}{FastFetch_Icon} ~/.config/fastfetch/penrose.txt");
}

string CreateBar(int percent, int length) {
	int count = (int)MathF.Floor((float)percent / 100 * length);
	return "".PadRight(count, '█').PadRight(length);
}

int PercentUsed(long free, long total) {
	float frac = BytesToGigaBytes(free) / (float)BytesToGigaBytes(total);
	return 100 - (int)(frac * 100);
}

int BytesToGigaBytes(long bytes) {
	return (int)(bytes / 1073741824L);
}

float RamPercent((float total, float free) p) {
	return p.free / p.total * 100;
}