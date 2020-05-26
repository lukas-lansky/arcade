using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Microsoft.DotNet.Helix.JobSender.Tests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]

namespace Microsoft.DotNet.Helix.Client
{
    internal static class Helpers
    {
        public static string RemoveTrailingSlash(string directoryPath)
        {
            return directoryPath.TrimEnd('/', '\\');
        }

        public static string ComputePathHash(string normalizedPath)
        {
            using (var hasher = SHA256.Create())
            {
                var dirHash = Convert.ToBase64String(hasher.ComputeHash(Encoding.UTF8.GetBytes(normalizedPath)));
                dirHash = dirHash.TrimEnd('='); // base64 url encode it.
                dirHash = dirHash.Replace('+', '-');
                dirHash = dirHash.Replace('/', '_');
                return dirHash;
            }
        }

        public static T MutexExec<T>(Func<Task<T>> f, string mutexName)
        {
            using (var mutex = new Mutex(false, mutexName))
            {
                bool hasMutex = false;
                try
                {
                    try
                    {
                        mutex.WaitOne();
                    }
                    catch (AbandonedMutexException)
                    {
                    }

                    hasMutex = true;
                    return f().GetAwaiter().GetResult(); // Can't await because of mutex
                }
                finally
                {
                    if (hasMutex)
                    {
                        mutex.ReleaseMutex();
                    }
                }
            }
        }

        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> pair, out TKey key, out TValue value) { key = pair.Key; value = pair.Value; }
    }
}
