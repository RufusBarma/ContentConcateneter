using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Client.Telegram.Models;
public enum LinkType
{
	None,
	Video,
	Img,
	Gif,
}
[BsonIgnoreExtraElements]
public record Link
{
	public ObjectId _id { get; init; }
	public string[] Urls { get; set; }
	public LinkType Type { get; set; }
	public string[] Category { get; init; }
}