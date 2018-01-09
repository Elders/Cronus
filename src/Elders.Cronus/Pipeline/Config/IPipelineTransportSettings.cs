using Elders.Cronus.Pipeline.Transport.Strategy;

namespace Elders.Cronus.Pipeline.Config
{
    public interface IPipelineTransportSettings : ISettingsBuilder
    {
        IEndpointNameConvention EndpointNameConvention { get; set; }
        IPipelineNameConvention PipelineNameConvention { get; set; }
    }

    public static class PipelineTransportSettingsExtensions
    {
        /// <summary>
        /// This is actually per Application Name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static T WithEndpointPerBoundedContext<T>(this T self) where T : IPipelineTransportSettings
        {
            self.PipelineNameConvention = new PipelinePerApplicationNameConvention();
            self.EndpointNameConvention = new EndpointPerConsumerNameConvention(self.PipelineNameConvention);
            return self;
        }

        public static T WithEndpointPerBoundedContextNamespace<T>(this T self) where T : IPipelineTransportSettings
        {
            self.PipelineNameConvention = new PipelinePerBoundedContextNamespaceConvention();
            self.EndpointNameConvention = new EndpointPerConsumerNameConvention(self.PipelineNameConvention);
            return self;
        }

        /// <summary>
        /// This is actually per Application Name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static T WithEndpointPerHandler<T>(this T self) where T : IPipelineTransportSettings
        {
            self.PipelineNameConvention = new PipelinePerApplicationNameConvention();
            self.EndpointNameConvention = new EndpointPerHandlerNameConvention(self.PipelineNameConvention);
            return self;
        }
    }
}
