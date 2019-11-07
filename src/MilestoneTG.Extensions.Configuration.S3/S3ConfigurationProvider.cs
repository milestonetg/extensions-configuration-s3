using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MilestoneTG.Extensions.Configuration.S3
{
    /// <summary>
    /// Defines a configuration provider for retrieving configuration files from AWS S3.
    /// </summary>
    public class S3ConfigurationProvider : ConfigurationProvider
    {
        private readonly IS3ProviderConfiguration config;
        private readonly IAmazonS3 s3client;
        private readonly IS3ObjectParser parser;
        private readonly IReloadTrigger reloadTrigger;

        private string previousEtag;

        /// <summary>
        /// Initialized a new <see cref="S3ConfigurationProvider"/> using the given <see cref="S3ConfigurationSource"/>.
        /// </summary>
        /// <param name="config">An implementation of <see cref="IS3ProviderConfiguration"/> that provides configuration.</param>
        /// <param name="s3client">An Amazon S3 client.</param>
        /// <param name="parser">The parser that will convert the S3 object to <see cref="Dictionary{TKey, TValue}"/> (where TKey and TValue are strings)</param>
        /// <param name="reloadTrigger">A trigger that will initiate the automatic reloading process.</param>
        public S3ConfigurationProvider(IS3ProviderConfiguration config, IAmazonS3 s3client, IS3ObjectParser parser, IReloadTrigger reloadTrigger)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));

            if (config.BucketName == null)
                throw new ArgumentException($"{nameof(S3ConfigurationSource.BucketName)} cannot be null.", nameof(config));

            if (config.Key == null)
                throw new ArgumentException($"{nameof(S3ConfigurationSource.Key)} cannot be null.", nameof(config));
            
            this.s3client = s3client;
            this.parser = parser;
            this.reloadTrigger = reloadTrigger;

            if (this.reloadTrigger != null)
                this.reloadTrigger.Triggered += OnReloadTriggered;
        }

        /// <summary>
        /// Initialized a new <see cref="S3ConfigurationProvider"/> using the given <see cref="S3ConfigurationSource"/>. Using this overload will not perform automatic reloading.
        /// </summary>
        /// <param name="source">An implementation of <see cref="IS3ProviderConfiguration"/> that provides configuration.</param>
        /// <param name="s3client">A factory that can be used to create Amazon S3 clients.</param>
        /// <param name="parser">The parser that will convert the S3 object to <see cref="Dictionary{TKey, TValue}"/> (where TKey and TValue are strings)</param>
        public S3ConfigurationProvider(IS3ProviderConfiguration source, IAmazonS3 s3client, IS3ObjectParser parser)
            : this(source, s3client, parser, null)
        {

        }

        private void OnReloadTriggered(object sender, EventArgs e)
        {
            Load(true);
        }
        
        /// <summary>
        /// If this configuration provider is currently performing a reload of the config data this method will block until
        /// the reload is called.
        /// 
        /// This method is not meant for general use. It is exposed so a Lambda function can wait for the reload to complete
        /// before completing the event causing the Lambda compute environment to be frozen.
        /// </summary>
        public void WaitForReloadToComplete(TimeSpan timeout)
        {
            this.reloadTrigger.BlockThreadUntilTriggered(timeout);
        }

        /// <summary>
        /// Loads (or reloads) the data for this provider.
        /// </summary>
        public override void Load() => Load(false);
        
        private void Load(bool reload) => LoadAsync(reload).ConfigureAwait(false).GetAwaiter().GetResult();
        
        private async Task LoadAsync(bool reload)
        {
            try
            {
                GetObjectRequest request = new GetObjectRequest()
                {
                    BucketName = config.BucketName,
                    Key = config.Key,
                    EtagToNotMatch = previousEtag
                };

                using (GetObjectResponse s3Response = await s3client.GetObjectAsync(request).ConfigureAwait(false))
                {
                    Data = await parser.ParseAsync(s3Response).ConfigureAwait(false);
                    this.previousEtag = s3Response.ETag;
                }
                
                OnReload();
            }
            catch (Exception)
            {
                if (config.Optional || reload)
                    return;

                throw;
            }
        }
    }
}
