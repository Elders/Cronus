using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NMSD.Cronus.Core.Commanding;

namespace NMSD.Cronus.Core.Transports
{
    public interface IEventStorePipelineConvention
    {
        string GetPipelineName(Assembly assemblyContainingEvents);
    }
}
