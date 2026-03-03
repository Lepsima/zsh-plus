using System.Reflection;

Profile profile = Profile.GetByHostName();
string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

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

string sedCommand = "sed -i";
sedCommand += $" -e 's|&B0|{"huh",-21}|g'";
sedCommand += $" -e 's|&B1|{"huh",-10}|g'";
sedCommand += $" -e 's|&B2|{"huh",-10}|g'";
sedCommand += $" {dir}{FastFetch_Icon}";
ZSH.Run(sedCommand);

List<Display> displays = DisplayInfo.GetDisplays();
foreach (Display d in displays) {
	Console.WriteLine($"{d.Resolution}" + $" ({d.Connector})");
}


DiskInfo.PrintDiskInfo(profile, $" {dir}{FastFetch_Icon}");

ZSH.Run("mkdir -p ~/.config/fastfetch/");
ZSH.Run($"sudo cp {dir}{FastFetch_Config} ~/.config/fastfetch/config.jsonc");
ZSH.Run($"sudo cp {dir}{FastFetch_Icon} ~/.config/fastfetch/penrose.txt");
ZSH.Run($"source {dir}/main.zsh");

return;

float RamPercent((float total, float free) p) {
	return p.free / p.total * 100;
}