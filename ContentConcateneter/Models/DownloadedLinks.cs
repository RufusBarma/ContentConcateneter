using MongoDB.Bson;

namespace Client.Telegram.Models;

public record DownloadedLinks
{
	public ObjectId _id { get; init; }
	public ObjectId PostedId { get; init; }
}
