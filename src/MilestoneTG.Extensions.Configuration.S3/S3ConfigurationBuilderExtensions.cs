using Amazon.Extensions.NETCore.Setup;
using Microsoft.Extensions.Configuration;
using System;

namespace MilestoneTG.Extensions.Configuration.S3
{
    /// <summary>
    /// Extension methods of <see cref="IConfigurationBuilder"/> for adding AWS S3 objects as configuration files.
    /// </summary>
    public static class S3ConfigurationBuilderExtensions
    {
        private const string AwsOptionsConfigurationKey = "AWS_CONFIGBUILDER_AWSOPTIONS";

        /// <summary>
        /// Adds the file with the given key in the given S3 bucket to the configuration builder.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="bucketName">The AWS S3 bucket containing the file.</param>
        /// <param name="key">The S3 object name representing the file.</param>
        /// <param name="optional">Whether the file is optional.</param>
        /// <param name="reloadAfter">The interval at which to automatically reload the configuration from S3.</param>
        /// <param name="parser">The <see cref="IS3ObjectParser"/> to use to parse the S3 Object.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddS3Object(this IConfigurationBuilder builder, string bucketName, string key, bool optional, TimeSpan? reloadAfter, IS3ObjectParser parser)
        {
            return builder.AddS3Object(builder.GetAwsOptions(), bucketName, key, optional, reloadAfter, parser);
        }

        /// <summary>
        /// Adds the file with the given key in the given S3 bucket to the configuration builder.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="awsOptions">An instance of AWSOptions to use to use to connect to S3.</param>
        /// <param name="bucketName">The AWS S3 bucket containing the file.</param>
        /// <param name="key">The S3 object name representing the file.</param>
        /// <param name="optional">Whether the file is optional.</param>
        /// <param name="reloadAfter">The interval at which to automatically reload the configuration from S3.</param>
        /// <param name="parser">The <see cref="IS3ObjectParser"/> to use to parse the S3 Object.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddS3Object(this IConfigurationBuilder builder, AWSOptions awsOptions, string bucketName, string key, bool optional, TimeSpan? reloadAfter, IS3ObjectParser parser)
        {
            return builder.AddS3Object(source =>
            {
                source.AwsOptions = awsOptions;
                source.BucketName = bucketName;
                source.Key = key;
                source.Optional = optional;
                source.Parser = parser;
                source.ReloadAfter = reloadAfter;
            });
        }

        /// <summary>
        /// Adds the file with the given key in the given S3 bucket to the configuration builder.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="source">An Action returning the <see cref="S3ConfigurationSource"/> to use.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddS3Object(this IConfigurationBuilder builder, Action<S3ConfigurationSource> source)
        {
            return builder.Add(source);
        }

        /// <summary>
        /// Gets the AWS Options from local configuration. In a production environment, this would pull from
        /// the IAM role of the running instance.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        private static AWSOptions GetAwsOptions(this IConfigurationBuilder builder)
        {
            if (builder.Properties.TryGetValue(AwsOptionsConfigurationKey, out var value) && value is AWSOptions existingOptions)
            {
                return existingOptions;
            }

            var config = builder.Build();
            var newOptions = config.GetAWSOptions();

            if (builder.Properties.ContainsKey(AwsOptionsConfigurationKey))
            {
                builder.Properties[AwsOptionsConfigurationKey] = newOptions;
            }
            else
            {
                builder.Properties.Add(AwsOptionsConfigurationKey, newOptions);
            }

            return newOptions;
        }
    }
}
