internal class Profile {
	private static readonly Profile[] profiles = [
		new()  {
			HostName = "LPLaptop",
			DiskMountPoint = "nvme0n1",
			FirstPartMount = "/",
			SecondPartMount = "/mnt/1MB",
		},
		
		new()  {
			HostName = "???",
			DiskMountPoint = "sdc",
			FirstPartMount = "/",
			SecondPartMount = "/home",
		},
	];

	public static Profile GetByHostName() {
		return profiles.FirstOrDefault(p => p.HostName.Equals(Environment.MachineName));
	}
	
	public string HostName { get; init; }
	public string DiskMountPoint { get; init; }
	
	public string FirstPartMount { get; init; }
	public string SecondPartMount { get; init; }
}