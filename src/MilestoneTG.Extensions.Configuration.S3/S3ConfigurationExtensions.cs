using Microsoft.Extensions.Configuration;
using System;

namespace MilestoneTG.Extensions.Configuration.S3
{
    /// <summary>
    /// This extension is an a different namespace to avoid misuse of this method which should only be called when being used from Lambda.
    /// </summary>
    public static class S3ConfigurationExtensions
    {
        /// <summary>
        /// This method blocks while any S3ConfigurationProvider added to IConfiguration are
        /// currently reloading the parameters from Parameter Store.
        /// 
        /// This is generally only needed when the provider is being called from a Lambda function. Without this call
        /// in a Lambda environment there is a potential of the background thread doing the refresh never running successfully.
        /// This can happen because the Lambda compute environment is frozen after the current Lambda event is complete.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="timeSpan"></param>
        public static void WaitForS3ReloadToComplete(this IConfiguration configuration, TimeSpan timeSpan)
        {
            var configRoot = configuration as ConfigurationRoot;
            if (configRoot == null)
            {
                return;
            }

            foreach (var provider in configRoot.Providers)
            {
                if (provider is S3ConfigurationProvider s3Provider)
                {
                    s3Provider.WaitForReloadToComplete(timeSpan);
                }
            }
        }
    }
}
