namespace ContentConcateneter.Downloader;

public class HttpDownloader: IDownloader
{
	public async Task Download(string url, string outPath, string name)
	{
		var httpClient = new HttpClient();
		// httpClient.Timeout = new TimeSpan(0, 30, 0);
		var filename = Path.GetFileName(new Uri(url).LocalPath);
		var response = await httpClient.GetAsync(url);
		await using var stream = await response.Content.ReadAsStreamAsync();
		if (!Directory.Exists(outPath))
			Directory.CreateDirectory(outPath);
		var filePath = Path.Combine(outPath, filename);
		var fileInfo = new FileInfo(filePath);
		await using var fileStream = fileInfo.OpenWrite();
		await stream.CopyToAsync(fileStream);
	}
}