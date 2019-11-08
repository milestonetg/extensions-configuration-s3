using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MilestoneTG.Extensions.Configuration.S3.Json;

namespace Example
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder => {
                    builder.AddJsonS3Object("mtg-test-config", "mySettings.json", reloadAfter: TimeSpan.FromSeconds(10));
                })
                .UseStartup<Startup>();
    }
}
