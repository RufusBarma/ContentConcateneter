using Client.Telegram.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

var configurationRoot = new ConfigurationBuilder()
	.AddEnvironmentVariables()
	.AddJsonFile("appsettings.json")
	.Build();

var ffmpegPath = "./FFmpeg";
FFmpeg.SetExecutablesPath(ffmpegPath);
await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, ffmpegPath);

var serviceProvider = new ServiceCollection()
	.AddSingleton<IConfiguration>(configurationRoot)
	.AddLogging(configure => configure.AddConsole())
	.AddSingleton<IMongoDatabase>(_ =>
	{
		var mongoUrl = new MongoUrl(configurationRoot.GetConnectionString("DefaultConnection"));
		return new MongoClient(mongoUrl).GetDatabase(mongoUrl.DatabaseName);
	})
	.AddTransient<IMongoCollection<Link>>(provider => provider.GetService<IMongoDatabase>().GetCollection<Link>("Links"))
	.BuildServiceProvider();

var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
var cancellationTokenSource = new CancellationTokenSource();
var startupTask = Task.Delay(1000);

AppDomain.CurrentDomain.ProcessExit += async (_, _) =>
{
	logger.LogInformation("Received SIGTERM");
	cancellationTokenSource.Cancel();
	await startupTask;
	logger.LogInformation("Safety shutdown");
	await serviceProvider.DisposeAsync();
};

await Task.Delay(Timeout.Infinite, cancellationTokenSource.Token);