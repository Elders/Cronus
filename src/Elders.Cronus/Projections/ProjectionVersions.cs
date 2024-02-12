using Elders.Cronus.Projections.Versioning;
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
            if (seed.Any() == false) throw new ArgumentException("ProjectionVersions seed cannot be empty.", nameof(seed));

            versions = new HashSet<ProjectionVersion>();
            foreach (var item in seed)
            {
                Add(item);
            }
        }

        public ProjectionVersions(ProjectionVersion seed) : this(new[] { seed }) { }

        /// <summary>
        /// This constructor is special. It is used only internally so we could get an instance of this object via reflection.
        /// Should be used with special care. You have to add at least one version if you use it.
        /// </summary>
        internal ProjectionVersions() { versions = new HashSet<ProjectionVersion>(); }

        [DataMember(Order = 1)]
        HashSet<ProjectionVersion> versions;

        public string ProjectionName => versions.FirstOrDefault().ProjectionName;

        public int Count { get { return versions.Count; } }

        public bool IsReadOnly { get { return true; } }

        private bool IsValidVersion(ProjectionVersion version)
        {
            if (version is null) throw new ArgumentNullException(nameof(version));

            var existingVersion = versions.FirstOrDefault();
            if (existingVersion is null)
                return true;

            if (existingVersion.ProjectionName.Equals(version.ProjectionName, StringComparison.OrdinalIgnoreCase) == false)
            {
                return false;
                throw new ArgumentException("Invalid version. " + version.ToString() + Environment.NewLine + "Expected version for projection: " + existingVersion.ProjectionName, nameof(version));
            }

            return true;
        }

        public void Add(ProjectionVersion version)
        {
            if (IsValidVersion(version) == false)
                return;

            if (version.Status == ProjectionStatus.NotPresent)
                return;

            if (version.Status == ProjectionStatus.Live || version.Status == ProjectionStatus.Paused || version.Status == ProjectionStatus.Canceled || version.Status == ProjectionStatus.Timedout)
            {
                var versionInRebuild = versions.Where(x => x == version.WithStatus(ProjectionStatus.Fixing)).SingleOrDefault(); // searches for building version for the version hash
                versions.Remove(versionInRebuild);

                var versionInReplay = versions.Where(x => x == version.WithStatus(ProjectionStatus.New)).SingleOrDefault(); // searches for building version for the version hash
                versions.Remove(versionInReplay);

                if (version.Status == ProjectionStatus.Paused || version.Status == ProjectionStatus.Canceled || version.Status == ProjectionStatus.Timedout)
                    versions.Add(version);
            }

            var testVersionSequenceWithThisLiveVersion = GetLive();
            if (testVersionSequenceWithThisLiveVersion is not null && version < testVersionSequenceWithThisLiveVersion)
                return;

            if (version.Status == ProjectionStatus.Fixing || version.Status == ProjectionStatus.New)
                versions.Add(version);

            if (version.Status == ProjectionStatus.Live)
            {
                var paused = versions.Where(x => x == version.WithStatus(ProjectionStatus.Paused)).SingleOrDefault();
                versions.Remove(paused);

                var canceled = versions.Where(x => x == version.WithStatus(ProjectionStatus.Canceled)).SingleOrDefault();
                versions.Remove(canceled);

                var timedout = versions.Where(x => x == version.WithStatus(ProjectionStatus.Timedout)).SingleOrDefault();
                versions.Remove(timedout);

                var currentLiveVer = GetLive();
                if (currentLiveVer is null || currentLiveVer <= version)
                    versions.Remove(currentLiveVer);

                versions.Add(version);
            }
        }

        public IEnumerable<ProjectionVersion> GetBuildingVersions()
        {
            return versions
                .Where(ver => ver.Status == ProjectionStatus.New || ver.Status == ProjectionStatus.Fixing)
                .OrderByDescending(x => x.Revision);
        }

        public bool IsInProgress()
        {
            return GetBuildingVersions().Any();
        }

        public bool HasRebuildInProgress()
        {
            return versions
                .Where(ver => ver.Status == ProjectionStatus.Fixing)
                .Any();
        }

        public bool HasReplayInProgress()
        {
            return versions
                .Where(ver => ver.Status == ProjectionStatus.New)
                .Any();
        }

        public bool Contains(ProjectionVersion item)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));

            return versions.Contains(item);
        }

        public bool Remove(ProjectionVersion item)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));

            if (versions.Count == 1)
                return false;

            return versions.Remove(item);
        }

        public ProjectionVersion GetNext(IProjectionVersioningPolicy policy, string hash)
        {
            if (IsVersionable(policy))
            {
                var maxRevision = versions.Max(ver => ver.Revision);
                var candidate = versions.Where(x => x.Revision == maxRevision).FirstOrDefault(); // TODO: This will crash with null ref
                return candidate.NextRevision(hash);
            }
            else
            {
                if (HasLiveVersion)
                {
                    return GetLive().NonVersionableRevision(hash); // here comes the problem 
                }
                else
                {
                    var maxRevision = versions.Max(ver => ver.Revision);
                    var candidate = versions.Where(x => x.Revision == maxRevision).FirstOrDefault(); // TODO: This will crash with null ref
                    return candidate.NonVersionableRevision(hash);
                }
            }
        }

        public bool IsVersionable(IProjectionVersioningPolicy policy)
        {
            try
            {
                string projectionName = versions.First().ProjectionName;
                return policy.IsVersionable(projectionName);
            }
            catch (Exception)
            {
                return true;
            }
        }

        public bool HasRebuildingVersion()
        {
            return versions.Any(ver => ver.Status == ProjectionStatus.Fixing);
        }

        public bool IsHashTheLiveOne(string hash)
        {
            if (HasLiveVersion == false)
                return false;

            bool isHashTheLiveOne = GetLive().Hash.Equals(hash, StringComparison.Ordinal);
            return isHashTheLiveOne;
        }

        public bool HasLiveVersion => GetLive() is null == false;

        public ProjectionVersion GetLive()
        {
            var liveVersions = versions.Where(x => x.Status == ProjectionStatus.Live);
            return liveVersions.FirstOrDefault();
        }

        public bool IsCanceled(ProjectionVersion version)
        {
            if (version is null) throw new ArgumentNullException(nameof(version));

            return versions.Where(ver => ver == version.WithStatus(ProjectionStatus.Canceled)).Any();
        }

        public bool IsOutdated(ProjectionVersion version)
        {
            if (version is null) throw new ArgumentNullException(nameof(version));

            if (GetLive() is null)
                return false;

            return GetLive() > version;
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
