# .NET Configuration Extensions for AWS S3

[![Build status](https://milestonetg.visualstudio.com/Milestone/_apis/build/status/extensions-configuration-s3)](https://milestonetg.visualstudio.com/Milestone/_build/latest?definitionId=34) 

.NET Configuration Extensions for loading configuration files from AWS S3.

Inspired by:

[Amazon.Extensions.Configuration.SystemsManager](https://github.com/aws/aws-dotnet-extensions-configuration) by Amazon Web Services

[Microsoft.Extensions.Configuration.Json](https://github.com/aspnet/Extensions/tree/v2.2.2/src/Configuration/Config.Json) by Microsoft

[NetEscapades.Configuration.Yaml](https://github.com/andrewlock/NetEscapades.Configuration) by Andrew Lock

## Why S3?
An industry best practice is to separate the management of config from source code. If you're deploying applications to AWS, S3 offers the ability to deploy configuration from your continuous deployment pipeline to a medium that is:

* Secure
* Audited
* Versioned
* Resilent and geographically redundent
* Meets compliancy regulations

## Getting Started

Add the package for your desired file format to your project:

For JSON files:

```
dotnet add package MilestoneTG.Extensions.Configuration.S3.Json
```

For YAML files:

```
dotnet add package MilestoneTG.Extensions.Configuration.S3.Yaml
```

Then add the configuration to your Configuration builder, specifying the S3 bucket and Object key:

```cs
public class Program
{
    public static void Main(string[] args)
    {
        CreateWebHostBuilder(args).Build().Run();
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(builder => {
                builder.AddJsonS3Object("my-config-bucket", "mySettings.json");
            })
            .UseStartup<Startup>();
}
```

You can also take advantage of the ASP.Net Core environment just as you would your local json files:

```cs
public class Program
{
    public static void Main(string[] args)
    {
        CreateWebHostBuilder(args).Build().Run();
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(builder => {
                var env = context.HostingEnvironment;
                builder.AddJsonS3Object("my-config-bucket", "mySettings.{env.EnvironmentName}.json");
            })
            .UseStartup<Startup>();
}
```

## An Important note about YAML

One thing to be aware of is that the YAML specification is case **sensitive**, so the following file is valid and has 3 distinct keys:

```yaml
test: Value1
Test: Value2
TEST: Value3
```

**However**, the `Microsoft.Extensions.Configuration` library is case **insensitive**. Attempting to load the provided file would throw an exception on attempting to load, complaining of a duplicate key.

## Reloading in AWS Lambda (a note from Amazon)

The `reloadAfter` parameter on `AddXXXXS3Object()` enables automatic reloading of configuration data from S3 as a background task.

In AWS Lambda, background tasks are paused after processing a Lambda event.  This could disrupt the provider from 
retrieving the latest configuration data from S3. To ensure the reload is performed within a Lambda event,
we recommend calling the extension method `WaitForS3ReloadToComplete` from the `IConfiguration` object in 
your Lambda function. This method will immediately return unless a reload is currently being performed.  The `WaitForS3ReloadToComplete` extension method to `IConfiguration` is available when you add the a
`using MilestoneTG.Extensions.Configuration.S3` statement.  See the example below:


```cs
using MilestoneTG.Extensions.Configuration.S3

...

var configurationBuilder = new ConfigurationBuilder();
configurationBuilder.AddJsonS3Object("my-config-bucket", "mySettings.{env.EnvironmentName}.json");
var configurations = configurationBuilder.Build();

...

configurations.WaitForS3ReloadToComplete(TimeSpan.FromSeconds(5));
```