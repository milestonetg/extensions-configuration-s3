using Microsoft.Extensions.Primitives;
using System;
using System.Threading;

namespace MilestoneTG.Extensions.Configuration.S3
{
    internal class ReloadTrigger : IReloadTrigger
    {
        private readonly ManualResetEvent reloadTaskEvent = new ManualResetEvent(true);

        public event EventHandler Triggered;

        public ReloadTrigger(TimeSpan reloadInterval)
        {
            SetupReloader(reloadInterval);
        }

        private void SetupReloader(TimeSpan reloadAfter)
        {
            ChangeToken.OnChange(() =>
            {
                var cancellationTokenSource = new CancellationTokenSource(reloadAfter);
                var cancellationChangeToken = new CancellationChangeToken(cancellationTokenSource.Token);
                return cancellationChangeToken;
            }, () =>
            {
                reloadTaskEvent.Reset();
                try
                {
                    Triggered?.Invoke(this, EventArgs.Empty);
                }
                finally
                {
                    reloadTaskEvent.Set();
                }
            });
        }

        /// <summary>
        /// If this configuration provider is currently performing a reload of the config data this method will block until
        /// the reload is called.
        /// 
        /// This method is not meant for general use. It is exposed so a Lambda function can wait for the reload to complete
        /// before completing the event causing the Lambda compute environment to be frozen.
        /// </summary>
        public void BlockThreadUntilTriggered(TimeSpan timeout)
        {
            reloadTaskEvent.WaitOne(timeout);
        }
    }
}
