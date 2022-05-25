using Client.Telegram.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace ContentConcateneter.Downloader;

public class DownloaderFromMongo
{
	private IDownloader _downloader;
	private readonly IMongoCollection<Link> _linksCollection;
	private readonly ILogger<DownloaderFromMongo> _logger;

	public DownloaderFromMongo(IDownloader downloader, IMongoCollection<Link> linksCollection, ILogger<DownloaderFromMongo> logger)
	{
		_downloader = downloader;
		_linksCollection = linksCollection;
		_logger = logger;
	}

	public async Task Download(CancellationToken cancellationToken)
	{
		var category = "";
		var cursor = await _linksCollection.FindAsync(
			link => 
				link.Category.Contains(category) &&
				link.Type == LinkType.Img
			, cancellationToken: cancellationToken);
		_logger.LogInformation("Got links");
		var links = cursor.ToEnumerable().ToList();
		var chunks = links
			.SelectMany(link => 
				link.Urls
					.Select((url, i) => 
						_downloader.Download(url, category, link._id.ToString()+i)))
			.Chunk(1);
		foreach (var chunk in chunks)
		{
			_logger.LogInformation("Start downloading chunk with size {Count}", chunk.Length);
			await Task.WhenAll(chunk);
		}
		_logger.LogInformation("Comleted");
	}
}