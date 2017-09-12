using Microsoft.Xrm.Sdk;
using OP.MSCRM.AutoNumberGenerator.Plugins.Constants;
using OP.MSCRM.AutoNumberGenerator.Plugins.ExceptionHandling;
using OP.MSCRM.AutoNumberGenerator.Plugins.Extensions;
using OP.MSCRM.AutoNumberGenerator.Plugins.Managers;
using System;
using System.Threading;

namespace OP.MSCRM.AutoNumberGenerator.Plugins.Plugins
{
    /// <summary>
    /// Auto Number generation plug-in
    /// </summary>
    public class GenerateAutoNumberPlugin : IPlugin
    {
        /// <summary>
        /// Get object instance to lock thread.
        /// </summary>
        public object ThisLock
        {
            get { return GenerateAutoNumberManager.ThisLock; }
        }

        /// <summary>
        /// Get GenerateAutoNumberManager instance
        /// </summary>
        public GenerateAutoNumberManager AutoNumberManager
        {
            get { return GenerateAutoNumberManager.AutoNumberManager; }
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

                // Lock duplicate Auto Number generation
                lock (ThisLock)
                {
                    //Plugin step message
                    switch (context.MessageName)
                    {
                        //Create
                        case ExecutionContextMessageName.Create:
   
                            //Method is executed sequentially — about 100 milliseconds apart
                            Thread.Sleep(100);

                            Entity entityToCreate = context.GetTarget();
                            AutoNumberManager.CreateAutoNumber(orgService, entityToCreate);

                            break;

                        //Update
                        case ExecutionContextMessageName.Update:

                            Entity entityToUpdate = context.GetTarget();
                            AutoNumberManager.UpdateAutoNumber(orgService, entityToUpdate);
                      
                            break;

                        //Delete
                        case ExecutionContextMessageName.Delete:
   
                            //Method is executed sequentially — about 100 milliseconds apart
                            Thread.Sleep(100);

                            EntityReference deletedEntityMonikier = context.GetReferenceTarget();
                            Entity entityToDelete = new Entity(deletedEntityMonikier.LogicalName, deletedEntityMonikier.Id);

                            AutoNumberManager.OverrideAutoNumber(orgService, entityToDelete);

                            break;


                        default:
                            break;
                    }
                }
            }
            catch (PluginException ex)
            {
                throw new InvalidPluginExecutionException($"{ex.Name} {ex.Description}");
            }
        }

    }
}
