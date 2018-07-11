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
            versions = new HashSet<ProjectionVersion>();
        }

        [DataMember(Order = 1)]
        HashSet<ProjectionVersion> versions;

        public int Count { get { return versions.Count; } }

        public bool IsReadOnly { get { return true; } }

        public void ValidateVersion(ProjectionVersion version)
        {
            var existingVersion = this.FirstOrDefault();
            if (ReferenceEquals(null, existingVersion))
                return;

            if (existingVersion.ProjectionName.Equals(version.ProjectionName, StringComparison.OrdinalIgnoreCase) == false)
            {
                throw new ArgumentException("Invalid version. " + version.ToString() + Environment.NewLine + "Expected version for projection: " + existingVersion.ProjectionName, nameof(version));
            }
        }

        // fix me
        public void Add(ProjectionVersion version)
        {
            if (ReferenceEquals(null, version)) throw new ArgumentNullException(nameof(version));
            ValidateVersion(version);

            if (version.Status != ProjectionStatus.Building)
            {
                var versionInBuild = this.Where(x => x == version.WithStatus(ProjectionStatus.Building)).SingleOrDefault(); // searches for building version for the version hash
                versions.Remove(versionInBuild);

                if (version.Status != ProjectionStatus.Live)
                    versions.Add(version);
            }

            if (version.Status == ProjectionStatus.Building)
                versions.Add(version);

            if (version.Status == ProjectionStatus.Live)
            {
                var canceled = this.Where(x => x == version.WithStatus(ProjectionStatus.Canceled)).SingleOrDefault();
                versions.Remove(canceled);

                var timedout = this.Where(x => x == version.WithStatus(ProjectionStatus.Timedout)).SingleOrDefault();
                versions.Remove(timedout);

                var currentLiveVer = GetLive();
                if (ReferenceEquals(null, currentLiveVer) || currentLiveVer <= version)
                {
                    versions.Remove(currentLiveVer);
                    versions.Add(version);
                }
            }
        }

        public ProjectionVersion GetLatest()
        {
            if (this.Count == 0) return default(ProjectionVersion);

            var maxRevision = this.Max(ver => ver.Revision);
            return this.Where(x => x.Revision == maxRevision).SingleOrDefault();
        }

        public ProjectionVersion GetNext()
        {
            if (this.Count == 0) return default(ProjectionVersion);

            return GetLatest().NextRevision();
        }

        public ProjectionVersion GetLive()
        {
            if (this.Count == 0) return default(ProjectionVersion);

            return this.Where(x => x.Status == ProjectionStatus.Live).SingleOrDefault();
        }

        public void Clear()
        {
            versions.Clear();
        }

        public bool Contains(ProjectionVersion item)
        {
            return this.Contains(item);
        }

        public void CopyTo(ProjectionVersion[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(ProjectionVersion item)
        {
            return versions.Remove(item);
        }

        public IEnumerator<ProjectionVersion> GetEnumerator()
        {
            return new HashSet<ProjectionVersion>(versions).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new HashSet<ProjectionVersion>(versions).GetEnumerator();
        }
    }

}
