using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;
using System.IO;
using NHibernate.Cfg;
using NHibernate.Mapping.ByCode;
using NHibernate.Tool.hbm2ddl;
using NMSD.Cronus.Persistence.MSSQL;
using NMSD.Cronus.DomainModelling;
using NHibernate;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using System.Data;
using System.Reflection;
using System.Collections;
using NHibernate.Type;
using NHibernate.Collection;
using NHibernate.Engine;
using NHibernate.Persister.Collection;
using System.Text;
using NHibernate.Loader;
using System.Diagnostics;
using NHibernate.DebugHelpers;
using NHibernate.Util;

namespace NMSD.Cronus.Sample.CommonFiles
{
    public static class NHibernateExtensions
    {
        /// <summary>
        /// Adds all Hyperion mappings to a NHibernate configuration.
        /// </summary>
        /// <param name="nhConf">The NHib configuration instance.</param>
        /// <returns>Returns the NHib configuration instance with Hyperion mappings.</returns>
        public static Configuration AddAutoMappings(this Configuration nhConf, IEnumerable<Type> types, Action<ModelMapper> modelMapper = null)
        {
            var englishPluralizationService = PluralizationService.CreateService(new CultureInfo("en-US"));
            var mapper = new ModelMapper(new HyperionModelInspector());

            //var baseEntityType = typeof(IEventHandler);
            //mapper.IsEntity((t, declared) => baseEntityType.IsAssignableFrom(t) && baseEntityType != t && !t.IsInterface);
            //mapper.IsRootEntity((t, declared) => baseEntityType.Equals(t.BaseType));

            mapper.BeforeMapClass += (insp, prop, map) => map.Table(englishPluralizationService.Pluralize(prop.Name));

            mapper.BeforeMapClass += (i, t, cm) =>
            {

                var memberInfo = t.GetProperty("Id", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var propType = memberInfo.PropertyType;
                if (typeof(AggregateRootId).IsAssignableFrom(propType))
                {

                    cm.ComponentAsId(memberInfo, x =>
                    {
                        //   var props = memberInfo.PropertyType.GetProperty("Id", BindingFlags.Instance | BindingFlags.NonPublic);
                        var property = memberInfo.PropertyType.GetProperty("Id");
                        x.Property(property, y => y.Column(t.Name + "Id"));
                    });
                }
                else
                {
                    cm.Id(map =>
                    {
                        map.Column(t.Name + "Id");

                        if (propType == typeof(Int32))
                            map.Generator(Generators.Identity);
                        else
                            map.Generator(Generators.Assigned);
                    });
                }
            };
            mapper.BeforeMapProperty += (modelInsperctor, member, propertyCustomizer) =>
            {
                if (typeof(AggregateRootId).IsAssignableFrom(member.LocalMember.GetPropertyOrFieldType()))
                {
                    var generic = typeof(AggregateIdUserType<>);
                    var t = member.LocalMember.GetPropertyOrFieldType();
                    var actualType = generic.MakeGenericType(t);
                    propertyCustomizer.Type(actualType, null);


                }
                else if (typeof(DateTime) == member.LocalMember.GetPropertyOrFieldType())
                {

                    propertyCustomizer.Type(typeof(FileTimeUtc), null);

                }
            };
            mapper.BeforeMapManyToOne += (insp, prop, map) => map.Column(prop.LocalMember.GetPropertyOrFieldType().Name + "Id");
            mapper.BeforeMapManyToOne += (insp, prop, map) => map.Cascade(NHibernate.Mapping.ByCode.Cascade.None);

            mapper.BeforeMapBag += (insp, prop, map) => map.Key(km => km.Column(prop.GetContainerEntity(insp).Name + "Id"));
            mapper.BeforeMapBag += (insp, prop, map) =>
            {
                map.Cascade(NHibernate.Mapping.ByCode.Cascade.All.Include(NHibernate.Mapping.ByCode.Cascade.DeleteOrphans));
                map.Inverse(true);
            };


            mapper.BeforeMapSet += (insp, prop, map) => map.Key(km => km.Column(prop.GetContainerEntity(insp).Name + "Id"));
            mapper.BeforeMapSet += (insp, prop, map) =>
            {
                map.Cascade(NHibernate.Mapping.ByCode.Cascade.All.Include(NHibernate.Mapping.ByCode.Cascade.DeleteOrphans));
                map.Inverse(true);
            };

            if (modelMapper != null)
                modelMapper(mapper);

            var mapping = mapper.CompileMappingFor(types);

            nhConf.AddDeserializedMapping(mapping, "Hyperion");

            return nhConf;
        }



        /// <summary>
        /// Drops the database based on the mappings.
        /// </summary>
        /// <param name="nhConf">The NHib configuration instance.</param>
        /// <returns>Returns the NHib configuration instance.</returns>
        public static Configuration DropTables(this Configuration nhConf)
        {
            new SchemaExport(nhConf).Drop(false, true);
            return nhConf;
        }

        /// <summary>
        /// Drops and creates the database based on the mappings.
        /// </summary>
        /// <param name="nhConf">The NHib configuration instance.</param>
        /// <returns>Returns the NHib configuration instance.</returns>
        public static Configuration CreateTables(this Configuration nhConf)
        {
            new SchemaExport(nhConf).Create(false, true);
            return nhConf;
        }

        public static Configuration CreateDatabase(this Configuration nhConf, string connectionString = null)
        {
            string conString =
                connectionString ??
                nhConf.GetProperty(NHibernate.Cfg.Environment.ConnectionString) ??
                System.Configuration.ConfigurationManager.ConnectionStrings[nhConf.GetProperty(NHibernate.Cfg.Environment.ConnectionStringName)].ConnectionString;

            if (!DatabaseManager.DatabaseExists(conString))
            {
                DatabaseManager.CreateDatabase(conString, enableSnapshotIsolation: true);
                nhConf.CreateTables();
            }

            return nhConf;
        }

        public static Configuration CreateDatabase_AND_OVERWRITE_EXISTING_DATABASE(this Configuration nhConf, string connectionString = null)
        {
            string conString =
                connectionString ??
                nhConf.GetProperty(NHibernate.Cfg.Environment.ConnectionString) ??
                System.Configuration.ConfigurationManager.ConnectionStrings[nhConf.GetProperty(NHibernate.Cfg.Environment.ConnectionStringName)].ConnectionString;

            if (DatabaseManager.DatabaseExists(conString))
                DatabaseManager.DeleteDatabase(conString);

            DatabaseManager.CreateDatabase(conString, enableSnapshotIsolation: true);
            nhConf.CreateTables();

            return nhConf;
        }
    }

    [Serializable]
    public sealed class BinaryWithProtobufUserType : NHibernate.UserTypes.IUserType
    {
        private Byte[] data = null;

        public static Protoreg.ProtoregSerializer Serializer { get; set; }

        public Boolean IsMutable { get { return false; } }

        public Object Assemble(Object cached, Object owner)
        {
            return (cached);
        }

        public Object DeepCopy(Object value)
        {
            return (value);
        }

        public Object Disassemble(Object value)
        {
            return (value);
        }

        public new Boolean Equals(Object x, Object y)
        {
            return (Object.Equals(x, y));
        }

        public Int32 GetHashCode(Object x)
        {
            return ((x != null) ? x.GetHashCode() : 0);
        }

        public override Int32 GetHashCode()
        {
            return ((this.data != null) ? this.data.GetHashCode() : 0);
        }

        public override Boolean Equals(Object obj)
        {
            BinaryWithProtobufUserType other = obj as BinaryWithProtobufUserType;

            if (other == null)
            {
                return (false);
            }

            if (Object.ReferenceEquals(this, other) == true)
            {
                return (true);
            }

            return (this.data.SequenceEqual(other.data));
        }

        public Object NullSafeGet(System.Data.IDataReader rs, String[] names, Object owner)
        {
            Int32 index = rs.GetOrdinal(names[0]);
            Byte[] data = rs.GetValue(index) as Byte[];

            this.data = data as Byte[];

            if (data == null)
            {
                return (null);
            }

            using (MemoryStream stream = new MemoryStream(this.data ?? new Byte[0]))
            {
                var desObject = Serializer.Deserialize(stream);

                return desObject;
            }
        }

        public void NullSafeSet(System.Data.IDbCommand cmd, Object value, Int32 index)
        {
            if (value != null)
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    Serializer.Serialize(stream, value);
                    value = stream.ToArray();
                }
            }

            (cmd.Parameters[index] as System.Data.Common.DbParameter).Value = value ?? DBNull.Value;
        }

