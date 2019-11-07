using System;

namespace MilestoneTG.Extensions.Configuration.S3
{
    /// <summary>
    /// Represents a trigger that initiates the reloading process.
    /// </summary>
    public interface IReloadTrigger
    {
        /// <summary>
        /// This event is fired when the configuration should be polled from S3.
        /// </summary>
        event EventHandler Triggered;

        /// <summary>
        /// Blocks the current thread until <see cref="Triggered"/> is fired.
        /// </summary>
        /// <param name="timeout"></param>
        void BlockThreadUntilTriggered(TimeSpan timeout);
    }
}