internal class Display(string card, string connector, string resolution) {
	public string Card { get; set; } = card;
	public string Connector { get; } = connector;
	public string Resolution { get; } = resolution;
}

internal static class DisplayInfo {
	public static List<Display> GetDisplays() {
		List<Display> result = [];
		const string drmPath = "/sys/class/drm";
		
		if (!Directory.Exists(drmPath))
			return result;

		IEnumerable<string> cards = Directory.GetDirectories(drmPath)
			.Where(d => Path.GetFileName(d).StartsWith("card"));

		result.AddRange(from card in cards
			let entries = Directory.GetDirectories(card)	 from entry in entries
			let connectorName = Path.GetFileName(entry)		 where connectorName.Contains('-')
			let statusFile = Path.Combine(entry, "status")	 where File.Exists(statusFile)
			let status = File.ReadAllText(statusFile).Trim() where status == "connected"
			let modesFile = Path.Combine(entry, "modes")	 where File.Exists(modesFile)
			let modes = File.ReadAllLines(modesFile)		 where modes.Length != 0
			select new Display(Path.GetFileName(card), connectorName, modes[0]));

		return result;
	}
}