using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace OutOut.Infrastructure.Services
{
    public class StringLockProvider
    {
        static readonly ConcurrentDictionary<string, SemaphoreSlim> lockDictionary = new ConcurrentDictionary<string, SemaphoreSlim>();

        public async Task WaitAsync(string key)
        {
            await lockDictionary.GetOrAdd(key, new SemaphoreSlim(1, 1)).WaitAsync();
        }

        public void Release(string key)
        {
            if (lockDictionary.TryGetValue(key, out SemaphoreSlim semaphore))
            {
                semaphore.Release();
            }
        }

        public void Delete(string key)
        {
            if (lockDictionary.TryGetValue(key, out SemaphoreSlim semaphore))
            {
                semaphore.Dispose();
                lockDictionary.TryRemove(key, out _);
            }
        }
    }
}
