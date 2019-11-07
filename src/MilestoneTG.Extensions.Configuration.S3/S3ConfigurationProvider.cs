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
                GetObjectRequest request = new GetObjectRequest()
                { 
                    BucketName = source.BucketName,
                    Key = source.Key,
                    EtagToNotMatch = previousEtag
                };


                using (GetObjectResponse s3Response = await s3.GetObjectAsync(source.BucketName, source.Key).ConfigureAwait(false))
                {
                    if (s3Response.ContentLength > 0)
                    {
                        Data = await source.Parser.ParseAsync(s3Response).ConfigureAwait(false);
                        previousEtag = s3Response.ETag;
                        OnReload();
                    }
                }

            }
            catch(Exception)
            {
                if (source.Optional || reload)
                    return;

                throw;
            }
        }
    }
}
