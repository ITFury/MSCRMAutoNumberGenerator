using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using OP.MSCRM.AutoNumberGenerator.Plugins.Entities;
using OP.MSCRM.AutoNumberGenerator.Plugins.ExceptionHandling;
using OP.MSCRM.AutoNumberGenerator.Plugins.Extensions;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;


namespace OP.MSCRM.AutoNumberGenerator.Plugins.Managers
{
    /// <summary>
    /// Auto Number generation manager
    /// </summary>
    public class GenerateAutoNumberManager
    {
        /// <summary>
        /// Object to lock
        /// </summary>
        public static object ThisLock = new object();

        /// <summary>
        /// Create Auto Number manager instance only once
        /// </summary>
        public static GenerateAutoNumberManager AutoNumberManager = new GenerateAutoNumberManager();

        /// <summary>
        /// Auto Number Sequence
        /// </summary>
        private int? Sequence { get; set; }

        /// <summary>
        /// Check is Auto Number updated from CRM UI or automatically from code. If from CRM UI - true otherwise - false.
        /// </summary>
        private bool IsUIUpdate { get; set; } = true;


        /// <summary>
        /// Generate Auto Number in presented format from Auto Number Config entity
        /// </summary>
        /// <param name="format">Auto Number Format</param>
        /// <param name="formatNumberLenght">Auto Number Format Number Lenght</param>
        /// <param name="sequence">Auto Number Sequence</param>
        /// <returns>Generated Auto Number</returns>
        public string GenerateAutoNumber(string format, int? formatNumberLenght, int? sequence)
        {
            try
            {
                //Auto Number Format Number Lenght formatting
                StringBuilder formatNumberLenghtBuilder = new StringBuilder();
                if (formatNumberLenght != null && formatNumberLenght.HasValue)
                {
                    formatNumberLenghtBuilder.Insert(0, "0", formatNumberLenght.Value);
                }
                var formatNumberCount = formatNumberLenghtBuilder.Length > 0 ? formatNumberLenghtBuilder.ToString() : null;

                //Auto Number Format formatting
                string[] formatNewSplitted;
                string formatWithLength = string.Empty;
                string autoNumberFormatted = string.IsNullOrEmpty(format) ? "{0}" : format;

                if (formatNumberCount != null)
                {
                    formatNewSplitted = autoNumberFormatted.Split('}');
                    formatWithLength = $"{formatNewSplitted[0]}:{formatNumberCount}";

                    if (string.IsNullOrEmpty(formatNewSplitted[1]))
                    {
                        //formatted without suffix
                        autoNumberFormatted = $"{formatWithLength}}}";
                    }
                    else
                    {
                        //formatted with suffix
                        autoNumberFormatted = $"{formatWithLength}}}{formatNewSplitted[1]}";
                    }
                }

                //Auto Number Sequence formatting
                var sequenceNew = sequence == null ? 1 : sequence.Value + 1;
                //Set Sequence property
                Sequence = sequenceNew;

                //Generated Auto Number
                string generatedAutoNumber = string.Format(autoNumberFormatted, sequenceNew);

                return generatedAutoNumber;
            }
            catch (Exception ex)
            {
                throw new PluginException("An error occurred in the GenerateAutoNumberPlugin plug-in.","Can't generate Auto Number.");
            }
        }


        /// <summary>
        /// Update Auto Number Config entity Sequence field
        /// </summary>
        /// <param name="orgService">Organization Service</param>
        /// <param name="autoNumberConfig">op_auto_number_config entity</param>
        private void UpdateAutoNumberConfig(IOrganizationService orgService, op_auto_number_config autoNumberConfig)
        {
            try
            {
                autoNumberConfig.op_sequence = Sequence == 0 ? null : Sequence;
                orgService.Update(autoNumberConfig);
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new PluginException("An error occurred in the GenerateAutoNumberPlugin plug-in.","Can't update Auto Number Config entity Sequence field.");
            }
            catch (Exception ex)
            {
                throw new PluginException("An error occurred in the GenerateAutoNumberPlugin plug-in.", $"{ex.Message}");
            }
        }


