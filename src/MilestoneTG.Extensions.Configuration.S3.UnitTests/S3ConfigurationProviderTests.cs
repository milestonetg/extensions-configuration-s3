using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;

namespace MilestoneTG.Extensions.Configuration.S3.UnitTests
{
    [TestFixture(Author = "Tamás Sinku (sinkutamas@gmail.com)")]
    [TestOf(typeof(S3ConfigurationProvider))]
    public class S3ConfigurationProviderTests
    {
        private const string ExpectedBucketName = "test-bucket";
        private const string ExpectedS3Key = "test-key";

        [Test(Description = "Tests that the constructor throws exception if configuration is null.")]
        [Author("Tamás Sinku (sinkutamas@gmail.com)")]
        public void ConstructorValidatesConfigForNullValueTest()
        {
            //arrange
            IS3ProviderConfiguration config = null;

            //act
            void Invoke() => new S3ConfigurationProvider(config, null, null, null);

            //assert
            ArgumentNullException ex = Assert.Throws<ArgumentNullException>(Invoke);
            Assert.That(ex.ParamName, Is.EqualTo("config"));
        }

        [Test(Description = "Tests that the constructor checks if a valid S3 bucket name is provided in the configuration.")]
        [Author("Tamás Sinku (sinkutamas@gmail.com)")]
        public void ConstructorValidatesBucketNameProperlyTest()
        {
            //arrange
            Mock<IS3ProviderConfiguration> configMock = new Mock<IS3ProviderConfiguration>(MockBehavior.Strict);
            configMock
                .SetupGet(x => x.BucketName)
                .Returns((string)null)
                .Verifiable();

            //act
            void Invoke() => new S3ConfigurationProvider(configMock.Object, null, null, null);

            //assert
            ArgumentException ex = Assert.Throws<ArgumentException>(Invoke);
            Assert.That(ex.ParamName, Is.EqualTo("config"));
            Assert.That(ex.Message.Contains("BucketName cannot be null."), Is.True);
            configMock.Verify();
        }

        [Test(Description = "Tests that the constructor checks if a valid S3 key is provided in the configuration.")]
        [Author("Tamás Sinku (sinkutamas@gmail.com)")]
        public void ConstructorValidatesKeyProperlyTest()
        {
            //arrange
            Mock<IS3ProviderConfiguration> configMock = new Mock<IS3ProviderConfiguration>(MockBehavior.Strict);
            configMock
                .SetupGet(x => x.BucketName)
                .Returns(ExpectedBucketName)
                .Verifiable();

            configMock
                .SetupGet(x => x.Key)
                .Returns((string)null)
                .Verifiable();

            //act
            void Invoke() => new S3ConfigurationProvider(configMock.Object, null, null, null);

            //assert
            ArgumentException ex = Assert.Throws<ArgumentException>(Invoke);
            Assert.That(ex.ParamName, Is.EqualTo("config"));
            Assert.That(ex.Message.Contains("Key cannot be null."), Is.True);
            configMock.Verify();
        }

