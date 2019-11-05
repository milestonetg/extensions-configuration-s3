using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;
using Microsoft.Extensions.Configuration;
using System;

namespace MilestoneTG.Extensions.Configuration.S3
{
    /// <summary>
    /// Defines a configuration source for retrieving configuration files from AWS S3.
    /// </summary>
    public class S3ConfigurationSource : IConfigurationSource, IS3ProviderConfiguration
    {
        /// <summary>
        /// <see cref="AWSOptions"/> used to create an AWS S3 Client />.
        /// </summary>
        public AWSOptions AwsOptions { get; set; }

        /// <summary>
        /// Name of the S3 bucket containing the configuration file.
        /// </summary>
        public string BucketName { get; set; }

        /// <summary>
        /// Name of the S3 object in the <see cref="BucketName"/> that represents the configuration file.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Determines if loading configuration data from the AWS S3 bucket and Object Key is optional.
        /// </summary>
        public bool Optional { get; set; }

        /// <summary>
        /// Parameters will be reloaded from the AWS S3 bucket and Object Key after the specified time frame
        /// </summary>
        public TimeSpan? ReloadAfter { get; set; }

        /// <summary>
        /// The parser to use to parse the stream once the S3 object is retrieved.
        /// </summary>
        public IS3ObjectParser Parser { get; set; }

        /// <summary>
        /// Builds the Microsoft.Extensions.Configuration.IConfigurationProvider for this source.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
        /// <returns>An <see cref="IConfigurationProvider"/>.</returns>
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            SelfValidation();
            return CreateProvider();
        }

        private void SelfValidation()
        {
            if (AwsOptions == null)
                throw new InvalidOperationException($"{nameof(AwsOptions)} must be set in order to create {nameof(S3ConfigurationProvider)}.");

            if (Parser == null)
                throw new InvalidOperationException($"{nameof(Parser)} must be set in order to create {nameof(S3ConfigurationProvider)}.");
        }

        private IConfigurationProvider CreateProvider()
        {
            ConfigurationProvider result = null;

            IAmazonS3 s3client = AwsOptions.CreateServiceClient<IAmazonS3>();

            if (ReloadAfter.HasValue)
                result = new S3ConfigurationProvider(this, s3client, Parser, new ReloadTrigger(ReloadAfter.Value));
            else
                result = new S3ConfigurationProvider(this, s3client, Parser);

            return result;
        }
    }
}
