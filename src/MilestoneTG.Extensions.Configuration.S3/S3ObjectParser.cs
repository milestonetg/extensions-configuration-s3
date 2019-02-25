using Amazon.S3.Model;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MilestoneTG.Extensions.Configuration.S3
{
    /// <summary>
    /// Base class for S3ObjectParsers.
    /// </summary>
    public abstract class S3ObjectParser : IS3ObjectParser
    {
        /// <summary>
        /// Parses the given S3 <see cref="GetObjectResponse"/>.
        /// </summary>
        /// <param name="s3Response">An <see cref="Amazon.S3.Model.GetObjectResponse"/></param>
        /// <returns>A IDictionary&lt;string, string&gt; to be consumed by the ConfigurationRoot.</returns>
        public async Task<IDictionary<string, string>> ParseAsync(GetObjectResponse s3Response)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                await s3Response.ResponseStream.CopyToAsync(ms).ConfigureAwait(false);
                ms.Position = 0;
                return ParseAsync(ms);
            }
        }

        /// <summary>
        /// Parses the given S3 Object's response <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">Copy of the Amazon.S3.Model.GetObjectResponse.ResponseStream"</param>
        /// <returns>A IDictionary&lt;string, string&gt; to be consumed by the ConfigurationRoot.</returns>
        public abstract IDictionary<string, string> ParseAsync(Stream stream);
    }
}
