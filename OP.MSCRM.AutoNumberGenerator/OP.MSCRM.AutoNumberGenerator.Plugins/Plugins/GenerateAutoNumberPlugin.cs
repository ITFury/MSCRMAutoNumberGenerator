using Microsoft.Xrm.Sdk;
using OP.MSCRM.AutoNumberGenerator.Plugins.Constants;
using OP.MSCRM.AutoNumberGenerator.Plugins.ExceptionHandling;
using OP.MSCRM.AutoNumberGenerator.Plugins.Extensions;
using OP.MSCRM.AutoNumberGenerator.Plugins.Managers;
using System;

namespace OP.MSCRM.AutoNumberGenerator.Plugins.Plugins
{
    /// <summary>
    /// Auto-Number generation plug-in
    /// </summary>
    public class GenerateAutoNumberPlugin : IPlugin
    {

        /// <summary>
        /// Get GenerateAutoNumberManager instance
        /// </summary>
        public GenerateAutoNumberManager GenerateAutoNumberManager
        {
            get { return GenerateAutoNumberManager.GenerateAutoNumber; }
        }

        
        /// <summary>
        /// Execute GenerateAutoNumberPlugin
        /// </summary>
        /// <param name="serviceProvider">Service Provider</param>
        /// Add GenerateAutoNumberPlugin Steps in PluginRegistrationTool. All default except:
        /// ---Step 1:---
        /// -------------
        /// Message: Create
        /// Primary Entity: <entity name where display Auto Number>
        /// Event Pipeline Stage of Execution: Pre-operation
        /// Execution Mode: Synchronous
        /// Deployment: Server
        /// ---Step 2:---
        /// -------------
        /// Message: Update
        /// Primary Entity: <entity name where display Auto Number>
        /// Filtering Attributes: <field name where display Auto Number>
        /// Event Pipeline Stage of Execution: Pre-operation
        /// Execution Mode: Synchronous
        /// Deployment: Server
        /// ---Step 3:---
        /// -------------
        /// Message: Delete
        /// Primary Entity: <entity name where display Auto Number>
        /// Event Pipeline Stage of Execution: Pre-operation
        /// Execution Mode: Synchronous
        /// Deployment: Server
        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                IPluginExecutionContext context = serviceProvider.GetPluginExecutionContext();
                IOrganizationService orgService = serviceProvider.GetOrganizationService();

                //Plugin step message
                switch (context.MessageName)
                {
                    //Create
                    case ExecutionContextMessageName.Create:

                        Entity entityToCreate = context.GetTarget();
                        GenerateAutoNumberManager.Create(orgService, entityToCreate);

                        break;

                    //Update
                    case ExecutionContextMessageName.Update:

                        //If depth > 1 (plug-in triggered from other step) do not allow execute update
                        if (context.Depth > 1) return;

                        Entity entityToUpdate = context.GetTarget();
                        GenerateAutoNumberManager.Update(orgService, entityToUpdate);

                        break;

                    //Delete
                    case ExecutionContextMessageName.Delete:

                        EntityReference deletedEntityMonikier = context.GetReferenceTarget();
                        Entity entityToDelete = new Entity(deletedEntityMonikier.LogicalName, deletedEntityMonikier.Id);

                        GenerateAutoNumberManager.Delete(orgService, entityToDelete);

                        break;


                    default:
                        break;
                }
            }            
            catch (PluginException ex)
            {
                throw new InvalidPluginExecutionException($"{ex.Name} {ex.Description}");
            }
        }
    }
}
