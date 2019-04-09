using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using OP.MSCRM.AutoNumberGenerator.Plugins.Constants;
using OP.MSCRM.AutoNumberGenerator.Plugins.Entities;
using OP.MSCRM.AutoNumberGenerator.Plugins.ExceptionHandling;
using OP.MSCRM.AutoNumberGenerator.Plugins.Plugins;
using System;
using System.ServiceModel;

namespace OP.MSCRM.AutoNumberGenerator.Plugins.Managers
{
    /// <summary>
    /// Auto-Number configuration manager to lock and update Auto-Number Configuration entity
    /// </summary>
    public class ConfigureAutoNumberManager
    {
        /// <summary>
        /// Create ConfigureAutoNumberManager instance only once
        /// </summary>
        public static ConfigureAutoNumberManager ConfigureAutoNumber = new ConfigureAutoNumberManager();

        public string GenerateAutoNumberPluginName = typeof(GenerateAutoNumberPlugin).Name;

        /// <summary>
        /// Lock Auto-Number Configuration entity to avoid Auto-Number generation duplicates
        /// </summary>
        /// <param name="orgService">Organization Service</param>
        /// <param name="configId">Auto-Number Configuration entity Id</param>
        /// <param name="configLogicalName">Auto-Number Configuration entity schema name</param>
        /// <returns>Locked Auto-Number Configuration entity</returns>
        public Op_AutoNumberConfig Lock(IOrganizationService orgService, Guid configId, string configLogicalName)
        {
            //Lock Auto-Number Configuration entity 
            Op_AutoNumberConfig configLocker = new Op_AutoNumberConfig
            {
                Id = configId,
                LogicalName = configLogicalName,
                IsLocked = true
            };
            //Lock all transactions
            orgService.Update(configLocker);

            //Retrive locked Auto-Number Configuration to prevent duplicates generation
            Op_AutoNumberConfig lockedConfig = orgService
                .Retrieve(configLocker.LogicalName, configLocker.Id, new ColumnSet(true))
                .ToEntity<Op_AutoNumberConfig>();

            if (lockedConfig == null)
            {
                throw new PluginException(string.Format(ExceptionMessages.Name, GenerateAutoNumberPluginName),
                    ExceptionMessages.ConfigCantFindMsg);
            }

            return lockedConfig;
        }


        /// <summary>
        /// Update Auto-Number Configuration entity Sequence attribute
        /// </summary>
        /// <param name="orgService">Organization Service</param>
        /// <param name="config">Auto-Number Configuration entity</param>
        /// <param name="currentSequence">Auto-Number Configuration entity Sequence value to update</param>
        public void Update(IOrganizationService orgService, Op_AutoNumberConfig config, int? currentSequence)
        {
            try
            {
                Op_AutoNumberConfig autoNumberConfigUpdate = new Op_AutoNumberConfig
                {
                    Id = config.Id,
                    LogicalName = config.LogicalName,
                    Sequence = currentSequence
                };
                orgService.Update(autoNumberConfigUpdate);
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new PluginException(string.Format(ExceptionMessages.Name, GenerateAutoNumberPluginName),
                    $"{ExceptionMessages.ConfigCantUpdateMsg} {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new PluginException(string.Format(ExceptionMessages.Name, GenerateAutoNumberPluginName),
                    ex.Message);
            }
        }
    }
}
