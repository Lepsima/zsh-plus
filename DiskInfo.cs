internal class Disk {
    public string Name { get; init; }
    public string Model { get; init; }
    public long SizeBytes { get; init; }
    public List<Partition> Partitions { get; } = [];

    public string GetFetchValue() {
        return DiskInfo.BytesToGigaBytes(SizeBytes).ToString().PadLeft(3);
    }

    public bool GetPartitionByMount(string mountPoint, out Partition part) {
        part = Partitions.FirstOrDefault(p => p.MountPoint.Equals(mountPoint));
        return part != null;
    }
}

internal class Partition {
    public string MountPoint { get; init; }
    public long TotalBytes { get; init; }
    public long FreeBytes { get; init; }
    public long UsedBytes => TotalBytes - FreeBytes;

    public static string CreateUseBar(float percent, int length) {
        int count = (int)MathF.Floor(percent * length);
        return "".PadRight(count, '█').PadRight(length);
    }
    
    public float GetFreePercent() => (float)(FreeBytes / (double)TotalBytes);
    public float GetUsedPercent() => 1f - GetFreePercent();
    
    public int GetUsedGigabytes() => DiskInfo.BytesToGigaBytes(UsedBytes);
    public int GetTotalGigabytes() => DiskInfo.BytesToGigaBytes(TotalBytes);
    
    private static string LeftPadInt(int value, int padding) {
        return value.ToString().PadLeft(padding);
    }
    
    public string[] GetFetchValues() {
        string[] values = new string[4];
        float percent = GetFreePercent();
        
        values[0] = ((int)(percent * 100) + "").PadLeft(3);
        values[1] = LeftPadInt(GetUsedGigabytes(), 3);
        values[2] = LeftPadInt(GetTotalGigabytes(), 3);
        values[3] = CreateUseBar(percent, 20);

        return values;
    }
}

internal static class DiskInfo {
    internal static int BytesToGigaBytes(long bytes) {
        return (int)(bytes / 1073741824L);
    }

    public static void PrintDiskInfo(Profile profile, string path) {
        List<string> values = [];
        
        if (GetDiskByMountPoint(profile, out Disk disk)) {
            values.Add(disk.GetFetchValue());
            
            if (disk.GetPartitionByMount(profile.FirstPartMount, out Partition rootPart)) 
                values.AddRange(rootPart.GetFetchValues());
            
            if (disk.GetPartitionByMount(profile.SecondPartMount, out Partition homePart)) 
                values.AddRange(homePart.GetFetchValues());
        }

        while (values.Count < 9) values.Add("?");
        
        string sedCommand = "sed -i";
        for (int i = 0; i < values.Count; i++) sedCommand += $" -e 's/&{i + 1}/{values[i]}/g'";
        
        sedCommand += $" -e 's|&A0|{profile.DiskMountPoint,-13}|g'";
        sedCommand += $" -e 's|&A1|{profile.FirstPartMount,-10}|g'";
        sedCommand += $" -e 's|&A2|{profile.SecondPartMount,-10}|g'";
        
        sedCommand += path;
	
        ZSH.Run(sedCommand);
    }

    private static bool GetDiskByMountPoint(Profile profile, out Disk disk) {
        List<(string Device, string MountPoint)> mounts = ParseMounts();
        disk = null;

        (string dir, string name) = Directory.GetDirectories("/sys/block")
            .Select(dir => (dir, name: Path.GetFileName(dir)))
            .FirstOrDefault(dir => dir.name.Equals(profile.DiskMountPoint));

        if (dir == null || name == null) return false;
        
        disk = new Disk {
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

        return true;
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