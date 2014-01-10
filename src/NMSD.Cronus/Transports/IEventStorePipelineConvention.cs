using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NMSD.Cronus.Commanding;

namespace NMSD.Cronus.Transports
{
    public interface IEventStorePipelineConvention
    {
        string GetPipelineName(Assembly assemblyContainingEvents);
    }
}
