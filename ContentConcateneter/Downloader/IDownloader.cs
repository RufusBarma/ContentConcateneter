namespace ContentConcateneter.Downloader;

public interface IDownloader
{
	public Task Download(string url, string outPath, string name);
}