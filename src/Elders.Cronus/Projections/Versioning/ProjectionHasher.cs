using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.Projections.Versioning
{
    public class ProjectionHasher
    {
        private static readonly char[] padding = { '=' };
        private readonly ConcurrentDictionary<Type, string> hashCache;

        public ProjectionHasher()
        {
            hashCache = new ConcurrentDictionary<Type, string>();
        }

        public string CalculateHash(string projectionName)
        {
            Type projectionType = projectionName.GetTypeByContract();
            return CalculateHash(projectionType);
        }

        public string CalculateHash(Type projectionType)
        {
            if (hashCache.TryGetValue(projectionType, out string hash) == false)
            {
                if (typeof(INonVersionableProjection).IsAssignableFrom(projectionType))
                {
                    hash = ((INonVersionableProjection)Activator.CreateInstance(projectionType)).GetHash();
                }
                else
                {
                    Type ieventHandler = typeof(IEventHandler<>).GetGenericTypeDefinition();
                    List<string> allEvents = projectionType
                        .GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == ieventHandler)
                        .Select(x => x.GetGenericArguments().First().GetContractId())
                        .ToList();

                    hash = CalculateHash(allEvents);
                }

                hashCache.TryAdd(projectionType, hash);
            }
            return hash;
        }

        string CalculateHash(List<string> contractIds)
        {
            int hashCode = HashCodeUtility.GetPersistentHashCode(contractIds);
            byte[] b = BitConverter.GetBytes(hashCode);
            string hash = Convert.ToBase64String(b);

            hash = hash.TrimEnd(padding).Replace("+", string.Empty).Replace("/", string.Empty).ToLower();

            return hash;
        }
    }
}