        /// <summary>
        /// Create Auto Number in presented entity
        /// </summary>
        /// <param name="orgService">Organization Service</param>
        /// <param name="currentEntity">Execution entity where display Auto Number</param>
        internal void CreateAutoNumber(IOrganizationService orgService, Entity currentEntity)
        {
            try
            {
                var entityName = currentEntity.LogicalName;

                //Retrieve entity data list from op_auto_number_config where op_entity_name equal entityName
                List<op_auto_number_config> autoNumberConfigs = orgService.RetrieveAutoNumberConfig(entityName);

                foreach (var autoNumberConfig in autoNumberConfigs)
                {
                    //Get values from op_auto_number_config entity
                    string configAutoNumberDisplayField = autoNumberConfig.op_field_name;
                    string configFormat = autoNumberConfig.op_format;
                    int? configFormatNumberLenght = autoNumberConfig.op_format_number_length;
                    int? configSeq = autoNumberConfig.op_sequence;

                    //Generate Auto Number
                    var generatedAutoNumber = GenerateAutoNumber(configFormat, configFormatNumberLenght, configSeq);

                    //Set Auto Number to presented Entity Field 
                    currentEntity[configAutoNumberDisplayField] = generatedAutoNumber;

                    //Update Auto Number Config entity Sequence
                    UpdateAutoNumberConfig(orgService, autoNumberConfig);
                    
                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new PluginException("An error occurred in the GenerateAutoNumberPlugin plug-in.","Can't create current entity Auto Number field.");
            }
            catch (Exception ex)
            {
                throw new PluginException("An error occurred in the GenerateAutoNumberPlugin plug-in.", $"{ex.Message}");
            }
        }


        /// <summary>
        /// Get Number from generated Auto Number
        /// </summary>
        /// <param name="autoNumber">Generated Auto Number</param>
        /// <returns>Number</returns>
        public int GetNumber(string autoNumber)
        {
            int number = 0;

            if (!string.IsNullOrEmpty(autoNumber))
            {
                string toNumber = Regex.Match(autoNumber, @"\d+").Value;
                if (!string.IsNullOrEmpty(toNumber))
                {
                    string trimZero = toNumber.TrimStart('0');
                    number = int.Parse(trimZero);
                }
            }

            return number;
        }


        /// <summary>
        /// Don't allow update Auto Number manually. Always set Auto Number to DB value.
        /// </summary>
        /// <param name="orgService">Organization Service</param>
        /// <param name="currentEntity">Execution entity where display Auto Number</param>
        internal void UpdateAutoNumber(IOrganizationService orgService, Entity currentEntity)
        {
            var entityName = currentEntity.LogicalName;

            //Retrieve entity data list from op_auto_number_config where op_entity_name equal entityName
            List<op_auto_number_config> autoNumberConfigs = orgService.RetrieveAutoNumberConfig(entityName);

            foreach (var autoNumberConfig in autoNumberConfigs)
            {
                //Get op_field_name value from op_auto_number_config entity
                string configAutoNumberDisplayField = autoNumberConfig.op_field_name;

                var currentEntityAutoNumber = currentEntity[configAutoNumberDisplayField];
                if (currentEntityAutoNumber != null)
                {
                    //Retrieve current entity
                    ColumnSet columnSet = new ColumnSet(configAutoNumberDisplayField);
                    Entity currentEntityFromDB = orgService.RetrieveById<Entity>(entityName, currentEntity.Id, columnSet);

                    if (currentEntityFromDB != null)
                    {
                        string autoNumberFromDB = currentEntityFromDB.GetAttributeValue<string>(configAutoNumberDisplayField);

                        if (currentEntityAutoNumber.ToString() != autoNumberFromDB && IsUIUpdate)
                        {
                            //Set Auto Number to DB value
                            currentEntity[configAutoNumberDisplayField] = autoNumberFromDB;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Update Auto Number Display Entity
        /// </summary>
        /// <param name="orgService">Organization Service</param>
        /// <param name="entityId">Entity Id that must be updated</param>
        /// <param name="entityName">Entity Name where display Auto Number</param>
        /// <param name="autoNumberAttribute">Entity Auto Number display attribute</param>
        /// <param name="autoNumber">New Auto Number that must be set</param>
        private void UpdateAutoNumber(IOrganizationService orgService, Guid entityId, string entityName, string autoNumberAttribute, string autoNumber)
        {
            try
            {
                Entity autoNumberDisplayEntity = new Entity();
                autoNumberDisplayEntity.Id = entityId;
                autoNumberDisplayEntity.LogicalName = entityName;
                //Set new Auto Number
                autoNumberDisplayEntity[autoNumberAttribute] = autoNumber;

                IsUIUpdate = false;

                orgService.Update(autoNumberDisplayEntity);

                IsUIUpdate = true;
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new PluginException("An error occurred in the GenerateAutoNumberPlugin plug-in.", "Can't update current entity Auto Number field.");
            }
            catch (Exception ex)
            {
                throw new PluginException("An error occurred in the GenerateAutoNumberPlugin plug-in.", $"{ex.Message}");
            }
        }


        /// <summary>
        /// Override Auto Number
        /// </summary>
        /// <param name="orgService">Organization Service</param>
        /// <param name="autoNumberDisplayEntity">Auto Number display entity</param>
        /// <param name="autoNumberConfig">op_auto_number_config entity</param>
        /// <param name="currentEntityAutoNumberSeq">execution entity Auto Number sequence</param>
        private void OverrideAutoNumber(IOrganizationService orgService, Entity autoNumberDisplayEntity, op_auto_number_config autoNumberConfig, int currentEntityAutoNumberSeq)
        {
            string configAutoNumberDisplayField = autoNumberConfig.op_field_name;

            string orderAutoNumber = autoNumberDisplayEntity.GetAttributeValue<string>(configAutoNumberDisplayField);
            int orderAutoNumberSeq = GetNumber(orderAutoNumber);
            //Get op_sequence value from op_auto_number_config entity
            int configSeq = autoNumberConfig.op_sequence.Value;

            if (currentEntityAutoNumberSeq <= orderAutoNumberSeq
                && orderAutoNumberSeq <= configSeq)
            {
                if (autoNumberDisplayEntity.Attributes.ContainsKey(configAutoNumberDisplayField))
                {
                    var overridenSeq = orderAutoNumberSeq - 2;

                    //Get values from op_auto_number_config entity
                    string configEntityName = autoNumberConfig.op_entity_name;
                    string configFormat = autoNumberConfig.op_format;
                    int? configFormatNumberLenght = autoNumberConfig.op_format_number_length;
                    
                    //Generate Auto Number
                    var generatedNewAutoNumber = GenerateAutoNumber(configFormat, configFormatNumberLenght, overridenSeq);

                    Guid autoNumberDisplayEntityId = autoNumberDisplayEntity.Id;

                    //Update Auto Number in order Entity 
                    UpdateAutoNumber(orgService, autoNumberDisplayEntityId, configEntityName, configAutoNumberDisplayField, generatedNewAutoNumber);
                }
            }
        }


        /// <summary>
        /// Override Auto Number if op_override_sequence is Yes
        /// </summary>
        /// <param name="orgService">Organization Service</param>
        /// <param name="currentEntity">execution entity</param>
        internal void OverrideAutoNumber(IOrganizationService orgService, Entity currentEntity)
        {
            var entityName = currentEntity.LogicalName;

            //Retrieve entity data list from op_auto_number_config where op_entity_name equal entityName
            List<op_auto_number_config> autoNumberConfigs = orgService.RetrieveAutoNumberConfig(entityName);

            if (autoNumberConfigs != null)
            {
                foreach (var autoNumberConfig in autoNumberConfigs)
                {
                    //Get Override Sequence
                    bool overrideSeq = autoNumberConfig.op_override_sequence;
                    //Get Sequence
                    int? configSeq = autoNumberConfig.op_sequence;
                    bool isConfigSeq = configSeq != null && configSeq.HasValue;

                    //If op_sequence not equal null and op_override_sequence is Yes then override Auto Number
                    if (isConfigSeq && overrideSeq)
                    {
                        //Get Field Name
                        string configAutoNumberDisplayField = autoNumberConfig.op_field_name;
                        
                        //Retrieve current entity all data
                        ColumnSet columnSet = new ColumnSet(configAutoNumberDisplayField);
                        Entity currentEntityFromDB = orgService.RetrieveById<Entity>(entityName, currentEntity.Id, columnSet);

                        if (currentEntityFromDB != null)
                        {
                            if (currentEntityFromDB.Attributes.ContainsKey(configAutoNumberDisplayField))
                            {
                                //Get Auto Number from presented entity
                                string currentEntityAutoNumber = currentEntityFromDB.GetAttributeValue<string>(configAutoNumberDisplayField);
                                int currentEntityAutoNumberSeq = GetNumber(currentEntityAutoNumber);

                                //If Auto Number isn't last in current entity then override Auto Number
                                if (configSeq.Value != currentEntityAutoNumberSeq)
                                {
                                    List<Entity> autoNumberDisplayEntities = orgService.RetrieveAutoNumberDisplayEntities(currentEntity, configAutoNumberDisplayField);

                                    if (autoNumberDisplayEntities != null)
                                    {
                                        foreach (var autoNumberDisplayEntity in autoNumberDisplayEntities)
                                        {
                                            OverrideAutoNumber(orgService, autoNumberDisplayEntity, autoNumberConfig, currentEntityAutoNumberSeq);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    //Set Sequence property
                    Sequence = configSeq.Value - 1;
                    //Update op_auto_number_config entity op_sequence field
                    UpdateAutoNumberConfig(orgService, autoNumberConfig);
                }
            }
        }


    }
}
