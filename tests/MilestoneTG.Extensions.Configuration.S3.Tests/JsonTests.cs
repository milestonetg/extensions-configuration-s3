using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;

using Microsoft.Extensions.Configuration;

using MilestoneTG.Extensions.Configuration.S3.Json;

using Shouldly;

using Xunit;

namespace MilestoneTG.Extensions.Configuration.S3.Tests;

[Collection(LocalStackCollection.CollectionName)]
public class JsonTests
{
    private readonly LocalStackFixture _fixture;
    
    public JsonTests(LocalStackFixture fixture)
    {
        _fixture = fixture;
    }
    
    [Fact]
    public async Task TestConfiguration()
    {
        var aws = new AWSOptions
        {
            DefaultClientConfig =
            {
                ServiceURL = _fixture.S3Uri,
            },
            Credentials = new BasicAWSCredentials("ignore", "ignore")
        };

        var s3 = aws.CreateServiceClient<IAmazonS3>();
        
        await s3.PutBucketAsync("test-bucket", TestContext.Current.CancellationToken);
        await s3.PutObjectAsync(new Amazon.S3.Model.PutObjectRequest
        {
            BucketName = "test-bucket",
            Key = "test-key.json",
            ContentBody = "{\"TestKey\": \"TestValue\"}"
        }, TestContext.Current.CancellationToken);
        
        var config = new ConfigurationBuilder()
            .AddJsonS3Object(aws, "test-bucket", "test-key.json")
            .Build();

        config["TestKey"].ShouldBe("TestValue");
    }
}