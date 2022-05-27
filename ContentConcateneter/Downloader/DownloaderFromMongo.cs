using Client.Telegram.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ContentConcateneter.Downloader;

public class DownloaderFromMongo
{
	private IDownloader _downloader;
	private readonly IMongoCollection<Link> _linksCollection;
	private readonly IMongoCollection<DownloadedLinks> _downloadedCollection;
	private readonly ILogger<DownloaderFromMongo> _logger;

	public DownloaderFromMongo(IDownloader downloader, IMongoCollection<Link> linksCollection, IMongoCollection<DownloadedLinks> downloadedCollection, ILogger<DownloaderFromMongo> logger)
	{
		_downloader = downloader;
		_linksCollection = linksCollection;
		_downloadedCollection = downloadedCollection;
		_logger = logger;
	}

	private async Task InsertDownloadedId(ObjectId id)
	{
		await _downloadedCollection.InsertOneAsync(new DownloadedLinks{PostedId = id});
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
		var links = cursor.ToEnumerable()
			.Where(link => !_downloadedCollection.Find(downloaded => downloaded.PostedId == link._id).Any())
			.Take(100)
			.ToList();
		_logger.LogInformation("Count links {Count}", links.Count);
		var chunks = links
			.Select(async link =>
			{
				_logger.LogInformation("Start downloading {Id}", link._id);
				try
				{
					await Task.WhenAll(link.Urls
						.Select((url, i) => _downloader.Download(url,
							string.Join('/',
								link.Category
									.Select(category => category
										.Replace(' ', '_')
										.Replace('/', '_'))
									.ToArray()),
							link._id.ToString() + i)));
					await InsertDownloadedId(link._id);
					_logger.LogInformation("Downloaded {Id}", link._id);
				}
				catch (Exception ex)
				{
					_logger.LogTrace(ex, "Exception on download {Id}", link._id);
				}
			})
			.Chunk(1);
		foreach (var chunk in chunks)
			await Task.WhenAll(chunk);

		_logger.LogInformation("Comleted");
	}
}