        public Object Replace(Object original, Object target, Object owner)
        {
            return (original);
        }

        public Type ReturnedType
        {
            get
            {
                return (typeof(List<NMSD.Cronus.DomainModelling.IEvent>));
            }
        }

        public NHibernate.SqlTypes.SqlType[] SqlTypes
        {
            get
            {
                return (new NHibernate.SqlTypes.SqlType[] { new NHibernate.SqlTypes.SqlType(System.Data.DbType.Binary) });
            }
        }
    }

    public class AggregateIdUserType<T> : IUserType where T : IAggregateRootId
    {
        bool IUserType.Equals(object x, object y)
        {
            if (ReferenceEquals(null, x) && ReferenceEquals(null, y))
                return true;
            if (ReferenceEquals(null, x))
                return false;
            else
                return x.Equals(y);
        }

        public object Assemble(object cached, object owner)
        {
            return cached;
        }

        public object DeepCopy(object value)
        {
            var copy = Activator.CreateInstance(typeof(T), ((T)value).Id);
            return copy;
        }

        public object Disassemble(object value)
        {
            return value;
        }

        public int GetHashCode(object x)
        {
            return x.GetHashCode();
        }

        public bool IsMutable
        {
            get { return true; }
        }

        public object NullSafeGet(System.Data.IDataReader rs, string[] names, object owner)
        {
            var index = rs.GetOrdinal(names[0]);
            if (rs.IsDBNull(index) || ((Guid)rs[index]) == default(Guid))
                return null;

            var result = Activator.CreateInstance(typeof(T), (Guid)rs[index]);
            return ((T)result);
            ;
        }

        public void NullSafeSet(System.Data.IDbCommand cmd, object value, int index)
        {
            if (value == null || value == DBNull.Value)
                NHibernateUtil.Guid.NullSafeSet(cmd, null, index);

            NHibernateUtil.Guid.Set(cmd, ((T)value).Id, index);
        }

        public object Replace(object original, object target, object owner)
        {
            return original;
        }

        public Type ReturnedType
        {
            get { return typeof(T); }
        }

        public NHibernate.SqlTypes.SqlType[] SqlTypes
        {
            get { return new SqlType[] { NHibernateUtil.Guid.SqlType }; }
        }
    }

    public class FileTimeUtc : NHibernate.UserTypes.IUserType
    {
        public SqlType[] SqlTypes
        {
            get
            {
                SqlType[] types = new SqlType[1];
                types[0] = new SqlType(DbType.Int64);
                return types;
            }
        }

        public System.Type ReturnedType
        {
            get { return typeof(DateTime); }
        }

        public new bool Equals(object x, object y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x == null)
            {
                return false;
            }
            else
            {
                return x.Equals(y);
            }
        }

        public int GetHashCode(object x)
        {
            return x.GetHashCode();
        }

        public object NullSafeGet(IDataReader rs, string[] names, object owner)
        {
            object results = NHibernateUtil.Int64.NullSafeGet(rs, names[0]);

            if (results == null)
                return null;

            long utcValue = (long)results;

            DateTime result = DateTime.FromFileTimeUtc(utcValue);
            return result;
        }

        public void NullSafeSet(IDbCommand cmd, object value, int index)
        {

            DateTime val = (DateTime)value;
            if (value == null || val == null)
            {
                NHibernateUtil.Int64.NullSafeSet(cmd, null, index);
                return;
            }
            long utcDatetime = val.ToFileTimeUtc();
            NHibernateUtil.Int64.NullSafeSet(cmd, utcDatetime, index);
        }

        public object DeepCopy(object value)
        {
            //We deep copy the uri by creating a new instance with the same contents
            if (value == null) return null;

            DateTime deepCopy = (DateTime)value;
            return deepCopy;
        }

        public bool IsMutable
        {
            get { return false; }
        }

        public object Replace(object original, object target, object owner)
        {
            //As our object is immutable we can just return the original
            return original;
        }

        public object Assemble(object cached, object owner)
        {
            //Used for casching, as our object is immutable we can just return it as is
            return DeepCopy(cached);
        }

        public object Disassemble(object value)
        {
            //Used for casching, as our object is immutable we can just return it as is
            return DeepCopy(value);
        }
    }

    public class GuidListUserType : IUserType
    {
        private const char cStringSeparator = '#';

        bool IUserType.Equals(object x, object y)
        {
            if (x == null || y == null) return false;
            List<Guid> xl = (List<Guid>)x;
            List<Guid> yl = (List<Guid>)y;
            if (xl.Count != yl.Count) return false;
            Boolean retvalue = xl.Except(yl).Count() == 0;
            return retvalue;
        }

        public object Assemble(object cached, object owner)
        {
            return cached;
        }

        public object DeepCopy(object value)
        {
            List<Guid> obj = (List<Guid>)value;
            List<Guid> retvalue = new List<Guid>(obj);

            return retvalue;
        }

        public object Disassemble(object value)
        {
            return value;
        }

        public int GetHashCode(object x)
        {
            return x.GetHashCode();
        }

        public bool IsMutable
        {
            get { return true; }
        }

        public object NullSafeGet(System.Data.IDataReader rs, string[] names, object owner)
        {
            List<Guid> result = new List<Guid>();
            Int32 index = rs.GetOrdinal(names[0]);
            if (rs.IsDBNull(index) || String.IsNullOrEmpty((String)rs[index]))
                return result;
            foreach (String s in ((String)rs[index]).Split(cStringSeparator))
            {
                var id = Guid.Parse(s);
                result.Add(id);
            }
            return result;
        }

        public void NullSafeSet(System.Data.IDbCommand cmd, object value, int index)
        {
            if (value == null || value == DBNull.Value)
            {
                NHibernateUtil.StringClob.NullSafeSet(cmd, null, index);
            }
            List<Guid> stringList = (List<Guid>)value;
            StringBuilder sb = new StringBuilder();
            foreach (Guid s in stringList)
            {
                sb.Append(s);
                sb.Append(cStringSeparator);
            }
            if (sb.Length > 0) sb.Length--;
            NHibernateUtil.StringClob.Set(cmd, sb.ToString(), index);
        }

        public object Replace(object original, object target, object owner)
        {
            return original;
        }

        public Type ReturnedType
        {
            get { return typeof(IList<String>); }
        }

        public NHibernate.SqlTypes.SqlType[] SqlTypes
        {
            get { return new SqlType[] { NHibernateUtil.StringClob.SqlType }; }
        }
    }

    public class GuidSetUserType : IUserType
    {
        private const char cStringSeparator = '#';

        bool IUserType.Equals(object x, object y)
        {
            if (x == null || y == null) return false;
            HashSet<Guid> xl = (HashSet<Guid>)x;
            HashSet<Guid> yl = (HashSet<Guid>)y;
            if (xl.Count != yl.Count) return false;
            Boolean retvalue = xl.Except(yl).Count() == 0;
            return retvalue;
        }

        public object Assemble(object cached, object owner)
        {
            return cached;
        }

        public object DeepCopy(object value)
        {
            HashSet<Guid> obj = (HashSet<Guid>)value;
            HashSet<Guid> retvalue = new HashSet<Guid>(obj);

            return retvalue;
        }

        public object Disassemble(object value)
        {
            return value;
        }

        public int GetHashCode(object x)
        {
            return x.GetHashCode();
        }

        public bool IsMutable
        {
            get { return true; }
        }

        public object NullSafeGet(System.Data.IDataReader rs, string[] names, object owner)
        {
            HashSet<Guid> result = new HashSet<Guid>();
            Int32 index = rs.GetOrdinal(names[0]);
            if (rs.IsDBNull(index) || String.IsNullOrEmpty((String)rs[index]))
                return result;
            foreach (String s in ((String)rs[index]).Split(cStringSeparator))
            {
                var id = Guid.Parse(s);
                result.Add(id);
            }
            return result;
        }

        public void NullSafeSet(System.Data.IDbCommand cmd, object value, int index)
        {
            if (value == null || value == DBNull.Value)
            {
                NHibernateUtil.StringClob.NullSafeSet(cmd, null, index);
            }
            HashSet<Guid> stringList = (HashSet<Guid>)value;
            StringBuilder sb = new StringBuilder();
            foreach (Guid s in stringList)
            {
                sb.Append(s);
                sb.Append(cStringSeparator);
            }
            if (sb.Length > 0) sb.Length--;
            NHibernateUtil.StringClob.Set(cmd, sb.ToString(), index);
        }

        public object Replace(object original, object target, object owner)
        {
            return original;
        }

        public Type ReturnedType
        {
            get { return typeof(ISet<String>); }
        }

        public NHibernate.SqlTypes.SqlType[] SqlTypes
        {
            get { return new SqlType[] { NHibernateUtil.StringClob.SqlType }; }
        }
    }

    public class Net4CollectionTypeFactory : DefaultCollectionTypeFactory
    {
        public override CollectionType Set<T>(string role, string propertyRef, bool embedded)
        {
            return new GenericSetType<T>(role, propertyRef);
        }

        public override CollectionType SortedSet<T>(string role, string propertyRef, bool embedded, IComparer<T> comparer)
        {
            return new GenericSortedSetType<T>(role, propertyRef, comparer);
        }
    }

    [Serializable]
    public class GenericSortedSetType<T> : GenericSetType<T>
    {
        private readonly IComparer<T> comparer;

        public GenericSortedSetType(string role, string propertyRef, IComparer<T> comparer)
            : base(role, propertyRef)
        {
            this.comparer = comparer;
        }

        public override object Instantiate(int anticipatedSize)
        {
            return new SortedSet<T>(this.comparer);
        }

        public IComparer<T> Comparer
        {
            get
            {
                return this.comparer;
            }
        }
    }

    /// <summary>
    /// An <see cref="IType"/> that maps an <see cref="ISet{T}"/> collection
    /// to the database.
    /// </summary>
    [Serializable]
    public class GenericSetType<T> : SetType
    {
        /// <summary>
        /// Initializes a new instance of a <see cref="GenericSetType{T}"/> class for
        /// a specific role.
        /// </summary>
        /// <param name="role">The role the persistent collection is in.</param>
        /// <param name="propertyRef">The name of the property in the
        /// owner object containing the collection ID, or <see langword="null" /> if it is
        /// the primary key.</param>
        public GenericSetType(string role, string propertyRef)
            : base(role, propertyRef, false)
        {
        }

        public override Type ReturnedClass
        {
            get { return typeof(ISet<T>); }
        }

        /// <summary>
        /// Instantiates a new <see cref="IPersistentCollection"/> for the set.
        /// </summary>
        /// <param name="session">The current <see cref="ISessionImplementor"/> for the set.</param>
        /// <param name="persister">The current <see cref="ICollectionPersister" /> for the set.</param>
        /// <param name="key"></param>
        public override IPersistentCollection Instantiate(ISessionImplementor session, ICollectionPersister persister,
                                                          object key)
        {
            return new PersistentGenericSet<T>(session);
        }

        /// <summary>
        /// Wraps an <see cref="IList{T}"/> in a <see cref="PersistentGenericSet&lt;T&gt;"/>.
        /// </summary>
        /// <param name="session">The <see cref="ISessionImplementor"/> for the collection to be a part of.</param>
        /// <param name="collection">The unwrapped <see cref="IList{T}"/>.</param>
        /// <returns>
        /// An <see cref="PersistentGenericSet&lt;T&gt;"/> that wraps the non NHibernate <see cref="IList{T}"/>.
        /// </returns>
        public override IPersistentCollection Wrap(ISessionImplementor session, object collection)
        {
            var set = collection as ISet<T>;
            if (set == null)
            {
                var stronglyTypedCollection = collection as ICollection<T>;
                if (stronglyTypedCollection == null)
                    throw new HibernateException(Role + " must be an implementation of ISet<T> or ICollection<T>");
                set = new HashSet<T>(stronglyTypedCollection);
            }
            return new PersistentGenericSet<T>(session, set);
        }

        public override object Instantiate(int anticipatedSize)
        {
            return new HashSet<T>();
        }

        protected override void Clear(object collection)
        {
            ((ISet<T>)collection).Clear();
        }

        protected override void Add(object collection, object element)
        {
            ((ISet<T>)collection).Add((T)element);
        }
    }

    /// <summary>
    /// A persistent wrapper for an <see cref="ISet{T}"/>
    /// </summary>
    [Serializable]
    [DebuggerTypeProxy(typeof(CollectionProxy<>))]
    public class PersistentGenericSet<T> : AbstractPersistentCollection, ISet<T>
    {
        /// <summary>
        /// The <see cref="ISet{T}"/> that NHibernate is wrapping.
        /// </summary>
        protected ISet<T> set;

        /// <summary>
        /// A temporary list that holds the objects while the PersistentSet is being
        /// populated from the database.  
        /// </summary>
        /// <remarks>
        /// This is necessary to ensure that the object being added to the PersistentSet doesn't
        /// have its' <c>GetHashCode()</c> and <c>Equals()</c> methods called during the load
        /// process.
        /// </remarks>
        [NonSerialized]
        private IList<T> tempList;

        public PersistentGenericSet()
        {
        }

        // needed for serialization

        /// <summary> 
        /// Constructor matching super.
        /// Instantiates a lazy set (the underlying set is un-initialized).
        /// </summary>
        /// <param name="session">The session to which this set will belong. </param>
        public PersistentGenericSet(ISessionImplementor session)
            : base(session)
        {
        }

        /// <summary> 
        /// Instantiates a non-lazy set (the underlying set is constructed
        /// from the incoming set reference).
        /// </summary>
        /// <param name="session">The session to which this set will belong. </param>
        /// <param name="original">The underlying set data. </param>
        public PersistentGenericSet(ISessionImplementor session, ISet<T> original)
            : base(session)
        {
            // Sets can be just a view of a part of another collection.
            // do we need to copy it to be sure it won't be changing
            // underneath us?
            // ie. this.set.addAll(set);
            set = original;
            SetInitialized();
            IsDirectlyAccessible = true;
        }

        public override bool RowUpdatePossible
        {
            get { return false; }
        }

        public override bool Empty
        {
            get { return set.Count == 0; }
        }

        public bool IsEmpty
        {
            get { return ReadSize() ? CachedSize == 0 : (set.Count == 0); }
        }

        public object SyncRoot
        {
            get { return this; }
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        #region ISet<T> Members

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            Read();
            return set.GetEnumerator();
        }

        public bool Contains(T o)
        {
            bool? exists = ReadElementExistence(o);
            return exists == null ? set.Contains(o) : exists.Value;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Read();
            Array.Copy(set.ToArray(), 0, array, arrayIndex, Count);
        }

        //public bool ContainsAll(ICollection c)
        //{
        //    Read();
        //    return set.ContainsAll(c);
        //}

        public bool Add(T o)
        {
            bool? exists = IsOperationQueueEnabled ? ReadElementExistence(o) : null;
            if (!exists.HasValue)
            {
                Initialize(true);
                if (set.Add(o))
                {
                    Dirty();
                    return true;
                }
                return false;
            }
            if (exists.Value)
            {
                return false;
            }
            QueueOperation(new SimpleAddDelayedOperation(this, o));
            return true;
        }

        public void UnionWith(IEnumerable<T> other)
        {
            Read();
            set.UnionWith(other);
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            Read();
            set.IntersectWith(other);
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            Read();
            set.ExceptWith(other);
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            Read();
            set.SymmetricExceptWith(other);
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            Read();
            return set.IsProperSupersetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            Read();
            return set.IsSupersetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            Read();
            return set.IsProperSupersetOf(other);
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            Read();
            return set.IsProperSubsetOf(other);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            Read();
            return set.Overlaps(other);
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            Read();
            return set.SetEquals(other);
        }

        public bool Remove(T o)
        {
            bool? exists = PutQueueEnabled ? ReadElementExistence(o) : null;
            if (!exists.HasValue)
            {
                Initialize(true);
                if (set.Remove(o))
                {
                    Dirty();
                    return true;
                }
                return false;
            }
            if (exists.Value)
            {
                QueueOperation(new SimpleRemoveDelayedOperation(this, o));
                return true;
            }
            return false;
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        public void Clear()
        {
            if (ClearQueueEnabled)
            {
                QueueOperation(new ClearDelayedOperation(this));
            }
            else
            {
                Initialize(true);
                if (set.Count != 0)
                {
                    set.Clear();
                    Dirty();
                }
            }
        }

        public int Count
        {
            get { return ReadSize() ? CachedSize : set.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public IEnumerator GetEnumerator()
        {
            Read();
            return set.GetEnumerator();
        }

        #endregion

        #region DelayedOperations

        #region Nested type: ClearDelayedOperation

        protected sealed class ClearDelayedOperation : IDelayedOperation
        {
            private readonly PersistentGenericSet<T> enclosingInstance;

            public ClearDelayedOperation(PersistentGenericSet<T> enclosingInstance)
            {
                this.enclosingInstance = enclosingInstance;
            }

            #region IDelayedOperation Members

            public object AddedInstance
            {
                get { return null; }
            }

            public object Orphan
            {
                get { throw new NotSupportedException("queued clear cannot be used with orphan delete"); }
            }

            public void Operate()
            {
                enclosingInstance.set.Clear();
            }

            #endregion
        }

        #endregion

        #region Nested type: SimpleAddDelayedOperation

        protected sealed class SimpleAddDelayedOperation : IDelayedOperation
        {
            private readonly PersistentGenericSet<T> enclosingInstance;
            private readonly T value;

            public SimpleAddDelayedOperation(PersistentGenericSet<T> enclosingInstance, T value)
            {
                this.enclosingInstance = enclosingInstance;
                this.value = value;
            }

            #region IDelayedOperation Members

            public object AddedInstance
            {
                get { return value; }
            }

            public object Orphan
            {
                get { return null; }
            }

            public void Operate()
            {
                enclosingInstance.set.Add(value);
            }

            #endregion
        }

        #endregion

        #region Nested type: SimpleRemoveDelayedOperation

        protected sealed class SimpleRemoveDelayedOperation : IDelayedOperation
        {
            private readonly PersistentGenericSet<T> enclosingInstance;
            private readonly T value;

            public SimpleRemoveDelayedOperation(PersistentGenericSet<T> enclosingInstance, T value)
            {
                this.enclosingInstance = enclosingInstance;
                this.value = value;
            }

            #region IDelayedOperation Members

            public object AddedInstance
            {
                get { return null; }
            }

            public object Orphan
            {
                get { return value; }
            }

            public void Operate()
            {
                enclosingInstance.set.Remove(value);
            }

            #endregion
        }

        #endregion

        #endregion

        public override ICollection GetSnapshot(ICollectionPersister persister)
        {
            var entityMode = Session.EntityMode;
            var clonedSet = new SetSnapShot<T>(set.Count);
            var enumerable = from object current in set
                             select persister.ElementType.DeepCopy(current, entityMode, persister.Factory);
            foreach (var copied in enumerable)
            {
                clonedSet.Add((T)copied);
            }
            return clonedSet;
        }

        public override ICollection GetOrphans(object snapshot, string entityName)
        {
            var sn = new SetSnapShot<object>((IEnumerable<object>)snapshot);
            if (set.Count == 0) return sn;
            if (((ICollection)sn).Count == 0) return sn;
            return GetOrphans(sn, set.ToArray(), entityName, Session);
        }

        public override bool EqualsSnapshot(ICollectionPersister persister)
        {
            var elementType = persister.ElementType;
            var snapshot = (ISetSnapshot<T>)GetSnapshot();
            if (((ICollection)snapshot).Count != set.Count)
            {
                return false;
            }

            return !(from object obj in set
                     let oldValue = snapshot[(T)obj]
                     where oldValue == null || elementType.IsDirty(oldValue, obj, Session)
                     select obj).Any();
        }

        public override bool IsSnapshotEmpty(object snapshot)
        {
            return ((ICollection)snapshot).Count == 0;
        }

        public override void BeforeInitialize(ICollectionPersister persister, int anticipatedSize)
        {
            set = (ISet<T>)persister.CollectionType.Instantiate(anticipatedSize);
        }

        /// <summary>
        /// Initializes this PersistentSet from the cached values.
        /// </summary>
        /// <param name="persister">The CollectionPersister to use to reassemble the PersistentSet.</param>
        /// <param name="disassembled">The disassembled PersistentSet.</param>
        /// <param name="owner">The owner object.</param>
        public override void InitializeFromCache(ICollectionPersister persister, object disassembled, object owner)
        {
            var array = (object[])disassembled;
            int size = array.Length;
            BeforeInitialize(persister, size);
            for (int i = 0; i < size; i++)
            {
                var element = (T)persister.ElementType.Assemble(array[i], Session, owner);
                if (element != null)
                {
                    set.Add(element);
                }
            }
            SetInitialized();
        }

        public override string ToString()
        {
            Read();
            return StringHelper.CollectionToString(set.ToArray());
        }

        public override object ReadFrom(IDataReader rs, ICollectionPersister role, ICollectionAliases descriptor, object owner)
        {
            var element = (T)role.ReadElement(rs, owner, descriptor.SuffixedElementAliases, Session);
            if (element != null)
            {
                tempList.Add(element);
            }
            return element;
        }

        /// <summary>
        /// Set up the temporary List that will be used in the EndRead() 
        /// to fully create the set.
        /// </summary>
        public override void BeginRead()
        {
            base.BeginRead();
            tempList = new List<T>();
        }

        /// <summary>
        /// Takes the contents stored in the temporary list created during <c>BeginRead()</c>
        /// that was populated during <c>ReadFrom()</c> and write it to the underlying 
        /// PersistentSet.
        /// </summary>
        public override bool EndRead(ICollectionPersister persister)
        {
            foreach (T item in tempList)
            {
                set.Add(item);
            }
            tempList = null;
            SetInitialized();
            return true;
        }

        public override IEnumerable Entries(ICollectionPersister persister)
        {
            return set;
        }

        public override object Disassemble(ICollectionPersister persister)
        {
            var result = new object[set.Count];
            int i = 0;

            foreach (object obj in set)
            {
                result[i++] = persister.ElementType.Disassemble(obj, Session, null);
            }
            return result;
        }

        public override IEnumerable GetDeletes(ICollectionPersister persister, bool indexIsFormula)
        {
            IType elementType = persister.ElementType;
            var sn = (ISetSnapshot<T>)GetSnapshot();
            var deletes = new List<T>(((ICollection<T>)sn).Count);

            deletes.AddRange(sn.Where(obj => !set.Contains(obj)));

            deletes.AddRange(from obj in set
                             let oldValue = sn[obj]
                             where oldValue != null && elementType.IsDirty(obj, oldValue, Session)
                             select oldValue);

            return deletes;
        }

        public override bool NeedsInserting(object entry, int i, IType elemType)
        {
            var sn = (ISetSnapshot<T>)GetSnapshot();
            object oldKey = sn[(T)entry];
            // note that it might be better to iterate the snapshot but this is safe,
            // assuming the user implements equals() properly, as required by the PersistentSet
            // contract!
            return oldKey == null || elemType.IsDirty(oldKey, entry, Session);
        }

        public override bool NeedsUpdating(object entry, int i, IType elemType)
        {
            return false;
        }

        public override object GetIndex(object entry, int i, ICollectionPersister persister)
        {
            throw new NotSupportedException("Sets don't have indexes");
        }

        public override object GetElement(object entry)
        {
            return entry;
        }

        public override object GetSnapshotElement(object entry, int i)
        {
            throw new NotSupportedException("Sets don't support updating by element");
        }

        public new void Read()
        {
            base.Read();
        }

        public override bool Equals(object other)
        {
            var that = other as ISet<T>;
            if (that == null)
            {
                return false;
            }
            Read();
            return set.SequenceEqual(that);
        }

        public override int GetHashCode()
        {
            Read();
            return set.GetHashCode();
        }

        public override bool EntryExists(object entry, int i)
        {
            return true;
        }

        public override bool IsWrapper(object collection)
        {
            return set == collection;
        }

        public void CopyTo(Array array, int index)
        {
            // NH : we really need to initialize the set ?
            Read();
            Array.Copy(set.ToArray(), 0, array, index, Count);
        }

        #region Nested type: ISetSnapshot

        private interface ISetSnapshot<Y> : ICollection<Y>, ICollection
        {
            Y this[Y element] { get; }
        }

        #endregion

        #region Nested type: SetSnapShot

        [Serializable]
        private class SetSnapShot<V> : ISetSnapshot<V>
        {
            private readonly List<V> elements;

            private SetSnapShot()
            {
                elements = new List<V>();
            }

            public SetSnapShot(int capacity)
            {
                elements = new List<V>(capacity);
            }

            public SetSnapShot(IEnumerable<V> collection)
            {
                elements = new List<V>(collection);
            }

            #region ISetSnapshot<V> Members

            public IEnumerator<V> GetEnumerator()
            {
                return elements.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Add(V item)
            {
                elements.Add(item);
            }

            public void Clear()
            {
                throw new InvalidOperationException();
            }

            public bool Contains(V item)
            {
                return elements.Contains(item);
            }

            public void CopyTo(V[] array, int arrayIndex)
            {
                elements.CopyTo(array, arrayIndex);
            }

            public bool Remove(V item)
            {
                throw new InvalidOperationException();
            }

            public void CopyTo(Array array, int index)
            {
                ((ICollection)elements).CopyTo(array, index);
            }

            int ICollection.Count
            {
                get { return elements.Count; }
            }

            public object SyncRoot
            {
                get { return ((ICollection)elements).SyncRoot; }
            }

            public bool IsSynchronized
            {
                get { return ((ICollection)elements).IsSynchronized; }
            }

            int ICollection<V>.Count
            {
                get { return elements.Count; }
            }

            public bool IsReadOnly
            {
                get { return ((ICollection<T>)elements).IsReadOnly; }
            }

            public V this[V element]
            {
                get
                {
                    int idx = elements.IndexOf(element);
                    if (idx >= 0)
                    {
                        return elements[idx];
                    }
                    return default(V);
                }
            }

            #endregion
        }

        #endregion
    }

    /// <summary>
    /// A <see cref="IModelInspector"/> which allows customization of conditions with explicitly declared members.
    /// </summary>
    public class HyperionModelInspector : IModelInspector, IModelExplicitDeclarationsHolder
    {
        private class MixinDeclaredModel : AbstractExplicitlyDeclaredModel
        {
            private readonly IModelInspector inspector;

            public MixinDeclaredModel(IModelInspector inspector)
            {
                this.inspector = inspector;
            }

            public override bool IsComponent(System.Type type)
            {
                return Components.Contains(type);
            }

            public override bool IsRootEntity(System.Type entityType)
            {
                return inspector.IsRootEntity(entityType);
            }

            public bool IsEntity(System.Type type)
            {
                return RootEntities.Contains(type) || type.GetBaseTypes().Any(t => RootEntities.Contains(t)) || HasDelayedEntityRegistration(type);
            }

            public bool IsTablePerClass(System.Type type)
            {
                ExecuteDelayedTypeRegistration(type);
                return IsMappedForTablePerClassEntities(type);
            }

            public bool IsTablePerClassSplit(System.Type type, object splitGroupId, MemberInfo member)
            {
                return Equals(splitGroupId, GetSplitGroupFor(member));
            }

            public bool IsTablePerClassHierarchy(System.Type type)
            {
                ExecuteDelayedTypeRegistration(type);
                return IsMappedForTablePerClassHierarchyEntities(type);
            }

            public bool IsTablePerConcreteClass(System.Type type)
            {
                ExecuteDelayedTypeRegistration(type);
                return IsMappedForTablePerConcreteClassEntities(type);
            }

            public bool IsOneToOne(MemberInfo member)
            {
                return OneToOneRelations.Contains(member);
            }

            public bool IsManyToOne(MemberInfo member)
            {
                if (typeof(AggregateRootId).IsAssignableFrom(member.GetPropertyOrFieldType()))
                    return false;
                else
                    return ManyToOneRelations.Contains(member);
            }

            public bool IsManyToMany(MemberInfo member)
            {
                return ManyToManyRelations.Contains(member);
            }

            public bool IsOneToMany(MemberInfo member)
            {
                return OneToManyRelations.Contains(member);
            }

            public bool IsManyToAny(MemberInfo member)
            {
                return ManyToAnyRelations.Contains(member);
            }

            public bool IsAny(MemberInfo member)
            {
                return Any.Contains(member);
            }

            public bool IsPersistentId(MemberInfo member)
            {
                return Poids.Contains(member);
            }

            public bool IsMemberOfComposedId(MemberInfo member)
            {
                return ComposedIds.Contains(member);
            }

            public bool IsVersion(MemberInfo member)
            {
                return VersionProperties.Contains(member);
            }

            public bool IsMemberOfNaturalId(MemberInfo member)
            {
                return NaturalIds.Contains(member);
            }

            public bool IsPersistentProperty(MemberInfo member)
            {
                if (typeof(AggregateRootId).IsAssignableFrom(member.GetPropertyOrFieldType()))
                    return true;
                else
                    return PersistentMembers.Contains(member);
            }

            public bool IsSet(MemberInfo role)
            {
                return Sets.Contains(role);
            }

            public bool IsBag(MemberInfo role)
            {
                return Bags.Contains(role);
            }

            public bool IsIdBag(MemberInfo role)
            {
                return IdBags.Contains(role);
            }

            public bool IsList(MemberInfo role)
            {
                return Lists.Contains(role);
            }

            public bool IsArray(MemberInfo role)
            {
                return Arrays.Contains(role);
            }

            public bool IsDictionary(MemberInfo role)
            {
                return Dictionaries.Contains(role);
            }

            public bool IsProperty(MemberInfo member)
            {
                if (typeof(AggregateRootId).IsAssignableFrom(member.GetPropertyOrFieldType()))
                    return true;
                return Properties.Contains(member);
            }

            public bool IsDynamicComponent(MemberInfo member)
            {
                return DynamicComponents.Contains(member);
            }

            public IEnumerable<string> GetPropertiesSplits(System.Type type)
            {
                return GetSplitGroupsFor(type);
            }
        }

        private readonly MixinDeclaredModel declaredModel;

        private Func<System.Type, bool, bool> isEntity = (t, declared) => declared;
        private Func<System.Type, bool, bool> isRootEntity;
        private Func<System.Type, bool, bool> isTablePerClass;
        private Func<SplitDefinition, bool, bool> isTablePerClassSplit = (sd, declared) => declared;
        private Func<System.Type, bool, bool> isTablePerClassHierarchy = (t, declared) => declared;
        private Func<System.Type, bool, bool> isTablePerConcreteClass = (t, declared) => declared;
        private Func<System.Type, IEnumerable<string>, IEnumerable<string>> splitsForType = (t, declared) => declared;
        private Func<System.Type, bool, bool> isComponent;

        private Func<MemberInfo, bool, bool> isPersistentId;
        private Func<MemberInfo, bool, bool> isPersistentProperty;
        private Func<MemberInfo, bool, bool> isVersion = (m, declared) => declared;

        private Func<MemberInfo, bool, bool> isProperty = (m, declared) => declared;
        private Func<MemberInfo, bool, bool> isDynamicComponent = (m, declared) => declared;
        private Func<MemberInfo, bool, bool> isAny = (m, declared) => declared;
        private Func<MemberInfo, bool, bool> isManyToMany = (m, declared) => declared;
        private Func<MemberInfo, bool, bool> isManyToAny = (m, declared) => declared;
        private Func<MemberInfo, bool, bool> isManyToOne;
        private Func<MemberInfo, bool, bool> isMemberOfNaturalId = (m, declared) => declared;
        private Func<MemberInfo, bool, bool> isOneToMany;
        private Func<MemberInfo, bool, bool> isOneToOne = (m, declared) => declared;

        private Func<MemberInfo, bool, bool> isSet;
        private Func<MemberInfo, bool, bool> isArray;
        private Func<MemberInfo, bool, bool> isBag;
        private Func<MemberInfo, bool, bool> isDictionary;
        private Func<MemberInfo, bool, bool> isIdBag = (m, declared) => declared;
        private Func<MemberInfo, bool, bool> isList = (m, declared) => declared;

        public HyperionModelInspector()
        {
            isEntity = (t, declared) => declared || MatchEntity(t);
            isRootEntity = (t, declared) => declared || MatchRootEntity(t);
            isTablePerClass = (t, declared) => declared || MatchTablePerClass(t);
            isPersistentId = (m, declared) => declared || MatchPoIdPattern(m);
            isComponent = (t, declared) => declared || MatchComponentPattern(t);
            isPersistentProperty = (m, declared) => declared || ((m is PropertyInfo) && MatchNoReadOnlyPropertyPattern(m));
            isSet = (m, declared) => declared || MatchCollection(m, MatchSetMember);
            isArray = (m, declared) => declared || MatchCollection(m, MatchArrayMember);
            isBag = (m, declared) => declared || MatchCollection(m, MatchBagMember);
            isDictionary = (m, declared) => declared || MatchCollection(m, MatchDictionaryMember);
            isManyToOne = (m, declared) => declared || MatchManyToOne(m);
            isOneToMany = (m, declared) => declared || MatchOneToMany(m);
            declaredModel = new MixinDeclaredModel(this);
        }

        private bool MatchRootEntity(System.Type type)
        {
            return type.IsClass && typeof(object).Equals(type.BaseType) && ((IModelInspector)this).IsEntity(type);
        }

        private bool MatchTablePerClass(System.Type type)
        {
            return !declaredModel.IsTablePerClassHierarchy(type) && !declaredModel.IsTablePerConcreteClass(type);
        }

        private bool MatchOneToMany(MemberInfo memberInfo)
        {
            var modelInspector = (IModelInspector)this;
            System.Type from = memberInfo.ReflectedType;
            System.Type to = memberInfo.GetPropertyOrFieldType().DetermineCollectionElementOrDictionaryValueType();
            if (to == null)
            {
                // no generic collection or simple property
                return false;
            }
            bool areEntities = modelInspector.IsEntity(from) && modelInspector.IsEntity(to);
            bool isFromComponentToEntity = modelInspector.IsComponent(from) && modelInspector.IsEntity(to);
            return !declaredModel.IsManyToMany(memberInfo) && (areEntities || isFromComponentToEntity);
        }

        private bool MatchManyToOne(MemberInfo memberInfo)
        {
            var modelInspector = (IModelInspector)this;
            System.Type from = memberInfo.ReflectedType;
            System.Type to = memberInfo.GetPropertyOrFieldType();

            bool areEntities = modelInspector.IsEntity(from) && modelInspector.IsEntity(to);
            bool isFromComponentToEntity = modelInspector.IsComponent(from) && modelInspector.IsEntity(to);
            return isFromComponentToEntity || (areEntities && !modelInspector.IsOneToOne(memberInfo));
        }

        protected bool MatchArrayMember(MemberInfo subject)
        {
            System.Type memberType = subject.GetPropertyOrFieldType();
            return memberType.IsArray && memberType.GetElementType() != typeof(byte);
        }

        protected bool MatchDictionaryMember(MemberInfo subject)
        {
            System.Type memberType = subject.GetPropertyOrFieldType();
            if (typeof(System.Collections.IDictionary).IsAssignableFrom(memberType))
            {
                return true;
            }
            if (memberType.IsGenericType)
            {
                return memberType.GetGenericInterfaceTypeDefinitions().Contains(typeof(IDictionary<,>));
            }
            return false;
        }

        protected bool MatchBagMember(MemberInfo subject)
        {
            System.Type memberType = subject.GetPropertyOrFieldType();
            return typeof(System.Collections.IEnumerable).IsAssignableFrom(memberType) && !(memberType == typeof(string) || memberType == typeof(byte[]));
        }

        protected bool MatchCollection(MemberInfo subject, Predicate<MemberInfo> specificCollectionPredicate)
        {
            const BindingFlags defaultBinding = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

            if (specificCollectionPredicate(subject)) return true;
            var pi = subject as PropertyInfo;
            if (pi != null)
            {
                var fieldInfo = (from ps in PropertyToField.DefaultStrategies.Values
                                 let fi = subject.DeclaringType.GetField(ps.GetFieldName(pi.Name), defaultBinding)
                                 where fi != null
                                 select fi).FirstOrDefault();

                if (fieldInfo != null)
                {
                    return specificCollectionPredicate(fieldInfo);
                }
            }
            return false;
        }

        protected bool MatchSetMember(MemberInfo subject)
        {
            var memberType = subject.GetPropertyOrFieldType();

            if (memberType.IsGenericType)
            {
                return memberType.GetGenericInterfaceTypeDefinitions().Contains(typeof(ISet<>));
            }
            return false;
        }

        protected bool MatchNoReadOnlyPropertyPattern(MemberInfo subject)
        {
            var isReadOnlyProperty = IsReadOnlyProperty(subject);
            return !isReadOnlyProperty;
        }

        protected bool IsReadOnlyProperty(MemberInfo subject)
        {
            const BindingFlags defaultBinding = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

            var property = subject as PropertyInfo;
            if (property == null)
            {
                return false;
            }
            if (CanReadCantWriteInsideType(property) || CanReadCantWriteInBaseType(property))
            {
                return !PropertyToField.DefaultStrategies.Values.Any(s => subject.DeclaringType.GetField(s.GetFieldName(property.Name), defaultBinding) != null) || IsAutoproperty(property);
            }
            return false;
        }

        protected bool IsAutoproperty(PropertyInfo property)
        {
            return property.ReflectedType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                                                                                     | BindingFlags.DeclaredOnly).Any(pi => pi.Name == string.Concat("<", property.Name, ">k__BackingField"));
        }

        protected bool CanReadCantWriteInsideType(PropertyInfo property)
        {
            return !property.CanWrite && property.CanRead && property.DeclaringType == property.ReflectedType;
        }

        protected bool CanReadCantWriteInBaseType(PropertyInfo property)
        {
            if (property.DeclaringType == property.ReflectedType)
            {
                return false;
            }
            var rfprop = property.DeclaringType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                                                                                     | BindingFlags.DeclaredOnly).SingleOrDefault(pi => pi.Name == property.Name);
            return rfprop != null && !rfprop.CanWrite && rfprop.CanRead;
        }

        protected bool MatchPoIdPattern(MemberInfo subject)
        {
            var name = subject.Name;
            return name.Equals("id", StringComparison.InvariantCultureIgnoreCase)
                         || name.Equals("poid", StringComparison.InvariantCultureIgnoreCase)
                         || name.Equals("oid", StringComparison.InvariantCultureIgnoreCase)
                         || (name.StartsWith(subject.DeclaringType.Name) && name.Equals(subject.DeclaringType.Name + "id", StringComparison.InvariantCultureIgnoreCase));
        }

        protected bool MatchComponentPattern(System.Type subject)
        {
            const BindingFlags flattenHierarchyMembers =
                BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            if (declaredModel.IsEntity(subject))
            {
                return false;
            }
            var modelInspector = (IModelInspector)this;
            return !subject.IsEnum && (subject.Namespace == null || !subject.Namespace.StartsWith("System")) /* hack */
                            && !modelInspector.IsEntity(subject)
                            && !subject.GetProperties(flattenHierarchyMembers).Cast<MemberInfo>().Concat(
                        subject.GetFields(flattenHierarchyMembers)).Any(m => modelInspector.IsPersistentId(m));
        }

        protected bool MatchEntity(System.Type subject)
        {
            const BindingFlags flattenHierarchyMembers =
                BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            if (declaredModel.Components.Contains(subject))
            {
                return false;
            }
            var modelInspector = (IModelInspector)this;
            return subject.IsClass &&
                   subject.GetProperties(flattenHierarchyMembers).Cast<MemberInfo>().Concat(subject.GetFields(flattenHierarchyMembers)).Any(m => modelInspector.IsPersistentId(m));
        }

        #region IModelExplicitDeclarationsHolder Members

        IEnumerable<System.Type> IModelExplicitDeclarationsHolder.RootEntities
        {
            get { return declaredModel.RootEntities; }
        }

        IEnumerable<System.Type> IModelExplicitDeclarationsHolder.Components
        {
            get { return declaredModel.Components; }
        }

        IEnumerable<System.Type> IModelExplicitDeclarationsHolder.TablePerClassEntities
        {
            get { return declaredModel.TablePerClassEntities; }
        }

        IEnumerable<System.Type> IModelExplicitDeclarationsHolder.TablePerClassHierarchyEntities
        {
            get { return declaredModel.TablePerClassHierarchyEntities; }
        }

        IEnumerable<System.Type> IModelExplicitDeclarationsHolder.TablePerConcreteClassEntities
        {
            get { return declaredModel.TablePerConcreteClassEntities; }
        }

        IEnumerable<MemberInfo> IModelExplicitDeclarationsHolder.OneToOneRelations
        {
            get { return declaredModel.OneToOneRelations; }
        }

        IEnumerable<MemberInfo> IModelExplicitDeclarationsHolder.ManyToOneRelations
        {
            get { return declaredModel.ManyToManyRelations; }
        }

        IEnumerable<MemberInfo> IModelExplicitDeclarationsHolder.ManyToManyRelations
        {
            get { return declaredModel.ManyToManyRelations; }
        }

        IEnumerable<MemberInfo> IModelExplicitDeclarationsHolder.OneToManyRelations
        {
            get { return declaredModel.OneToManyRelations; }
        }

        IEnumerable<MemberInfo> IModelExplicitDeclarationsHolder.ManyToAnyRelations
        {
            get { return declaredModel.ManyToAnyRelations; }
        }

        IEnumerable<MemberInfo> IModelExplicitDeclarationsHolder.Any
        {
            get { return declaredModel.Any; }
        }

        IEnumerable<MemberInfo> IModelExplicitDeclarationsHolder.Poids
        {
            get { return declaredModel.Poids; }
        }

        IEnumerable<MemberInfo> IModelExplicitDeclarationsHolder.VersionProperties
        {
            get { return declaredModel.VersionProperties; }
        }

        IEnumerable<MemberInfo> IModelExplicitDeclarationsHolder.NaturalIds
        {
            get { return declaredModel.NaturalIds; }
        }

        IEnumerable<MemberInfo> IModelExplicitDeclarationsHolder.Sets
        {
            get { return declaredModel.Sets; }
        }

        IEnumerable<MemberInfo> IModelExplicitDeclarationsHolder.Bags
        {
            get { return declaredModel.Bags; }
        }

        IEnumerable<MemberInfo> IModelExplicitDeclarationsHolder.IdBags
        {
            get { return declaredModel.IdBags; }
        }

        IEnumerable<MemberInfo> IModelExplicitDeclarationsHolder.Lists
        {
            get { return declaredModel.Lists; }
        }

        IEnumerable<MemberInfo> IModelExplicitDeclarationsHolder.Arrays
        {
            get { return declaredModel.Arrays; }
        }

        IEnumerable<MemberInfo> IModelExplicitDeclarationsHolder.Dictionaries
        {
            get { return declaredModel.Dictionaries; }
        }

        IEnumerable<MemberInfo> IModelExplicitDeclarationsHolder.Properties
        {
            get { return declaredModel.Properties; }
        }

        IEnumerable<MemberInfo> IModelExplicitDeclarationsHolder.PersistentMembers
        {
            get { return declaredModel.PersistentMembers; }
        }

        IEnumerable<SplitDefinition> IModelExplicitDeclarationsHolder.SplitDefinitions
        {
            get { return declaredModel.SplitDefinitions; }
        }

        IEnumerable<MemberInfo> IModelExplicitDeclarationsHolder.DynamicComponents
        {
            get { return declaredModel.DynamicComponents; }
        }

        IEnumerable<string> IModelExplicitDeclarationsHolder.GetSplitGroupsFor(System.Type type)
        {
            return declaredModel.GetSplitGroupsFor(type);
        }

        string IModelExplicitDeclarationsHolder.GetSplitGroupFor(MemberInfo member)
        {
            return declaredModel.GetSplitGroupFor(member);
        }

        void IModelExplicitDeclarationsHolder.AddAsRootEntity(System.Type type)
        {
            declaredModel.AddAsRootEntity(type);
        }

        void IModelExplicitDeclarationsHolder.AddAsComponent(System.Type type)
        {
            declaredModel.AddAsComponent(type);
        }

        void IModelExplicitDeclarationsHolder.AddAsTablePerClassEntity(System.Type type)
        {
            declaredModel.AddAsTablePerClassEntity(type);
        }

        void IModelExplicitDeclarationsHolder.AddAsTablePerClassHierarchyEntity(System.Type type)
        {
            declaredModel.AddAsTablePerClassHierarchyEntity(type);
        }

        void IModelExplicitDeclarationsHolder.AddAsTablePerConcreteClassEntity(System.Type type)
        {
            declaredModel.AddAsTablePerConcreteClassEntity(type);
        }

        void IModelExplicitDeclarationsHolder.AddAsOneToOneRelation(MemberInfo member)
        {
            declaredModel.AddAsOneToOneRelation(member);
        }

        void IModelExplicitDeclarationsHolder.AddAsManyToOneRelation(MemberInfo member)
        {
            declaredModel.AddAsManyToOneRelation(member);
        }

        void IModelExplicitDeclarationsHolder.AddAsManyToManyRelation(MemberInfo member)
        {
            declaredModel.AddAsManyToManyRelation(member);
        }

        void IModelExplicitDeclarationsHolder.AddAsOneToManyRelation(MemberInfo member)
        {
            declaredModel.AddAsOneToManyRelation(member);
        }

        void IModelExplicitDeclarationsHolder.AddAsManyToAnyRelation(MemberInfo member)
        {
            declaredModel.AddAsManyToAnyRelation(member);
        }

        void IModelExplicitDeclarationsHolder.AddAsAny(MemberInfo member)
        {
            declaredModel.AddAsAny(member);
        }

        void IModelExplicitDeclarationsHolder.AddAsPoid(MemberInfo member)
        {
            declaredModel.AddAsPoid(member);
        }

        void IModelExplicitDeclarationsHolder.AddAsVersionProperty(MemberInfo member)
        {
            declaredModel.AddAsVersionProperty(member);
        }

        void IModelExplicitDeclarationsHolder.AddAsNaturalId(MemberInfo member)
        {
            declaredModel.AddAsNaturalId(member);
        }

        void IModelExplicitDeclarationsHolder.AddAsSet(MemberInfo member)
        {
            declaredModel.AddAsSet(member);
        }

        void IModelExplicitDeclarationsHolder.AddAsBag(MemberInfo member)
        {
            declaredModel.AddAsBag(member);
        }

        void IModelExplicitDeclarationsHolder.AddAsIdBag(MemberInfo member)
        {
            declaredModel.AddAsIdBag(member);
        }

        void IModelExplicitDeclarationsHolder.AddAsList(MemberInfo member)
        {
            declaredModel.AddAsList(member);
        }

        void IModelExplicitDeclarationsHolder.AddAsArray(MemberInfo member)
        {
            declaredModel.AddAsArray(member);
        }

        void IModelExplicitDeclarationsHolder.AddAsMap(MemberInfo member)
        {
            declaredModel.AddAsMap(member);
        }

        void IModelExplicitDeclarationsHolder.AddAsProperty(MemberInfo member)
        {
            declaredModel.AddAsProperty(member);
        }

        void IModelExplicitDeclarationsHolder.AddAsPersistentMember(MemberInfo member)
        {
            declaredModel.AddAsPersistentMember(member);
        }

        void IModelExplicitDeclarationsHolder.AddAsPropertySplit(SplitDefinition definition)
        {
            declaredModel.AddAsPropertySplit(definition);
        }

        void IModelExplicitDeclarationsHolder.AddAsDynamicComponent(MemberInfo member, System.Type componentTemplate)
        {
            declaredModel.AddAsDynamicComponent(member, componentTemplate);
        }

        IEnumerable<MemberInfo> IModelExplicitDeclarationsHolder.ComposedIds
        {
            get { return declaredModel.ComposedIds; }
        }

        void IModelExplicitDeclarationsHolder.AddAsPartOfComposedId(MemberInfo member)
        {
            declaredModel.AddAsPartOfComposedId(member);
        }

        #endregion

        #region Implementation of IModelInspector

        bool IModelInspector.IsRootEntity(System.Type type)
        {
            bool declaredResult = declaredModel.RootEntities.Contains(type);
            return isRootEntity(type, declaredResult);
        }

        bool IModelInspector.IsComponent(System.Type type)
        {
            bool declaredResult = declaredModel.Components.Contains(type);
            return isComponent(type, declaredResult);
        }

        bool IModelInspector.IsEntity(System.Type type)
        {
            bool declaredResult = declaredModel.IsEntity(type);
            return isEntity(type, declaredResult);
        }

        bool IModelInspector.IsTablePerClass(System.Type type)
        {
            bool declaredResult = declaredModel.IsTablePerClass(type);
            return isTablePerClass(type, declaredResult);
        }

        bool IModelInspector.IsTablePerClassSplit(System.Type type, object splitGroupId, MemberInfo member)
        {
            bool declaredResult = declaredModel.IsTablePerClassSplit(type, splitGroupId, member);
            return isTablePerClassSplit(new SplitDefinition(type, splitGroupId.ToString(), member), declaredResult);
        }

        bool IModelInspector.IsTablePerClassHierarchy(System.Type type)
        {
            bool declaredResult = declaredModel.IsTablePerClassHierarchy(type);
            return isTablePerClassHierarchy(type, declaredResult);
        }

        bool IModelInspector.IsTablePerConcreteClass(System.Type type)
        {
            bool declaredResult = declaredModel.IsTablePerConcreteClass(type);
            return isTablePerConcreteClass(type, declaredResult);
        }

        bool IModelInspector.IsOneToOne(MemberInfo member)
        {
            bool declaredResult = DeclaredPolymorphicMatch(member, m => declaredModel.IsOneToOne(m));
            return isOneToOne(member, declaredResult);
        }

        bool IModelInspector.IsManyToOne(MemberInfo member)
        {
            bool declaredResult = DeclaredPolymorphicMatch(member, m => declaredModel.IsManyToOne(m));
            return isManyToOne(member, declaredResult);
        }

        bool IModelInspector.IsManyToMany(MemberInfo member)
        {
            bool declaredResult = DeclaredPolymorphicMatch(member, m => declaredModel.IsManyToMany(m));
            return isManyToMany(member, declaredResult);
        }

        bool IModelInspector.IsOneToMany(MemberInfo member)
        {
            bool declaredResult = DeclaredPolymorphicMatch(member, m => declaredModel.IsOneToMany(m));
            return isOneToMany(member, declaredResult);
        }

        bool IModelInspector.IsManyToAny(MemberInfo member)
        {
            bool declaredResult = DeclaredPolymorphicMatch(member, m => declaredModel.IsManyToAny(m));
            return isManyToAny(member, declaredResult);
        }

        bool IModelInspector.IsAny(MemberInfo member)
        {
            bool declaredResult = DeclaredPolymorphicMatch(member, m => declaredModel.IsAny(m));
            return isAny(member, declaredResult);
        }

        bool IModelInspector.IsPersistentId(MemberInfo member)
        {
            bool declaredResult = DeclaredPolymorphicMatch(member, m => declaredModel.IsPersistentId(m));
            return isPersistentId(member, declaredResult);
        }

        bool IModelInspector.IsMemberOfComposedId(MemberInfo member)
        {
            return declaredModel.IsMemberOfComposedId(member);
        }

        bool IModelInspector.IsVersion(MemberInfo member)
        {
            bool declaredResult = DeclaredPolymorphicMatch(member, m => declaredModel.IsVersion(m));
            return isVersion(member, declaredResult);
        }

        bool IModelInspector.IsMemberOfNaturalId(MemberInfo member)
        {
            bool declaredResult = DeclaredPolymorphicMatch(member, m => declaredModel.IsMemberOfNaturalId(m));
            return isMemberOfNaturalId(member, declaredResult);
        }

        bool IModelInspector.IsPersistentProperty(MemberInfo member)
        {
            bool declaredResult = DeclaredPolymorphicMatch(member, m => declaredModel.IsPersistentProperty(m));
            return isPersistentProperty(member, declaredResult);
        }

        bool IModelInspector.IsSet(MemberInfo role)
        {
            bool declaredResult = DeclaredPolymorphicMatch(role, m => declaredModel.IsSet(m));
            return isSet(role, declaredResult);
        }

        bool IModelInspector.IsBag(MemberInfo role)
        {
            bool declaredResult = DeclaredPolymorphicMatch(role, m => declaredModel.IsBag(m));
            return isBag(role, declaredResult);
        }

        bool IModelInspector.IsIdBag(MemberInfo role)
        {
            bool declaredResult = DeclaredPolymorphicMatch(role, m => declaredModel.IsIdBag(m));
            return isIdBag(role, declaredResult);
        }

        bool IModelInspector.IsList(MemberInfo role)
        {
            bool declaredResult = DeclaredPolymorphicMatch(role, m => declaredModel.IsList(m));
            return isList(role, declaredResult);
        }

        bool IModelInspector.IsArray(MemberInfo role)
        {
            bool declaredResult = DeclaredPolymorphicMatch(role, m => declaredModel.IsArray(m));
            return isArray(role, declaredResult);
        }

        bool IModelInspector.IsDictionary(MemberInfo role)
        {
            bool declaredResult = DeclaredPolymorphicMatch(role, m => declaredModel.IsDictionary(m));
            return isDictionary(role, declaredResult);
        }

        bool IModelInspector.IsProperty(MemberInfo member)
        {
            bool declaredResult = DeclaredPolymorphicMatch(member, m => declaredModel.IsProperty(m));
            return isProperty(member, declaredResult);
        }

        bool IModelInspector.IsDynamicComponent(MemberInfo member)
        {
            bool declaredResult = DeclaredPolymorphicMatch(member, m => declaredModel.IsDynamicComponent(m));
            return isDynamicComponent(member, declaredResult);
        }

        System.Type IModelInspector.GetDynamicComponentTemplate(MemberInfo member)
        {
            return declaredModel.GetDynamicComponentTemplate(member);
        }
        System.Type IModelExplicitDeclarationsHolder.GetDynamicComponentTemplate(MemberInfo member)
        {
            return declaredModel.GetDynamicComponentTemplate(member);
        }

        IEnumerable<string> IModelInspector.GetPropertiesSplits(System.Type type)
        {
            IEnumerable<string> declaredResult = declaredModel.GetPropertiesSplits(type);
            return splitsForType(type, declaredResult);
        }

        #endregion

        protected virtual bool DeclaredPolymorphicMatch(MemberInfo member, Func<MemberInfo, bool> declaredMatch)
        {
            return declaredMatch(member)
                         || member.GetMemberFromDeclaringClasses().Any(declaredMatch)
                         || member.GetPropertyFromInterfaces().Any(declaredMatch);
        }

        public void IsRootEntity(Func<System.Type, bool, bool> match)
        {
            if (match == null)
            {
                return;
            }
            isRootEntity = match;
        }

        public void IsComponent(Func<System.Type, bool, bool> match)
        {
            if (match == null)
            {
                return;
            }
            isComponent = match;
        }

        public void IsEntity(Func<System.Type, bool, bool> match)
        {
            if (match == null)
            {
                return;
            }
            isEntity = match;
        }

        public void IsTablePerClass(Func<System.Type, bool, bool> match)
        {
            if (match == null)
            {
                return;
            }
            isTablePerClass = match;
        }

        public void IsTablePerClassHierarchy(Func<System.Type, bool, bool> match)
        {
            if (match == null)
            {
                return;
            }
            isTablePerClassHierarchy = match;
        }

        public void IsTablePerConcreteClass(Func<System.Type, bool, bool> match)
        {
            if (match == null)
            {
                return;
            }
            isTablePerConcreteClass = match;
        }

        public void IsOneToOne(Func<MemberInfo, bool, bool> match)
        {
            if (match == null)
            {
                return;
            }
            isOneToOne = match;
        }

        public void IsManyToOne(Func<MemberInfo, bool, bool> match)
        {
            if (match == null)
            {
                return;
            }
            isManyToOne = match;
        }

        public void IsManyToMany(Func<MemberInfo, bool, bool> match)
        {
            if (match == null)
            {
                return;
            }
            isManyToMany = match;
        }

        public void IsOneToMany(Func<MemberInfo, bool, bool> match)
        {
            if (match == null)
            {
                return;
            }
            isOneToMany = match;
        }

        public void IsManyToAny(Func<MemberInfo, bool, bool> match)
        {
            if (match == null)
            {
                return;
            }
            isManyToAny = match;
        }

        public void IsAny(Func<MemberInfo, bool, bool> match)
        {
            if (match == null)
            {
                return;
            }
            isAny = match;
        }

        public void IsPersistentId(Func<MemberInfo, bool, bool> match)
        {
            if (match == null)
            {
                return;
            }
            isPersistentId = match;
        }

        public void IsVersion(Func<MemberInfo, bool, bool> match)
        {
            if (match == null)
            {
                return;
            }
            isVersion = match;
        }

        public void IsMemberOfNaturalId(Func<MemberInfo, bool, bool> match)
        {
            if (match == null)
            {
                return;
            }
            isMemberOfNaturalId = match;
        }

        public void IsPersistentProperty(Func<MemberInfo, bool, bool> match)
        {
            if (match == null)
            {
                return;
            }
            isPersistentProperty = match;
        }

        public void IsSet(Func<MemberInfo, bool, bool> match)
        {
            if (match == null)
            {
                return;
            }
            isSet = match;
        }

        public void IsBag(Func<MemberInfo, bool, bool> match)
        {
            if (match == null)
            {
                return;
            }
            isBag = match;
        }

        public void IsIdBag(Func<MemberInfo, bool, bool> match)
        {
            if (match == null)
            {
                return;
            }
            isIdBag = match;
        }

        public void IsList(Func<MemberInfo, bool, bool> match)
        {
            if (match == null)
            {
                return;
            }
            isList = match;
        }

        public void IsArray(Func<MemberInfo, bool, bool> match)
        {
            if (match == null)
            {
                return;
            }
            isArray = match;
        }

        public void IsDictionary(Func<MemberInfo, bool, bool> match)
        {
            if (match == null)
            {
                return;
            }
            isDictionary = match;
        }

        public void IsProperty(Func<MemberInfo, bool, bool> match)
        {
            if (match == null)
            {
                return;
            }
            isProperty = match;
        }

        public void SplitsFor(Func<System.Type, IEnumerable<string>, IEnumerable<string>> getPropertiesSplitsId)
        {
            if (getPropertiesSplitsId == null)
            {
                return;
            }
            splitsForType = getPropertiesSplitsId;
        }

        public void IsTablePerClassSplit(Func<SplitDefinition, bool, bool> match)
        {
            if (match == null)
            {
                return;
            }
            isTablePerClassSplit = match;
        }

        public void IsDynamicComponent(Func<MemberInfo, bool, bool> match)
        {
            if (match == null)
            {
                return;
            }
            isDynamicComponent = match;
        }
    }
}
