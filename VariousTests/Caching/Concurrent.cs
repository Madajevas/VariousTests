using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace VariousTests.Caching
{
    public class Concurrent
    {
        private IProbe probe;
        private IFileStore store;

        [SetUp]
        public void SetUp()
        {
            var memCacheOptions = new MemoryCacheOptions
            {
                SizeLimit = 512,
                ExpirationScanFrequency = TimeSpan.FromMilliseconds(10),
            };
            var options = Substitute.For<IOptions<MemoryCacheOptions>>();
            options.Value.Returns(memCacheOptions);

            probe = Substitute.For<IProbe>();
            store = new CachingFileStore(new S3FileStore(probe), options);
            // store = new CachingFileStoreNotCongestionImmune(new S3FileStore(probe), options);
        }

        [Test]
        public async Task GetFile_CalledInParallelForSameFile_GetsFileOnlyOnce()
        {
            await Task.WhenAll(store.GetFile("1"), store.GetFile("1"), store.GetFile("1"), store.GetFile("1"), store.GetFile("1"));

            probe.Received(1).Hit(Arg.Any<string>());
        }

        [Test]
        public async Task GetFile_WhenParallelRequestsForTwoFilesArrives_GetsEachFileOnce()
        {
            await Task.WhenAll(store.GetFile("1"), store.GetFile("2"), store.GetFile("1"), store.GetFile("2"));

            probe.Received(1).Hit("1");
            probe.Received(1).Hit("2");
        }

        [Test]
        public async Task GetFile_WhenCacheSizeLimitIsExceeded_RedownloadsSameFile()
        {
            await Task.WhenAll(store.GetFile("1"), store.GetFile("2"), store.GetFile("3"));
            await Task.Delay(50); // give it time for cache to clean it self
            await store.GetFile("1");

            probe.Received(2).Hit("1");
        }

        [Test]
        public async Task GetFile_EvenUnderLoad_ReturnsResults()
        {
            var files = await Task.WhenAll(store.GetFile("1"), store.GetFile("1"));

            Assert.That(files[0], Is.SameAs(files[1]));
        }


        public interface IProbe
        {
            void Hit(string key);
        }

        interface IFileStore
        {
            Task<byte[]> GetFile(string key);
        }

        sealed class S3FileStore : IFileStore
        {
            private readonly IProbe probe;

            public S3FileStore(IProbe probe)
            {
                this.probe = probe;
            }

            public async Task<byte[]> GetFile(string key)
            {
                probe.Hit(key);
                await Task.Delay(Random.Shared.Next(20, 50));
                return new byte[256];
            }
        }

        sealed class CachingFileStoreNotCongestionImmune : IFileStore
        {
            private readonly IFileStore decoratee;
            private MemoryCache memoryCache;

            public CachingFileStoreNotCongestionImmune(IFileStore decoratee, IOptions<MemoryCacheOptions> memoryCacheOptions)
            {
                this.decoratee = decoratee;

                memoryCache = new MemoryCache(memoryCacheOptions.Value);
            }

            public Task<byte[]> GetFile(string key)
            {
                return memoryCache.GetOrCreateAsync(key, factory: async entry =>
                {
                    var file = await decoratee.GetFile(key);
                    entry.SetSize(file.Length);
                    entry.SetSlidingExpiration(TimeSpan.FromMinutes(5));

                    return file;
                })!;
            }
        }

        sealed class CachingFileStore : IFileStore
        {
            private readonly IFileStore decoratee;
            private readonly MemoryCache memoryCache;
            private readonly ConcurrentDictionary<string, TaskCompletionSource<byte[]>> concurrentDictionary;

            public CachingFileStore(IFileStore decoratee, IOptions<MemoryCacheOptions> memoryCacheOptions)
            {
                this.decoratee = decoratee;
                memoryCache = new MemoryCache(memoryCacheOptions.Value);
                concurrentDictionary = new ConcurrentDictionary<string, TaskCompletionSource<byte[]>>();
            }

            private async Task DownloadFile(string key, TaskCompletionSource<byte[]> completeTo)
            {
                try
                {
                    var file = await decoratee.GetFile(key);
                    var entryOptions = new MemoryCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromSeconds(2),
                        Size = file.Length,
                    };

                    memoryCache.Set(key, file, entryOptions);
                    completeTo.SetResult(file);
                }
                catch (Exception ex)
                {
                    completeTo.SetException(ex);
                }
                finally
                {
                    concurrentDictionary.Remove(key, out _);
                }
            }

            public Task<byte[]> GetFile(string key)
            {
                if (memoryCache.TryGetValue<byte[]>(key, out var result))
                {
                    return Task.FromResult(result);
                }

                var task = concurrentDictionary.GetOrAdd(key, k =>
                {
                    var tcs = new TaskCompletionSource<byte[]>();
                    _ = DownloadFile(k, tcs); // fire and forget
                    return tcs;
                });
                return task.Task;
            }
        }
    }

    class MyToken : IChangeToken
    {
        public bool HasChanged => throw new NotImplementedException();

        public bool ActiveChangeCallbacks => throw new NotImplementedException();

        public IDisposable RegisterChangeCallback(Action<object?> callback, object? state)
        {
            throw new NotImplementedException();
        }
    }
}
