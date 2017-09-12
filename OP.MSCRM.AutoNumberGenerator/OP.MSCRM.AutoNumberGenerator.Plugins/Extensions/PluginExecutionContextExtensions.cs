using Microsoft.Xrm.Sdk;


namespace OP.MSCRM.AutoNumberGenerator.Plugins.Extensions
{
    public static class PluginExecutionContextExtensions
    {
        /// <summary>
        /// Obtain the target entity from the input parmameters.
        /// </summary>
        /// <param name="pluginExecutionContext"></param>
        /// <returns></returns>
        public static Entity GetTarget(this IPluginExecutionContext pluginExecutionContext)
        {
            // The InputParameters collection contains all the data passed in the message request.
            return pluginExecutionContext.InputParameters.Contains("Target") && pluginExecutionContext.InputParameters["Target"] is Entity
                ? (Entity)pluginExecutionContext.InputParameters["Target"]
                : null;
        }


        /// <summary>
        /// Obtain the target entity reference from the input parmameters.
        /// </summary>
        /// <param name="pluginExecutionContext"></param>
        /// <returns></returns>
        public static EntityReference GetReferenceTarget(this IPluginExecutionContext pluginExecutionContext)
        {
            return pluginExecutionContext.InputParameters.Contains("Target") && pluginExecutionContext.InputParameters["Target"] is EntityReference
                ? (EntityReference)pluginExecutionContext.InputParameters["Target"]
                : null;
        }
    }
}
