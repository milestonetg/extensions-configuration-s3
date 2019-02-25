using Amazon.S3.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MilestoneTG.Extensions.Configuration.S3
{
    /// <summary>
    /// Interface implemented by S3ObjectParsers. A Parser is used to parse the contents
    /// of the S3Object into the configuration dictionary.
    /// </summary>
    public interface IS3ObjectParser
    {
        /// <summary>
        /// Parses the given S3 <see cref="GetObjectResponse"/>.
        /// </summary>
        /// <param name="s3Response">An <see cref="Amazon.S3.Model.GetObjectResponse"/></param>
        /// <returns>A <see cref="IDictionary<string, string>"/> to be consumed by the ConfigurationRoot.</returns>
        Task<IDictionary<string, string>> ParseAsync(GetObjectResponse s3Response);
    }
}
