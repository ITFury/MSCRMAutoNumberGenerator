using Microsoft.Xrm.Sdk;
using OP.MSCRM.AutoNumberGenerator.Plugins.Constants;
using OP.MSCRM.AutoNumberGenerator.Plugins.Entities;
using OP.MSCRM.AutoNumberGenerator.Plugins.ExceptionHandling;
using OP.MSCRM.AutoNumberGenerator.Plugins.Extensions;
using OP.MSCRM.AutoNumberGenerator.Plugins.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace OP.MSCRM.AutoNumberGenerator.Plugins.Managers
{
    /// <summary>
    /// Auto-Number generation manager to generate, create, update, delete and rearrange Auto-Number in display entity
    /// </summary>
    public class GenerateAutoNumberManager
    {
        /// <summary>
        /// Create GenerateAutoNumberManager instance only once
        /// </summary>
        public static GenerateAutoNumberManager GenerateAutoNumber = new GenerateAutoNumberManager();

        /// <summary>
        /// Get ConfigureAutoNumberManager instance
        /// </summary>
        public ConfigureAutoNumberManager ConfigureAutoNumberManager
        {
            get { return ConfigureAutoNumberManager.ConfigureAutoNumber; }
        }

        public string GenerateAutoNumberPluginName = typeof(GenerateAutoNumberPlugin).Name;

        /// <summary>
        /// Get Number without suffix, prefix and specific symbols from generated Auto-Number
        /// </summary>
        /// <param name="autoNumber">Generated Auto-Number</param>
        /// <param name="prefix">Auto-Number prefix from config</param>
        /// <param name="suffix">Auto-Number suffix from config</param>
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
        /// <param name="length">Length of Auto-Nomber base. For example, 4 will be displayed as 0000</param>
        /// <param name="sequence">Current Auto-Number sequence</param>
        /// <param name="prefix">Auto-Number prefix</param>
        /// <param name="suffix">Auto-Number suffix</param>
        /// <returns>Generated Auto-Number</returns>
        public string Generate(int? length, int? sequence, string prefix, string suffix)
        {
            var autoNumber = string.Empty;

            if (sequence != null)
            {
                //Auto-Number length formatting
                StringBuilder numberLengthBuilder = new StringBuilder();
                if (length != null && length.HasValue)
                {
                    numberLengthBuilder.Insert(0, "0", length.Value);
                }
                var numberLength = numberLengthBuilder.Length > 0 ? numberLengthBuilder.ToString() : "0";

                //Generate Auto-Number
                var numberFormat = $"{{0:{numberLength}}}";
                autoNumber = string.Format(numberFormat, sequence);

                if (!string.IsNullOrWhiteSpace(prefix))
                {
                    autoNumber = $"{prefix}{autoNumber}";
                }

                if (!string.IsNullOrWhiteSpace(suffix))
                {
                    autoNumber = $"{autoNumber}{suffix}";
                }
            }

            if (string.IsNullOrWhiteSpace(autoNumber))
            {
                throw new PluginException(string.Format(ExceptionMessages.Name, GenerateAutoNumberPluginName),
                ExceptionMessages.CantGenerateMsg);
            }

            return autoNumber;
        }

       
        /// <summary>
        /// Create Auto-Number in current entity
        /// </summary>
        /// <param name="orgService">Organization Service</param>
        /// <param name="currentEntity">Current entity schema name where display Auto-Number</param>
        internal void Create(IOrganizationService orgService, Entity currentEntity)
        {
            string configFieldSchemaName = string.Empty;
            string entityName = currentEntity.LogicalName;
            try
            {
                List<Op_AutoNumberConfig> configs = orgService.RetrieveAutoNumberConfig(entityName);
                if (configs != null)
                {
                    foreach (var config in configs)
                    {
                        //Lock Auto-Number Configuration entity 
                        Op_AutoNumberConfig lockedConfig = ConfigureAutoNumberManager.Lock(orgService, config.Id, config.LogicalName);

                        //Get values from Auto-Number Configuration entity
                        configFieldSchemaName = lockedConfig.FieldSchemaName;
                        int? configIncrement = lockedConfig.Increment;
                        int? configSequence = lockedConfig.Sequence;
                        int? configLength = lockedConfig.Length;
                        string configPrefix = lockedConfig.Prefix;
                        string configSuffix = lockedConfig.Suffix;
                        string configPreview = lockedConfig.Preview;

                        //Get sequence from Auto-Number 
                        List<Entity> displayEntities = orgService
                            .RetrieveAutoNumberDisplayEntitiesExceptCurrent(currentEntity, configFieldSchemaName);

                        //Validate have Auto-Number display entity default Auto-Number
                        bool containsDefaultAutoNumber = displayEntities != null
                            && displayEntities
                                .Any(e => e.Contains(configFieldSchemaName) && e.GetAttributeValue<string>(configFieldSchemaName) == configPreview);

                        //Get current sequence
                        //If display entity do not have default Auto-Number, set sequence to default value, otherwise add increment
                        int? currentSequence = !containsDefaultAutoNumber
                                 ? configSequence
                                 : configSequence + configIncrement;

                        var generatedAutoNumber = Generate(configLength, currentSequence, configPrefix, configSuffix);

                        currentEntity[configFieldSchemaName] = generatedAutoNumber;

                        //Update Auto-Number Configuration entity Sequence attribute
                        ConfigureAutoNumberManager.Update(orgService, lockedConfig, currentSequence);
                    }
                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                var fieldDisplayName = orgService.RetrieveAttributeDisplayName(entityName, configFieldSchemaName);

                throw new PluginException(string.Format(ExceptionMessages.Name, GenerateAutoNumberPluginName),
                    $"{string.Format(ExceptionMessages.CantSetMsg, fieldDisplayName)} {ex.Message}");
            }
            catch(PluginException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new PluginException(string.Format(ExceptionMessages.Name, GenerateAutoNumberPluginName), ex.Message);
            }
        }


        /// <summary>
        /// Don't allow update Auto-Number manually
        /// </summary>
        /// <param name="orgService">Organization Service</param>
        /// <param name="currentEntity">Current entity where display Auto-Number</param>
        internal void Update(IOrganizationService orgService, Entity currentEntity)
        {
            var entityName = currentEntity.LogicalName;
            List<Op_AutoNumberConfig> configs = orgService.RetrieveAutoNumberConfig(entityName);
            if (configs != null)
            {
                foreach (var config in configs)
                {
                    //Lock Auto-Number Configuration entity 
                    Op_AutoNumberConfig lockedConfig = ConfigureAutoNumberManager.Lock(orgService, config.Id, config.LogicalName);

                    //Get Auto-Number display Field Name value from Auto-Number Configuration entity
                    string configFieldSchemaName = lockedConfig.FieldSchemaName;

                    //If current entity Auto-Number was updated manually, show error message
                    if (currentEntity.Contains(configFieldSchemaName))
                    {
                        var fieldDisplayName = orgService.RetrieveAttributeDisplayName(entityName, configFieldSchemaName);

                        throw new PluginException(string.Format(ExceptionMessages.Name, GenerateAutoNumberPluginName),
                            string.Format(ExceptionMessages.CantUpdateManuallyMsg, fieldDisplayName));
                     }
                }
            }
        }


        /// <summary>
        /// Update Auto-Number automatically in display entity 
        /// </summary>
        /// <param name="orgService">Organization Service</param>
        /// <param name="displayEntity">Entity where display Auto-Number</param>
        /// <param name="fieldSchemaName">Attribute schema name where display Auto-Number</param>
        /// <param name="autoNumber">Generated Auto-Number</param>
        public void UpdateAutoNumber(IOrganizationService orgService, Entity displayEntity, string fieldSchemaName, string autoNumber)
        {
            try
            {
                Entity displayEntityUpdate = new Entity(displayEntity.LogicalName)
                {
                    Id = displayEntity.Id
                };
                displayEntityUpdate.Attributes[fieldSchemaName] = autoNumber;

                orgService.Update(displayEntityUpdate);
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new PluginException(string.Format(ExceptionMessages.Name, GenerateAutoNumberPluginName), 
                    $"{ExceptionMessages.CantUpdateMsg} {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new PluginException(string.Format(ExceptionMessages.Name, GenerateAutoNumberPluginName), ex.Message);
            }
        }


        /// <summary>
        /// Rearrange Auto-Number in display entity in case if number rearrange is allowed in Auto-Number Configuration entity 
        /// </summary>
        /// <param name="orgService">Organization Service</param>
        /// <param name="displayEntityToRearrange">Entity where display and rearrange Auto-Number</param>
        /// <param name="config">Auto-Number Configuration entity</param>
        /// <param name="currentEntitySequence">Current entity Auto-Number sequence</param>
        private void Rearrange(IOrganizationService orgService, Entity displayEntityToRearrange, Op_AutoNumberConfig config, int currentEntitySequence)
        {
            string configFieldSchemaName = config.FieldSchemaName;
            string autoNumberToRearrange = displayEntityToRearrange.GetAttributeValue<string>(configFieldSchemaName);

            string configPrefix = config.Prefix;
            string configSuffix = config.Suffix;
            int autoNumberToRearrangeSeq = GetNumber(autoNumberToRearrange, configPrefix, configSuffix);
            
            //Get Sequence value from Auto-Number Configuration entity
            int configSequence = config.Sequence.Value;

            if (currentEntitySequence <= autoNumberToRearrangeSeq
                && autoNumberToRearrangeSeq <= configSequence)
            {
                if (displayEntityToRearrange.Attributes.ContainsKey(configFieldSchemaName))
                {
                    int? configIncrement = config.Increment;
                    var rearrangedSequence = autoNumberToRearrangeSeq - configIncrement;
                    int? configLength = config.Length;
                    
                    //Generate Auto-Number
                    var generatedAutoNumber = Generate(configLength, rearrangedSequence, configPrefix, configSuffix);

                    //Update rearranged Auto-Number in display entity
                    UpdateAutoNumber(orgService, displayEntityToRearrange, configFieldSchemaName, generatedAutoNumber);
                }
            }
        }


        /// <summary>
        /// Delete or rearrnage Auto-Number 
        /// If Rearrange Sequence After Delete value is set to Yes, then delete and rearrange Auto-Number, otherwise only delete 
        /// </summary>
        /// <param name="orgService">Organization Service</param>
        /// <param name="currentEntity">Current entity</param>
        internal void Delete(IOrganizationService orgService, Entity currentEntity)
        {
            try
            {
                var entityName = currentEntity.LogicalName;
                List<Op_AutoNumberConfig> configs = orgService.RetrieveAutoNumberConfig(entityName);
                if (configs != null)
                {
                    foreach (var config in configs)
                    {
                        //Lock Auto-Number Configuration entity 
                        Op_AutoNumberConfig lockedConfig = ConfigureAutoNumberManager.Lock(orgService, config.Id, config.LogicalName);

                        //Get values from Auto-Number Configuration entity                
                        bool configRrearrangeSequence = lockedConfig.IsRearrangeSequence;
                        int? configIncrement = lockedConfig.Increment;
                        int? configSequence = lockedConfig.Sequence;

                        //Set current Sequence
                        int? currentSequence = configSequence.Value - configIncrement;

                        //If Rearrange Sequence After Delete value is set to Yes then rearrange Auto-Number
                        if (configRrearrangeSequence)
                        {
                            //Get Auto-Number display Field Name value from Auto-Number Configuration entity
                            string configFieldSchemaName = lockedConfig.FieldSchemaName;

                            //Retrieve current Auto-Number display entity
                            Entity currentEntityFromDB = orgService.RetrieveAutoNumberDisplayEntity(entityName, currentEntity.Id, configFieldSchemaName);
                            if (currentEntityFromDB != null)
                            {
                                if (currentEntityFromDB.Attributes.ContainsKey(configFieldSchemaName))
                                {
                                    //Get Auto-Number from current entity
                                    string currentEntityAutoNumber = currentEntityFromDB.GetAttributeValue<string>(configFieldSchemaName);

                                    string confiPrefix = lockedConfig.Prefix;
                                    string configSuffix = lockedConfig.Suffix;
                                    int currentEntitySequence = GetNumber(currentEntityAutoNumber, confiPrefix, configSuffix);

                                    //If Auto-Number isn't last in current entity then rearrange Auto-Number
                                    if (configSequence.Value != currentEntitySequence)
                                    {
                                        List<Entity> displayEntitiesToRearrange = orgService.RetrieveAutoNumberDisplayEntitiesExceptCurrent
                                            (currentEntity, configFieldSchemaName);
                                        if (displayEntitiesToRearrange != null)
                                        {
                                            foreach (var displayEntityToRearrange in displayEntitiesToRearrange)
                                            {
                                                Rearrange(orgService, displayEntityToRearrange, lockedConfig, currentEntitySequence);
                                            }
                                        }
                                    }
                                    //If Auto-Number is last in current entity then reset Sequence to default value
                                    else
                                    {
                                        //Reset Sequence to default value
                                        currentSequence = lockedConfig.Start;
                                    }
                                }
                            }
                        }

                        //Update Auto-Number Configuration entity Sequence field
                        if (configSequence != lockedConfig.Start)
                        {
                            ConfigureAutoNumberManager.Update(orgService, lockedConfig, currentSequence);
                        }
                    }
                }
            }
            catch (PluginException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new PluginException(string.Format(ExceptionMessages.Name, GenerateAutoNumberPluginName), ex.Message);
            }
        }

    }
}
