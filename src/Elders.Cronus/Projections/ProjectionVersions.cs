using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Elders.Cronus.Projections
{
    [DataContract(Name = "fe1b2668-75e4-4b29-b2b0-b1db2c10a685")]
    public class ProjectionVersions : ICollection<ProjectionVersion>
    {
        public ProjectionVersions(HashSet<ProjectionVersion> seed)
        {
            versions = seed;
        }

        public ProjectionVersions() : this(new HashSet<ProjectionVersion>()) { }

        [DataMember(Order = 1)]
        HashSet<ProjectionVersion> versions;

        public int Count { get { return versions.Count; } }

        public bool IsReadOnly { get { return true; } }

        public void ValidateVersion(ProjectionVersion version)
        {
            if (version is null) throw new ArgumentNullException(nameof(version));

            var existingVersion = this.FirstOrDefault();
            if (ReferenceEquals(null, existingVersion))
                return;

            if (existingVersion.ProjectionName.Equals(version.ProjectionName, StringComparison.OrdinalIgnoreCase) == false)
            {
                throw new ArgumentException("Invalid version. " + version.ToString() + Environment.NewLine + "Expected version for projection: " + existingVersion.ProjectionName, nameof(version));
            }
        }

        public ProjectionVersions WithoutTheGarbage()
        {
            HashSet<ProjectionVersion> result = new HashSet<ProjectionVersion>();

            foreach (var version in versions)
            {
                if (version.Status != ProjectionStatus.Building)
                {
                    var versionInBuild = this.Where(x => x == version.WithStatus(ProjectionStatus.Building)).SingleOrDefault(); // searches for building version for the version hash
                    result.Remove(versionInBuild);

                    if (version.Status != ProjectionStatus.Live)
                        result.Add(version);
                }

                if (version.Status == ProjectionStatus.Building)
                    result.Add(version);

                if (version.Status == ProjectionStatus.Live)
                {
                    var canceled = this.Where(x => x == version.WithStatus(ProjectionStatus.Canceled)).SingleOrDefault();
                    result.Remove(canceled);

                    var timedout = this.Where(x => x == version.WithStatus(ProjectionStatus.Timedout)).SingleOrDefault();
                    result.Remove(timedout);

                    var currentLiveVer = GetLive();
                    if (ReferenceEquals(null, currentLiveVer) || currentLiveVer <= version)
                    {
                        result.Remove(currentLiveVer);
                        result.Add(version);
                    }
                }
            }

            return new ProjectionVersions(result);
        }

        public void Add(ProjectionVersion version)
        {
            ValidateVersion(version);

            versions.Add(version);

            if (version.Status == ProjectionStatus.Live)
            {
                if (liveVersion is null || liveVersion < version)
                    liveVersion = version;
            }
        }

        public void Clear()
        {
            versions.Clear();
        }

        public bool Contains(ProjectionVersion item)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));

            return this.Contains(item);
        }

        public void CopyTo(ProjectionVersion[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(ProjectionVersion item)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));

            return versions.Remove(item);
        }



        public ProjectionVersion GetNext()
        {
            if (this.Count == 0) return default(ProjectionVersion);

            var maxRevision = this.Max(ver => ver.Revision);
            var candidate = this.Where(x => x.Revision == maxRevision).FirstOrDefault();
            return candidate.NextRevision();
        }

        ProjectionVersion liveVersion;

        public ProjectionVersion GetLive() => liveVersion;

        public bool IsCanceled(ProjectionVersion version)
        {
            if (version is null) throw new ArgumentNullException(nameof(version));

            return versions.Where(ver => ver == version.WithStatus(ProjectionStatus.Canceled)).Any();
        }

        public bool IsOutdatad(ProjectionVersion version)
        {
            if (version is null) throw new ArgumentNullException(nameof(version));

            if (liveVersion is null)
                return false;

            return liveVersion > version;
        }

        public bool IsNotPresent()
        {
            return this.Any() == false;
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
