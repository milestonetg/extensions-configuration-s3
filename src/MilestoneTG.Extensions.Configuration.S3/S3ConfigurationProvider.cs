using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MilestoneTG.Extensions.Configuration.S3
{
    /// <summary>
    /// Defines a configuration provider for retrieving configuration files from AWS S3.
    /// </summary>
    public class S3ConfigurationProvider : ConfigurationProvider
    {
        readonly S3ConfigurationSource source;
        readonly IAmazonS3 s3;

        readonly ManualResetEvent reloadTaskEvent = new ManualResetEvent(true);

        string previousEtag = string.Empty;

        /// <summary>
        /// Initialized a new <see cref="S3ConfigurationProvider"/> using the given <see cref="S3ConfigurationSource"/>.
        /// </summary>
        /// <param name="source">The instance of <see cref="S3ConfigurationSource"/> to use for this provider instance.</param>
        public S3ConfigurationProvider(S3ConfigurationSource source)
        {
            this.source = source ?? throw new ArgumentNullException(nameof(source));
            if (source.AwsOptions == null) throw new ArgumentNullException(nameof(source.AwsOptions));
            if (source.BucketName == null) throw new ArgumentNullException(nameof(source.BucketName));
            if (source.Key == null) throw new ArgumentNullException(nameof(source.Key));

            s3 = source.AwsOptions.CreateServiceClient<IAmazonS3>();

            if (source.ReloadAfter != null)
            {
                ChangeToken.OnChange(() =>
                {
                    var cancellationTokenSource = new CancellationTokenSource(source.ReloadAfter.Value);
                    var cancellationChangeToken = new CancellationChangeToken(cancellationTokenSource.Token);
                    return cancellationChangeToken;
                }, () =>
                {
                    reloadTaskEvent.Reset();
                    try
                    {
                        Load(true);
                    }
                    finally
                    {
                        reloadTaskEvent.Set();
                    }
                });
            }
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
            reloadTaskEvent.WaitOne(timeout);
        }

        /// <summary>
        ///  Loads (or reloads) the data for this provider.
        /// </summary>
        public override void Load() => Load(false);

        private void Load(bool reload) => LoadAsync(reload).ConfigureAwait(false).GetAwaiter().GetResult();

        private async Task LoadAsync(bool reload)
        {
            try
            {
                // Despite this thread: https://forums.aws.amazon.com/thread.jspa?threadID=77995&tstart=0
                // GetObjectAsync(...) still throws and AmazonS3Exception when ETags do not match.
                // Since a config change is the exception to the rule, and throwing exceptions is expensive, 
                // we'll get the metadata and check the etag manually first before getting the entire object.
                // This will increase performance of the config checks, since we won't download the entire object 
                // every time only to disgard it. Only checking on reload ensures that the inital load doesn't take 
                // the double hit.
                if (reload)
                {
                    GetObjectMetadataResponse metadata = await s3.GetObjectMetadataAsync(source.BucketName, source.Key).ConfigureAwait(false);

                    // If the Etag is the same, don't bother downloading, deserializing, and notifying the config system.
                    if (metadata.ETag == previousEtag)
                    {
                        return;
                    }
                }
                
                // We are certain there is new config...
                using (GetObjectResponse s3Response = await s3.GetObjectAsync(source.BucketName, source.Key).ConfigureAwait(false))
                {
                        Data = await source.Parser.ParseAsync(s3Response).ConfigureAwait(false);
                        previousEtag = s3Response.ETag;
                }

                // Notify the configuration system of the change...
                OnReload();
            }
            catch (Exception)
            {
                // if the config is optional or we are on a reload, we don't care...
                if (source.Optional || reload)
                    return;

                throw;
            }
        }
    }
}
