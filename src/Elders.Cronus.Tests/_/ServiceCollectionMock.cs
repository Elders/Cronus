using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace Elders.Cronus;

public class ServiceCollectionMock : List<ServiceDescriptor>, IServiceCollection { }
