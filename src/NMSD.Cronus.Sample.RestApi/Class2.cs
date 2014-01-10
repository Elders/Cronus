using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;

namespace NMSD.Cronus.Sample.RestApi
{
    public class AreaHttpControllerSelector : DefaultHttpControllerSelector
    {
        private const string AreaRouteVariableName = "area";

        private readonly HttpConfiguration _configuration;
        private readonly Lazy<ConcurrentDictionary<string, Type>> _apiControllerTypes;

        public AreaHttpControllerSelector(HttpConfiguration configuration)
            : base(configuration)
        {
            _configuration = configuration;
            _apiControllerTypes = new Lazy<ConcurrentDictionary<string, Type>>(GetControllerTypes);
        }

        public override HttpControllerDescriptor SelectController(HttpRequestMessage request)
        {
            return this.GetApiController(request);
        }

        private static string GetAreaName(HttpRequestMessage request)
        {
            var data = request.GetRouteData();
            if (data.Route.DataTokens == null)
            {
                return null;
            }
            else
            {
                object areaName;
                return data.Route.DataTokens.TryGetValue(AreaRouteVariableName, out areaName) ? areaName.ToString() : null;
            }
        }

        private static ConcurrentDictionary<string, Type> GetControllerTypes()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var types = assemblies
                .SelectMany(a => a
                    .GetTypes().Where(t =>
                        !t.IsAbstract &&
                        t.Name.EndsWith(ControllerSuffix, StringComparison.OrdinalIgnoreCase) &&
                        typeof(IHttpController).IsAssignableFrom(t)))
                .ToDictionary(t => t.FullName, t => t);

            return new ConcurrentDictionary<string, Type>(types);
        }

        private HttpControllerDescriptor GetApiController(HttpRequestMessage request)
        {
            var areaName = GetAreaName(request);
            var controllerName = GetControllerName(request);
            var type = GetControllerType(areaName, controllerName);

            return new HttpControllerDescriptor(_configuration, controllerName, type);
        }

        private Type GetControllerType(string areaName, string controllerName)
        {
            var query = _apiControllerTypes.Value.AsEnumerable();

            if (string.IsNullOrEmpty(areaName))
            {
                query = query.WithoutAreaName();
            }
            else
            {
                query = query.ByAreaName(areaName);
            }

            return query
                .ByControllerName(controllerName)
                .Select(x => x.Value)
                .Single();
        }
    }

    public static class ControllerTypeSpecifications
    {
        public static IEnumerable<KeyValuePair<string, Type>> ByAreaName(this IEnumerable<KeyValuePair<string, Type>> query, string areaName)
        {
            var areaNameToFind = string.Format(CultureInfo.InvariantCulture, ".{0}.", areaName);

            return query.Where(x => x.Key.IndexOf(areaNameToFind, StringComparison.OrdinalIgnoreCase) != -1);
        }

        public static IEnumerable<KeyValuePair<string, Type>> WithoutAreaName(this IEnumerable<KeyValuePair<string, Type>> query)
        {
            return query.Where(x => x.Key.IndexOf(".areas.", StringComparison.OrdinalIgnoreCase) == -1);
        }

        public static IEnumerable<KeyValuePair<string, Type>> ByControllerName(this IEnumerable<KeyValuePair<string, Type>> query, string controllerName)
        {
            var controllerNameToFind = string.Format(CultureInfo.InvariantCulture, ".{0}{1}", controllerName, AreaHttpControllerSelector.ControllerSuffix);

            return query.Where(x => x.Key.EndsWith(controllerNameToFind, StringComparison.OrdinalIgnoreCase));
        }
    }
}