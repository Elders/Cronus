using System;
using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.Projections.Versioning
{
    public class ProjectionHasher
    {
        static readonly char[] padding = { '=' };

        public string CalculateHash(string projectionName)
        {
            Type projectionType = projectionName.GetTypeByContract();
            return CalculateHash(projectionType);
        }

        public string CalculateHash(Type projectionType)
        {
            if (typeof(INonVersionableProjection).IsAssignableFrom(projectionType))
            {
                return ((INonVersionableProjection)Activator.CreateInstance(projectionType)).GetHash();
            }

            var ieventHandler = typeof(IEventHandler<>).GetGenericTypeDefinition();
            var allEvents = projectionType
                .GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == ieventHandler)
                .Select(x => x.GetGenericArguments().First().GetContractId())
                .ToList();

            return CalculateHash(allEvents);
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
