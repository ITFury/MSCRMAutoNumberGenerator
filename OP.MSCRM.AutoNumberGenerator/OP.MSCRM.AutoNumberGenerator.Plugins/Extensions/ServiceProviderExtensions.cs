using Microsoft.Xrm.Sdk;
using System;

namespace OP.MSCRM.AutoNumberGenerator.Plugins.Extensions
{
    public static class ServiceProviderExtensions
    {
        /// <summary>
        /// Get Crm plugin execution context
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static IPluginExecutionContext GetPluginExecutionContext(this IServiceProvider serviceProvider)
        {
            // Obtain the execution context service from the service provider.
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(serviceProvider.ToString());
            }
            return (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
        }


        /// <summary>
        /// Get Crm organization service
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static IOrganizationService GetOrganizationService(this IServiceProvider serviceProvider)
        {
            // Obtain the execution context service from the service provider.
            var pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            // Obtain the Organization Service factory service from the service provider
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            // Use the factory to generate the Organization Service.
            return factory.CreateOrganizationService(pluginExecutionContext.UserId);
        }

    }
}
