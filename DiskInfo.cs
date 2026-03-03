internal class Disk {
    public string Name { get; init; }
    public string Model { get; init; }
    public long SizeBytes { get; init; }
    public List<Partition> Partitions { get; } = [];
}

internal class Partition {
    public string MountPoint { get; init; }
    public long TotalBytes { get; init; }
    public long FreeBytes { get; init; }
    public long UsedBytes => TotalBytes - FreeBytes;
}

internal static class DiskInfo {
    public static List<Disk> GetDisks() {
        List<Disk> disks = [];
        var mounts = ParseMounts();

        foreach (string dir in Directory.GetDirectories("/sys/block")) {
            string name = Path.GetFileName(dir);

            if (name.StartsWith("loop") ||
                name.StartsWith("ram"))
                continue;

            Disk disk = new() {
                Name = name,
                Model = ReadIfExists($"{dir}/device/model"),
                SizeBytes = GetDiskSizeBytes(dir)
            };

            IEnumerable<string> partitions = Directory.GetDirectories(dir)
                .Where(d => Path.GetFileName(d).StartsWith(name));

            foreach (string partDir in partitions) {
                string partName = Path.GetFileName(partDir);

                (string Device, string MountPoint) mount = mounts.FirstOrDefault(m => m.Device.EndsWith(partName));
                if (mount.Device == null || mount.MountPoint == null) continue;
                try {
                    DriveInfo drive = new(mount.MountPoint);

                    disk.Partitions.Add(new Partition {
                        MountPoint = mount.MountPoint,
                        TotalBytes = drive.TotalSize,
                        FreeBytes = drive.AvailableFreeSpace
                    });
                }
                catch {
                    // ignored
                }
            }

            disks.Add(disk);
        }

        return disks;
    }

    private static long GetDiskSizeBytes(string sysBlockPath) {
        var sectorsPath = Path.Combine(sysBlockPath, "size");
        if (!File.Exists(sectorsPath))
            return 0;

        long sectors = long.Parse(File.ReadAllText(sectorsPath).Trim());
        return sectors * 512;
    }

    private static string ReadIfExists(string path) {
        return File.Exists(path)
            ? File.ReadAllText(path).Trim()
            : "Unknown";
    }

    private static List<(string Device, string MountPoint)> ParseMounts() {
        return File.ReadAllLines("/proc/self/mounts")
            .Select(line => line.Split(' '))
            .Where(parts => parts.Length >= 2 && parts[0].StartsWith("/dev/"))
            .Select(parts => (parts[0], parts[1]))
            .ToList();
    }
}