using System;
using System.Collections.Generic;
using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;
using NHibernate.Cfg;
using NHibernate.Mapping.ByCode;
using NHibernate.Tool.hbm2ddl;
using NMSD.Cronus.Persistence.MSSQL;

namespace NMSD.Cronus.Sample.Player
{
    public static class NHibernateExtensions
    {
        /// <summary>
        /// Adds all Hyperion mappings to a NHibernate configuration.
        /// </summary>
        /// <param name="nhConf">The NHib configuration instance.</param>
        /// <returns>Returns the NHib configuration instance with Hyperion mappings.</returns>
        public static Configuration AddAutoMappings(this Configuration nhConf, IEnumerable<Type> types, Action<ConventionModelMapper> modelMapper = null)
        {
            var englishPluralizationService = PluralizationService.CreateService(new CultureInfo("en-US"));
            var mapper = new ConventionModelMapper();

            //var baseEntityType = typeof(IEventHandler);
            //mapper.IsEntity((t, declared) => baseEntityType.IsAssignableFrom(t) && baseEntityType != t && !t.IsInterface);
            //mapper.IsRootEntity((t, declared) => baseEntityType.Equals(t.BaseType));

            mapper.BeforeMapClass += (insp, prop, map) => map.Table(englishPluralizationService.Pluralize(prop.Name));

            mapper.BeforeMapClass += (i, t, cm) => cm.Id(map =>
            {
                map.Column(t.Name + "Id");

                if (t.GetProperty("Id").PropertyType == typeof(Int32))
                    map.Generator(Generators.Identity);
                else
                    map.Generator(Generators.Assigned);
            });

            mapper.BeforeMapManyToOne += (insp, prop, map) => map.Column(prop.LocalMember.GetPropertyOrFieldType().Name + "Id");
            mapper.BeforeMapManyToOne += (insp, prop, map) => map.Cascade(Cascade.None);

            mapper.BeforeMapBag += (insp, prop, map) => map.Key(km => km.Column(prop.GetContainerEntity(insp).Name + "Id"));
            mapper.BeforeMapBag += (insp, prop, map) =>
            {
                map.Cascade(Cascade.All.Include(Cascade.DeleteOrphans));
                map.Inverse(true);
            };


            mapper.BeforeMapSet += (insp, prop, map) => map.Key(km => km.Column(prop.GetContainerEntity(insp).Name + "Id"));
            mapper.BeforeMapSet += (insp, prop, map) =>
            {
                map.Cascade(Cascade.All.Include(Cascade.DeleteOrphans));
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



}