        [Test(Description = "Tests that the LoadAsync method can load configuration properly from S3 bucket.")]
        [Author("Tamás Sinku (sinkutamas@gmail.com)")]
        public void ConfigurationInitialLoadingTest()
        {
            //arrange
            IDictionary<string, string> configFromS3 = new Dictionary<string, string>()
            {
                ["key1"] = "value1",
                ["key2"] = "value2",
            };

            Mock<IS3ProviderConfiguration> configMock = new Mock<IS3ProviderConfiguration>(MockBehavior.Strict);
            configMock
                .SetupGet(x => x.BucketName)
                .Returns(ExpectedBucketName)
                .Verifiable();

            configMock
                .SetupGet(x => x.Key)
                .Returns(ExpectedS3Key)
                .Verifiable();

            Mock<IAmazonS3> s3Mock = new Mock<IAmazonS3>(MockBehavior.Strict);
            s3Mock
                .Setup(x => x.GetObjectAsync(
                    It.Is<GetObjectRequest>(input => input.BucketName == ExpectedBucketName && input.Key == ExpectedS3Key && input.EtagToNotMatch == null),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(
                    new GetObjectResponse()
                    {
                        BucketName = ExpectedBucketName,
                        Key = ExpectedS3Key
                    }
                )
                .Verifiable();

            Mock<IS3ObjectParser> parserMock = new Mock<IS3ObjectParser>(MockBehavior.Strict);
            parserMock
                .Setup(x => x.ParseAsync(
                    It.Is<GetObjectResponse>(
                        input => input.BucketName == ExpectedBucketName && input.Key == ExpectedS3Key
                    )
                ))
                .ReturnsAsync(configFromS3)
                .Verifiable();

            //act
            S3ConfigurationProvider provider = new S3ConfigurationProvider(configMock.Object, s3Mock.Object, parserMock.Object, null);

            bool reloadTriggered = false;
            using (ChangeToken.OnChange(provider.GetReloadToken, () => reloadTriggered = true))
            {
                provider.Load();
            }

            //assert
            Assert.That(reloadTriggered, Is.True);
            configMock.Verify();
            s3Mock.Verify();
            parserMock.Verify();
        }

        [Test(Description = "Tests that the provider can reload configuration properly and notifies the configuration system about the change.")]
        [Author("Tamás Sinku (sinkutamas@gmail.com)")]
        public void ConfigurationReloadingTest()
        {
            //arrange
            string initialEtag = "1";
            IDictionary<string, string> initialConfiguration = new Dictionary<string, string>()
            {
                ["key1"] = "value1",
                ["key2"] = "value2",
            };

            string newEtag = "2";
            IDictionary<string, string> newConfiguration = new Dictionary<string, string>()
            {
                ["key1"] = "new-value1",
                ["key2"] = "new-value2",
            };

            Mock<IS3ProviderConfiguration> configMock = new Mock<IS3ProviderConfiguration>(MockBehavior.Strict);
            configMock
                .SetupGet(x => x.BucketName)
                .Returns(ExpectedBucketName)
                .Verifiable();

            configMock
                .SetupGet(x => x.Key)
                .Returns(ExpectedS3Key)
                .Verifiable();
            
            Mock<IAmazonS3> s3Mock = new Mock<IAmazonS3>(MockBehavior.Strict);
            s3Mock
                .SetupSequence(x => x.GetObjectAsync(
                    It.Is<GetObjectRequest>(input => input.BucketName == ExpectedBucketName && input.Key == ExpectedS3Key && input.EtagToNotMatch == null || input.EtagToNotMatch == initialEtag),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(
                    new GetObjectResponse()
                    {
                        BucketName = ExpectedBucketName,
                        Key = ExpectedS3Key,
                        ETag = initialEtag
                    }
                )
                .ReturnsAsync(
                    new GetObjectResponse()
                    {
                        BucketName = ExpectedBucketName,
                        Key = ExpectedS3Key,
                        ETag = newEtag
                    }
                );
            
            Mock<IS3ObjectParser> parserMock = new Mock<IS3ObjectParser>(MockBehavior.Strict);
            parserMock
                .SetupSequence(x => x.ParseAsync(
                    It.Is<GetObjectResponse>(
                        input => input.BucketName == ExpectedBucketName && input.Key == ExpectedS3Key
                    )
                ))
                .ReturnsAsync(initialConfiguration)
                .ReturnsAsync(newConfiguration);
            
            Mock<IReloadTrigger> triggerMock = new Mock<IReloadTrigger>();
            triggerMock
                .SetupAdd(x => x.Triggered += It.IsAny<EventHandler>())
                .Verifiable();

            //act
            S3ConfigurationProvider provider = new S3ConfigurationProvider(configMock.Object, s3Mock.Object, parserMock.Object, triggerMock.Object);

            int loadTimes = 0;
            using (ChangeToken.OnChange(provider.GetReloadToken, () => loadTimes++))
            {
                provider.Load();
                triggerMock.Raise(x => x.Triggered += null, EventArgs.Empty);
            }
            
            //assert
            foreach (var configItem in newConfiguration)
            {
                if (!provider.TryGet(configItem.Key, out string value))
                    Assert.Fail($"Configuration provider was not able to retrieve the expected configuration value for key '{configItem.Key}'.");

                Assert.That(value, Is.EqualTo(configItem.Value));
            }

            Assert.That(loadTimes, Is.EqualTo(2));
            configMock.Verify();
            s3Mock.Verify();
            parserMock.Verify();
            triggerMock.Verify();
        }

        [Test(Description = "Tests that the provider does not notify the configuration system during reload when the configuration has not changed.")]
        [Author("Tamás Sinku (sinkutamas@gmail.com)")]
        public void ConfigurationReloadingTestWithNonChangedConfig()
        {
            //arrange
            string initialEtag = "1";
            IDictionary<string, string> initialConfiguration = new Dictionary<string, string>()
            {
                ["key1"] = "value1",
                ["key2"] = "value2",
            };

            Mock<IS3ProviderConfiguration> configMock = new Mock<IS3ProviderConfiguration>(MockBehavior.Strict);
            configMock
                .SetupGet(x => x.BucketName)
                .Returns(ExpectedBucketName)
                .Verifiable();

            configMock
                .SetupGet(x => x.Key)
                .Returns(ExpectedS3Key)
                .Verifiable();

            configMock
                .SetupGet(x => x.Optional)
                .Returns(false)
                .Verifiable();
            
            Mock<IAmazonS3> s3Mock = new Mock<IAmazonS3>(MockBehavior.Strict);
            s3Mock
                .SetupSequence(x => x.GetObjectAsync(
                    It.Is<GetObjectRequest>(input => input.BucketName == ExpectedBucketName && input.Key == ExpectedS3Key && input.EtagToNotMatch == null || input.EtagToNotMatch == initialEtag),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(
                    new GetObjectResponse()
                    {
                        BucketName = ExpectedBucketName,
                        Key = ExpectedS3Key,
                        ETag = initialEtag
                    }
                )
                .ThrowsAsync(
                    new AmazonS3Exception("")
                    {
                        ErrorCode = "NotModified",
                        StatusCode = System.Net.HttpStatusCode.NotModified
                    }
                );

            Mock<IS3ObjectParser> parserMock = new Mock<IS3ObjectParser>(MockBehavior.Strict);
            parserMock
                .Setup(x => x.ParseAsync(
                    It.Is<GetObjectResponse>(
                        input => input.BucketName == ExpectedBucketName && input.Key == ExpectedS3Key && input.ETag == initialEtag
                    )
                ))
                .ReturnsAsync(initialConfiguration)
                .Verifiable();

            Mock<IReloadTrigger> triggerMock = new Mock<IReloadTrigger>();
            triggerMock
                .SetupAdd(x => x.Triggered += It.IsAny<EventHandler>())
                .Verifiable();

            //act
            S3ConfigurationProvider provider = new S3ConfigurationProvider(configMock.Object, s3Mock.Object, parserMock.Object, triggerMock.Object);

            int loadTimes = 0;

            void Invoke()
            {
                using (ChangeToken.OnChange(provider.GetReloadToken, () => loadTimes++))
                {
                    provider.Load();
                    triggerMock.Raise(x => x.Triggered += null, EventArgs.Empty);
                }
            }

            //assert
            Assert.DoesNotThrow(Invoke);
            Assert.That(loadTimes, Is.EqualTo(1));
            configMock.Verify();
            s3Mock.Verify();
            parserMock.Verify();
            triggerMock.Verify();
        }

        [Description("Tests the provider's behavior on initial load for a non-existing key in the given S3 bucket depending on whether the configuration source is optional or mandatory.")]
        [Author("Tamás Sinku (sinkutamas@gmail.com)")]
        [TestCase(false, Description = "The configuration source is mandatory and exception should be thrown.")]
        [TestCase(true, Description = "The configuration source is optional and exception should not be thrown.")]
        public void InitialLoadBehaviorWithNonExistingKeyTest(bool optional)
        {
            //arrange
            const string nonExistingKey = "not-existing-key.json";

            Mock<IS3ProviderConfiguration> configMock = new Mock<IS3ProviderConfiguration>(MockBehavior.Strict);
            configMock
                .SetupGet(x => x.BucketName)
                .Returns(ExpectedBucketName)
                .Verifiable();

            configMock
                .SetupGet(x => x.Key)
                .Returns(nonExistingKey)
                .Verifiable();

            configMock
                .SetupGet(x => x.Optional)
                .Returns(optional)
                .Verifiable();

            Mock<IAmazonS3> s3Mock = new Mock<IAmazonS3>(MockBehavior.Strict);
            s3Mock
                .SetupSequence(x => x.GetObjectAsync(
                    It.Is<GetObjectRequest>(input => input.BucketName == ExpectedBucketName && input.Key == nonExistingKey && input.EtagToNotMatch == null),
                    It.IsAny<CancellationToken>()
                ))
                .ThrowsAsync(
                    new AmazonS3Exception("")
                    {
                        ErrorCode = "NoSuchKey",
                        StatusCode = System.Net.HttpStatusCode.NotFound
                    }
                );
            
            //act
            S3ConfigurationProvider provider = new S3ConfigurationProvider(configMock.Object, s3Mock.Object, null, null);
            void Invoke() => provider.Load();

            //assert
            if (optional)
                Assert.DoesNotThrow(Invoke);
            else
                Assert.Throws<AmazonS3Exception>(Invoke);

            configMock.Verify();
            s3Mock.Verify();
        }
    }
}
