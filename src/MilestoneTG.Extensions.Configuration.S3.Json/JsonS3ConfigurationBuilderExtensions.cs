using Amazon.Extensions.NETCore.Setup;
using Microsoft.Extensions.Configuration;
using System;

namespace MilestoneTG.Extensions.Configuration.S3.Json
{
    /// <summary>
    /// ConfigurationBuilder extensions for adding S3 objects using the JSON parser.
    /// </summary>
    public static class JsonS3ConfigurationBuilderExtensions
    {
        /// <summary>
        /// Adds the JSON file with the given key in the given S3 bucket to the configuration builder.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="bucketName">The AWS S3 bucket containing the file.</param>
        /// <param name="key">The S3 object name representing the file.</param>
        /// <param name="optional">Whether the file is optional.</param>
        /// <param name="reloadAfter">The interval at which to automatically reload the configuration from S3.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddJsonS3Object(this IConfigurationBuilder builder, string bucketName, string key, bool optional = false, TimeSpan? reloadAfter = null)
        {
            return builder.AddS3Object(bucketName, key, optional, reloadAfter, new JsonS3ObjectParser());
        }

        /// <summary>
        /// Adds the JSON file with the given key in the given S3 bucket to the configuration builder.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="awsOptions">An instance of AWSOptions to use to use to connect to S3.</param>
        /// <param name="bucketName">The AWS S3 bucket containing the file.</param>
        /// <param name="key">The S3 object name representing the file.</param>
        /// <param name="optional">Whether the file is optional.</param>
        /// <param name="reloadAfter">The interval at which to automatically reload the configuration from S3.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddJsonS3Object(this IConfigurationBuilder builder, AWSOptions awsOptions, string bucketName, string key, bool optional = false, TimeSpan? reloadAfter = null)
        {
            return builder.AddS3Object(awsOptions, bucketName, key, optional, reloadAfter, new JsonS3ObjectParser());
        }
    }
}
