using System.Runtime.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace Elders.Cronus.Projections
{
    [DataContract(Name = "fe1b2668-75e4-4b29-b2b0-b1db2c10a685")]
    public class ProjectionVersions : ICollection<ProjectionVersion>
    {
        public ProjectionVersions()
        {
            Versions = new HashSet<ProjectionVersion>();
        }

        public ProjectionVersions(IEnumerable<ProjectionVersion> versions)
        {
            Versions = new HashSet<ProjectionVersion>(versions);
        }

        [DataMember(Order = 1)]
        public HashSet<ProjectionVersion> Versions { get; private set; }

        public int Count { get { return Versions.Count; } }

        public bool IsReadOnly { get { return true; } }

        public void ValidateVersion(ProjectionVersion version)
        {
            var existingVersion = Versions.FirstOrDefault();
            if (ReferenceEquals(null, existingVersion))
                return;

            if (existingVersion.ProjectionContractId.Equals(version.ProjectionContractId, StringComparison.OrdinalIgnoreCase) == false)
            {
                throw new ArgumentException("Invalid version. " + version.ToString() + Environment.NewLine + "Expecting version similar to an existing one: " + existingVersion, nameof(version));
            }
        }

        public void Add(ProjectionVersion version)
        {
            if (ReferenceEquals(null, version)) throw new ArgumentNullException(nameof(version));
            ValidateVersion(version);

            if (version.Status == ProjectionStatus.Canceled)
            {
                var versionInBuild = Versions.Where(x => x == version.WithStatus(ProjectionStatus.Building)).SingleOrDefault();
                if (Versions.Remove(versionInBuild))
                    Versions.Add(version);
            }
            else if (version.Status == ProjectionStatus.Live)
            {
                var versionInBuild = Versions.Where(x => x == version.WithStatus(ProjectionStatus.Building)).SingleOrDefault();
                Versions.Remove(versionInBuild);
                Versions.Add(version);
            }
            else
            {
                Versions.Add(version);
            }
        }

        public ProjectionVersion GetLatest()
        {
            return Versions.Where(x => x.Revision == Versions.Max(ver => ver.Revision)).SingleOrDefault();
        }

        public ProjectionVersion GetNextRevision()
        {
            return GetLatest().NextRevision();
        }

        public ProjectionVersion GetLive()
        {
            return Versions.Where(x => x.Status == ProjectionStatus.Live).SingleOrDefault();
        }

        public void Clear()
        {
            Versions.Clear();
        }

        public bool Contains(ProjectionVersion item)
        {
            return Versions.Contains(item);
        }

        public void CopyTo(ProjectionVersion[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(ProjectionVersion item)
        {
            return Versions.Remove(item);
        }

        public IEnumerator<ProjectionVersion> GetEnumerator()
        {
            return Versions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Versions.GetEnumerator();
        }
    }

}
