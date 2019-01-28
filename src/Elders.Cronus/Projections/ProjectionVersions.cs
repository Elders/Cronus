using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Elders.Cronus.Projections
{
    [DataContract(Name = "fe1b2668-75e4-4b29-b2b0-b1db2c10a685")]
    public class ProjectionVersions
    {
        public ProjectionVersions(IEnumerable<ProjectionVersion> seed)
        {
            versions = new HashSet<ProjectionVersion>();
            foreach (var item in seed)
            {
                Add(item);
            }
        }

        public ProjectionVersions() : this(new HashSet<ProjectionVersion>()) { }

        [DataMember(Order = 1)]
        HashSet<ProjectionVersion> versions;

        public int Count { get { return versions.Count; } }

        public bool IsReadOnly { get { return true; } }

        public void ValidateVersion(ProjectionVersion version)
        {
            if (version is null) throw new ArgumentNullException(nameof(version));

            var existingVersion = versions.FirstOrDefault();
            if (ReferenceEquals(null, existingVersion))
                return;

            if (existingVersion.ProjectionName.Equals(version.ProjectionName, StringComparison.OrdinalIgnoreCase) == false)
            {
                throw new ArgumentException("Invalid version. " + version.ToString() + Environment.NewLine + "Expected version for projection: " + existingVersion.ProjectionName, nameof(version));
            }
        }

        public void Add(ProjectionVersion version)
        {
            ValidateVersion(version);

            if (version.Status == ProjectionStatus.NotPresent)
                return;

            if (version.Status != ProjectionStatus.Building)
            {
                var versionInBuild = versions.Where(x => x == version.WithStatus(ProjectionStatus.Building)).SingleOrDefault(); // searches for building version for the version hash
                versions.Remove(versionInBuild);

                if (version.Status != ProjectionStatus.Live)
                    versions.Add(version);
            }

            if (version.Status == ProjectionStatus.Building)
                versions.Add(version);

            if (version.Status == ProjectionStatus.Live)
            {
                var canceled = versions.Where(x => x == version.WithStatus(ProjectionStatus.Canceled)).SingleOrDefault();
                versions.Remove(canceled);

                var timedout = versions.Where(x => x == version.WithStatus(ProjectionStatus.Timedout)).SingleOrDefault();
                versions.Remove(timedout);

                var currentLiveVer = GetLive();
                if (currentLiveVer is null || currentLiveVer <= version)
                {
                    versions.Remove(currentLiveVer);
                    versions.Add(version);
                }
            }
        }

        public IEnumerable<ProjectionVersion> GetBuildingVersions()
        {
            return versions
                .Where(ver => ver.Status == ProjectionStatus.Building)
                .OrderByDescending(x => x.Revision);
        }

        public bool HasBuildingVersion()
        {
            return GetBuildingVersions().Any();
        }

        public void Clear()
        {
            versions.Clear();
        }

        public bool Contains(ProjectionVersion item)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));

            return versions.Contains(item);
        }

        public bool Remove(ProjectionVersion item)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));

            return versions.Remove(item);
        }

        public ProjectionVersion GetNext()
        {
            if (versions.Count == 0) return default(ProjectionVersion);

            var maxRevision = versions.Max(ver => ver.Revision);
            var candidate = versions.Where(x => x.Revision == maxRevision).FirstOrDefault();
            return candidate.NextRevision();
        }

        public bool IsHashTheLiveOne(string hash)
        {
            if (HasLiveVersion == false)
                return false;

            bool isHashTheLiveOne = GetLive().Hash.Equals(hash, StringComparison.Ordinal);
            return isHashTheLiveOne;
        }

        public bool HasLiveVersion => GetLive() is null == false;

        public ProjectionVersion GetLive() => versions.Where(x => x.Status == ProjectionStatus.Live).SingleOrDefault();

        public bool IsCanceled(ProjectionVersion version)
        {
            if (version is null) throw new ArgumentNullException(nameof(version));

            return versions.Where(ver => ver == version.WithStatus(ProjectionStatus.Canceled)).Any();
        }

        public bool IsOutdatad(ProjectionVersion version)
        {
            if (version is null) throw new ArgumentNullException(nameof(version));

            if (GetLive() is null)
                return false;

            return GetLive() > version;
        }

        public bool IsNotPresent()
        {
            return versions.Any() == false;
        }

        public IEnumerator<ProjectionVersion> GetEnumerator()
        {
            return new HashSet<ProjectionVersion>(versions).GetEnumerator();
        }

        public override string ToString()
        {
            StringBuilder info = new StringBuilder();
            foreach (ProjectionVersion version in versions)
            {
                info.AppendLine(version.ToString());
            }
            return info.ToString();
        }
    }
}
