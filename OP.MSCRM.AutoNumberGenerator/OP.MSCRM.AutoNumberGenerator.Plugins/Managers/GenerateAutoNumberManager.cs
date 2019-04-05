using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using OP.MSCRM.AutoNumberGenerator.Plugins.Entities;
using OP.MSCRM.AutoNumberGenerator.Plugins.ExceptionHandling;
using OP.MSCRM.AutoNumberGenerator.Plugins.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace OP.MSCRM.AutoNumberGenerator.Plugins.Managers
{
    /// <summary>
    /// Auto-Number generation manager to generate, create, update, delete and rearrange Auto-Number
    /// </summary>
    public class GenerateAutoNumberManager
    {
        /// <summary>
        /// Create Auto-Number manager instance only once
        /// </summary>
        public static GenerateAutoNumberManager AutoNumberManager = new GenerateAutoNumberManager();

        /// <summary>
        /// Auto-Number Sequence
        /// </summary>
        private int? Sequence { get; set; }

        /// <summary>
        /// Get Number without suffix, prefix and other symbols from generated Auto-Number
        /// </summary>
        /// <param name="autoNumber">Generated Auto-Number</param>
        /// <param name="prefix">Generated Auto-Number prefix</param>
        /// <param name="suffix">Generated Auto-Number suffix</param>
        /// <returns>Number</returns>
        public int GetNumber(string autoNumber, string prefix, string suffix)
        {
            int number = 0;

            if (!string.IsNullOrWhiteSpace(autoNumber))
            {
                //Remove prefix
                if (!string.IsNullOrWhiteSpace(prefix))
                {
                    int prefixLength = prefix.Length;
                    autoNumber = autoNumber.Substring(prefixLength, autoNumber.Length - prefixLength);
                }

                //Remove suffix
                if (!string.IsNullOrWhiteSpace(suffix))
                {
                    autoNumber = autoNumber.Substring(0, autoNumber.Length - suffix.Length);
                }

                if (!string.IsNullOrWhiteSpace(autoNumber))
                {
                    //Remove 0 before number
                    string trimZero = autoNumber.TrimStart('0');
                    number = int.Parse(trimZero);
                }
            }

            return number;
        }

        /// <summary>
        /// Generate Auto-Number in presented format specified in Auto-Number Configuration settings
        /// </summary>
        /// <param name="numberLength">Length of Auto-Nomber base. For example, 4 will be displayed as 0000</param>
        /// <param name="sequence">Current Auto-Number sequence</param>
        /// <param name="prefix">Auto-Number prefix</param>
        /// <param name="suffix">Auto-Number suffix</param>
        /// <returns>Generated Auto-Number</returns>
        public string GenerateAutoNumber(int? numberLength, int? sequence, string prefix, string suffix)
        {
            try
            {
                //Auto-Number lenght formatting
                StringBuilder numberLenghtBuilder = new StringBuilder();
                if (numberLength != null && numberLength.HasValue)
                {
                    numberLenghtBuilder.Insert(0, "0", numberLength.Value);
                }
                var numberCount = numberLenghtBuilder.Length > 0 ? numberLenghtBuilder.ToString() : "0";

                //Set current sequence
                Sequence = sequence;

                //Generate Auto-Number
                var numberFormat = $"{{0:{numberCount}}}";
                var autoNumber = string.Format(numberFormat, Sequence);

                if (!string.IsNullOrWhiteSpace(prefix))
                {
                    autoNumber = $"{prefix}{autoNumber}";
                }

                if (!string.IsNullOrWhiteSpace(suffix))
                {
                    autoNumber = $"{autoNumber}{suffix}";
                }

                return autoNumber;
            }
            catch (Exception ex)
            {
                throw new PluginException("An error occurred in the GenerateAutoNumberPlugin plug-in.", $"Can't generate Auto-Number. {ex.Message}");
            }
        }

        /// <summary>
        /// Lock Auto-Number Configuration entity to avoid Auto-Number generation duplicates
        /// </summary>
        /// <param name="orgService">Organization Service</param>
        /// <param name="autoNumberConfigId">Auto-Number Configuration entity Id</param>
        /// <param name="autoNumberConfigLogicalName">Auto-Number Configuration entity schema name</param>
        /// <returns>Locked Auto-Number Configuration entity</returns>
        private op_auto_number_config LockAutoNumberConfig (IOrganizationService orgService, Guid autoNumberConfigId, string autoNumberConfigLogicalName)
        {
            //Lock Auto-Number Configuration entity 
            op_auto_number_config autoNumberConfigLocker = new op_auto_number_config
            {
                Id = autoNumberConfigId,
                LogicalName = autoNumberConfigLogicalName,
                op_is_locked = true
            };
            //Lock all transactions
            orgService.Update(autoNumberConfigLocker);

            //Retrive locked Auto-Number Configuration to prevent duplicates generation
            op_auto_number_config lockedAutoNumberConfig = orgService
                .Retrieve(autoNumberConfigLocker.LogicalName, autoNumberConfigLocker.Id, new ColumnSet(true))
                .ToEntity<op_auto_number_config>();

            if(lockedAutoNumberConfig == null)
            {
                throw new PluginException("An error occurred in the GenerateAutoNumberPlugin plug-in.", "Can't find Auto-Number Configuration entity.");
            }

            return lockedAutoNumberConfig;
        }


        /// <summary>
        /// Update Auto-Number Configiguration entity Sequence attribute
        /// </summary>
        /// <param name="orgService">Organization Service</param>
        /// <param name="autoNumberConfig">Auto-Number Configiguration entity</param>
        private void UpdateAutoNumberConfig(IOrganizationService orgService, op_auto_number_config autoNumberConfig)
        {
            try
            {
                op_auto_number_config autoNumberConfigUpdate = new op_auto_number_config
                {
                    Id = autoNumberConfig.Id,
                    LogicalName = autoNumberConfig.LogicalName,
                    op_sequence = Sequence
                };
                orgService.Update(autoNumberConfigUpdate);
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new PluginException("An error occurred in the GenerateAutoNumberPlugin plug-in.",
                    $"Can't update Auto-Number Configuration entity Sequence field. {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new PluginException("An error occurred in the GenerateAutoNumberPlugin plug-in.",
                    $"{ex.Message}");
            }
        }

        /// <summary>
        /// Create Auto-Number in current entity
        /// </summary>
        /// <param name="orgService">Organization Service</param>
        /// <param name="currentEntity">Current entity schema name where display Auto-Number</param>
        internal void CreateAutoNumber(IOrganizationService orgService, Entity currentEntity)
        {
            string configAutoNumberDisplayField = string.Empty;
            string entityName = currentEntity.LogicalName;
            try
            {
                List<op_auto_number_config> autoNumberConfigs = orgService.RetrieveAutoNumberConfig(entityName);

                foreach (var autoNumberConfig in autoNumberConfigs)
                {
                    //Lock Auto-Number Configuration entity 
                    op_auto_number_config lockedAutoNumberConfig = LockAutoNumberConfig(orgService, autoNumberConfig.Id, autoNumberConfig.LogicalName);

                    //Get values from Auto-Number Configuration entity
                    configAutoNumberDisplayField = lockedAutoNumberConfig.op_field_name;
                    int? numberStep = lockedAutoNumberConfig.op_number_step;
                    int? sequence = lockedAutoNumberConfig.op_sequence;
                    int? numberLength = lockedAutoNumberConfig.op_number_length;
                    string prefix = lockedAutoNumberConfig.op_number_prefix;
                    string suffix = lockedAutoNumberConfig.op_number_suffix;
                    string preview = lockedAutoNumberConfig.op_number_preview;

                    //Get sequence from Auto-Number 
                    List<Entity> autoNumberDisplayEntities = orgService
                        .RetrieveAutoNumberDisplayEntitiesExceptCurrent(currentEntity, configAutoNumberDisplayField);

                    //Validate have Auto-Number display entity default Auto-Number
                    bool containsDefaultAutoNumber = autoNumberDisplayEntities != null
                        && autoNumberDisplayEntities
                            .Any(e => e.Contains(configAutoNumberDisplayField) && e.GetAttributeValue<string>(configAutoNumberDisplayField) == preview);

                    //Get current sequence
                    //If display entity do not have default Auto-Number, set sequence to default value, otherwise next increased by number step
                    int? newSequence = !containsDefaultAutoNumber
                             ? sequence
                             : sequence + numberStep;

                    //Generate Auto-Number
                    var generatedAutoNumber = GenerateAutoNumber(numberLength, newSequence, prefix, suffix);

                    //Set Auto-Number in current entity
                    currentEntity[configAutoNumberDisplayField] = generatedAutoNumber;

                    //Update Auto-Number Configuration entity Sequence attribute
                    UpdateAutoNumberConfig(orgService, lockedAutoNumberConfig);
                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                var autoNumberFieldDisplayName = orgService.RetrieveAttributeDisplayName(entityName, configAutoNumberDisplayField);

                throw new PluginException("An error occurred in the GenerateAutoNumberPlugin plug-in.",
                    $"Can't set Auto-Number value of {autoNumberFieldDisplayName} field in current entity. {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new PluginException("An error occurred in the GenerateAutoNumberPlugin plug-in.", $"{ex.Message}");
            }
        }


        /// <summary>
        /// Don't allow update Auto-Number manually. Always set Auto-Number to database value.
        /// </summary>
        /// <param name="orgService">Organization Service</param>
        /// <param name="currentEntity">Current entity where display Auto-Number</param>
        internal void NotUpdateAutoNumber(IOrganizationService orgService, Entity currentEntity)
        {
            var entityName = currentEntity.LogicalName;
            List<op_auto_number_config> autoNumberConfigs = orgService.RetrieveAutoNumberConfig(entityName);

            foreach (var autoNumberConfig in autoNumberConfigs)
            {
                //Lock Auto-Number Configuration entity 
                op_auto_number_config lockedAutoNumberConfig = LockAutoNumberConfig(orgService, autoNumberConfig.Id, autoNumberConfig.LogicalName);

                //Get Auto-Number display Field Name value from Auto-Number Configuration entity
                string configAutoNumberDisplayField = lockedAutoNumberConfig.op_field_name;

                //If current entity Auto-Number was updated manually, show error message
                if(currentEntity.Contains(configAutoNumberDisplayField))
                {
                    var autoNumberFieldDisplayName = orgService.RetrieveAttributeDisplayName(entityName, configAutoNumberDisplayField);

                    throw new PluginException("An error occurred in the GenerateAutoNumberPlugin plug-in.", 
                        $"Automatically generated number of {autoNumberFieldDisplayName} field can't be updated manually.");
                }
            }
        }


        /// <summary>
        /// Update Auto-Number automatically in display entity. 
        /// </summary>
        /// <param name="orgService">Organization Service</param>
        /// <param name="autoNumberDisplayEntity">Entity where display Auto-Number</param>
        /// <param name="autoNumberAttribute">Attribute schema name where display Auto-Number</param>
        /// <param name="autoNumber">Generated Auto-Number</param>
        private void UpdateAutoNumber(IOrganizationService orgService, Entity autoNumberDisplayEntity, string autoNumberAttribute, string autoNumber)
        {
            try
            {
                Entity autoNumberDisplayEntityUpdate = new Entity(autoNumberDisplayEntity.LogicalName)
                {
                    Id = autoNumberDisplayEntity.Id
                };
                //Set generated Auto-Number
                autoNumberDisplayEntityUpdate.Attributes[autoNumberAttribute] = autoNumber;

                orgService.Update(autoNumberDisplayEntityUpdate);
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new PluginException("An error occurred in the GenerateAutoNumberPlugin plug-in.", $"Can't update current entity Auto-Number field. {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new PluginException("An error occurred in the GenerateAutoNumberPlugin plug-in.", $"{ex.Message}");
            }
        }


        /// <summary>
        /// Rearrange Auto-Number in display entity in case if number rearrange is allowed in Auto-Number Configuration entity. 
        /// </summary>
        /// <param name="orgService">Organization Service</param>
        /// <param name="autoNumberDisplayEntityToRearrange">Entity where display and rearrange Auto-Number</param>
        /// <param name="autoNumberConfig">Auto-Number Configuration entity</param>
        /// <param name="currentEntityAutoNumberSeq">Current entity Auto-Number sequence</param>
        private void RearrangeAutoNumber(IOrganizationService orgService, Entity autoNumberDisplayEntityToRearrange, op_auto_number_config autoNumberConfig, int currentEntityAutoNumberSeq)
        {
            string configAutoNumberDisplayField = autoNumberConfig.op_field_name;
            string autoNumberToRearrange = autoNumberDisplayEntityToRearrange.GetAttributeValue<string>(configAutoNumberDisplayField);

            string prefix = autoNumberConfig.op_number_prefix;
            string suffix = autoNumberConfig.op_number_suffix;
            int autoNumberToRearrangeSeq = GetNumber(autoNumberToRearrange, prefix, suffix);
            
            //Get Sequence value from Auto-Number Configuration entity
            int configSeq = autoNumberConfig.op_sequence.Value;

            if (currentEntityAutoNumberSeq <= autoNumberToRearrangeSeq
                && autoNumberToRearrangeSeq <= configSeq)
            {
                if (autoNumberDisplayEntityToRearrange.Attributes.ContainsKey(configAutoNumberDisplayField))
                {
                    int? configNumberStep = autoNumberConfig.op_number_step;
                    var rearrangedSeq = autoNumberToRearrangeSeq - configNumberStep;
                    int? numberLenght = autoNumberConfig.op_number_length;
                    
                    //Generate Auto-Number
                    var generatedNewAutoNumber = GenerateAutoNumber(numberLenght, rearrangedSeq, prefix, suffix);

                    //Update rearranged Auto-Number in display entity
                    UpdateAutoNumber(orgService, autoNumberDisplayEntityToRearrange, configAutoNumberDisplayField, generatedNewAutoNumber);
                }
            }
        }


        /// <summary>
        /// Delete or rearrnage Auto-Number. 
        /// If Rearrange Sequence After Delete value is set to Yes, then delete and rearrange Auto-Number, otherwise only delete 
        /// </summary>
        /// <param name="orgService">Organization Service</param>
        /// <param name="currentEntity">Current entity</param>
        internal void DeleteAutoNumber(IOrganizationService orgService, Entity currentEntity)
        {
            var entityName = currentEntity.LogicalName;
            List<op_auto_number_config> autoNumberConfigs = orgService.RetrieveAutoNumberConfig(entityName);

            foreach (var autoNumberConfig in autoNumberConfigs)
            {
                //Lock Auto-Number Configuration entity 
                op_auto_number_config lockedAutoNumberConfig = LockAutoNumberConfig(orgService, autoNumberConfig.Id, autoNumberConfig.LogicalName);

                //Get values from Auto-Number Configuration entity                
                bool rearrangeSeq = lockedAutoNumberConfig.op_rearrange_sequence;
                int? configNumberStep = lockedAutoNumberConfig.op_number_step;
                int? configSeq = lockedAutoNumberConfig.op_sequence;

                //Set current Sequence
                Sequence = configSeq.Value - configNumberStep;

                //If Rearrange Sequence After Delete value is set to Yes then rearrange Auto-Number
                if (rearrangeSeq)
                {
                    //Get Auto-Number display Field Name value from Auto-Number Configuration entity
                    string configAutoNumberDisplayField = lockedAutoNumberConfig.op_field_name;

                    //Retrieve current Auto-Number display entity
                    Entity currentEntityFromDB = orgService.RetrieveAutoNumberDisplayEntity(entityName, currentEntity.Id, configAutoNumberDisplayField);
                    if (currentEntityFromDB != null)
                    {
                        if (currentEntityFromDB.Attributes.ContainsKey(configAutoNumberDisplayField))
                        {
                            //Get Auto-Number from current entity
                            string currentEntityAutoNumber = currentEntityFromDB.GetAttributeValue<string>(configAutoNumberDisplayField);

                            string prefix = lockedAutoNumberConfig.op_number_prefix;
                            string suffix = lockedAutoNumberConfig.op_number_suffix;
                            int currentEntityAutoNumberSeq = GetNumber(currentEntityAutoNumber, prefix,suffix);

                            //If Auto-Number isn't last in current entity then rearrange Auto-Number
                            if (configSeq.Value != currentEntityAutoNumberSeq)
                            {
                                List<Entity> autoNumberDisplayEntitiesToRearrange = orgService.RetrieveAutoNumberDisplayEntitiesExceptCurrent(currentEntity, configAutoNumberDisplayField);
                                if (autoNumberDisplayEntitiesToRearrange != null)
                                {
                                    foreach (var autoNumberDisplayEntityToRearrange in autoNumberDisplayEntitiesToRearrange)
                                    {
                                        RearrangeAutoNumber(orgService, autoNumberDisplayEntityToRearrange, lockedAutoNumberConfig, currentEntityAutoNumberSeq);
                                    }
                                }
                            }
                            //If Auto-Number is last in current entity then reset Sequence to default value
                            else
                            {
                                //Reset Sequence to default value
                                Sequence = lockedAutoNumberConfig.op_start_number;
                            }
                        }
                    }
                }

                //Update Auto-Number Configuration entity Sequence field
                if (configSeq != lockedAutoNumberConfig.op_start_number)
                {
                    UpdateAutoNumberConfig(orgService, lockedAutoNumberConfig);
                }
            }
        }
    }
}
