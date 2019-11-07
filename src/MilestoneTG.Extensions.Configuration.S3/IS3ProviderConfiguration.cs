using System;

namespace MilestoneTG.Extensions.Configuration.S3
{
    /// <summary>
    /// Represents the expected configuration for <see cref="S3ConfigurationProvider"/>.
    /// </summary>
    public interface IS3ProviderConfiguration
    {
        /// <summary>
        /// Gets the name of the S3 bucket containing the configuration file.
        /// </summary>
        string BucketName { get; }

        /// <summary>
        /// Gets the name of the S3 object in the <see cref="BucketName"/> that represents the configuration file.
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Gets the time interval between two reloads. Null is returned when the configuration does not need to be reloaded.
        /// </summary>
        TimeSpan? ReloadAfter { get; }

        /// <summary>
        /// Gets if loading configuration data from the AWS S3 bucket and Object Key is optional.
        /// </summary>
        bool Optional { get; }
    }
}