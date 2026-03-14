using Testcontainers.LocalStack;

namespace MilestoneTG.Extensions.Configuration.S3.Tests;

public class LocalStackFixture : IAsyncLifetime, IDisposable
{
    private LocalStackContainer _container;
    
    public LocalStackFixture()
    {
        _container = new LocalStackBuilder("localstack/localstack:4.14.0").Build();
    }

    public string Uri => _container.GetConnectionString();
    public string S3Uri { get; private set; } = string.Empty;
    
    public void Dispose()
    { }

    public async ValueTask DisposeAsync()
    {
        await _container.StopAsync();
        await _container.DisposeAsync();
    }

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();

        var baseUri = _container.GetConnectionString();
        S3Uri = baseUri.Replace("127.0.0.1", "s3.localhost");
    }
}

[CollectionDefinition(CollectionName)]
public class LocalStackCollection : ICollectionFixture<LocalStackFixture>
{
    public const string CollectionName = "LocalStack";   
}