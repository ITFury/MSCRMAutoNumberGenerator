using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using OP.MSCRM.AutoNumberGenerator.Plugins.Entities;
using System;
using System.Collections.Generic;
using System.Linq;


namespace OP.MSCRM.AutoNumberGenerator.Plugins.Extensions
{
    /// <summary>
    /// Organization Service operation extensions
    /// </summary>
    public static class OrganizationServiceExtensions
    {

        /// <summary>
        /// Retrieve entity by Id
        /// </summary>
        /// <typeparam name="T">Entity</typeparam>
        /// <param name="orgService">Organization Service</param>
        /// <param name="entityName">Entity schema name</param>
        /// <param name="id">Entity Id</param>
        /// <param name="returnColumn">Column to return</param>
        /// <returns>Entity</returns>
        public static T RetrieveById<T>(this IOrganizationService orgService, string entityName, Guid id, ColumnSet returnColumn) where T : Entity
        {
            Entity entity = orgService.Retrieve(entityName.ToLower(), id, returnColumn);
            if (entity == null)
            {
                return null;
            }

            return entity.ToEntity<T>();
        }


        /// <summary>
        /// Base Retrieve entity list by parameters
        /// </summary>
        /// <typeparam name="T">Entity</typeparam>
        /// <param name="orgService">Organization Service</param>
        /// <param name="param">Search parameters</param>
        /// <param name="value">Search parameters values</param>
        /// <param name="returnColumn">Column to return</param>
        /// <param name="condition">Condition operator</param>
        /// <param name="entityName">Entity schema name to return</param>
        /// <returns>Entity list</returns>
        public static List<T> RetrieveByParamBase<T>(this IOrganizationService orgService, string[] param, string[] value, ColumnSet returnColumn, ConditionOperator[] condition, string entityName) where T : Entity
        {
            if (param.Length != value.Length)
            {
                return null;
            }

            string entityLogicalName = string.IsNullOrEmpty(entityName) ? typeof(T).Name.ToString().ToLower() : entityName;

            QueryExpression query = new QueryExpression
            {
                EntityName = entityLogicalName,
                Criteria = new FilterExpression(),
                ColumnSet = returnColumn
            };

            for (int i = 0; i < param.Length; i++)
            {
                if (condition != null)
                {
                    query.Criteria.AddCondition(param[i], condition[i], value[i]);
                }
                else
                {
                    query.Criteria.AddCondition(param[i], ConditionOperator.Equal, value[i]);
                }
            }

            EntityCollection entityCollection = orgService.RetrieveMultiple(query);

            if (entityCollection == null || entityCollection.Entities.Count == 0)
            {
                return null;
            }

            List<T> entities = entityCollection.Entities.Select(e => e.ToEntity<T>()).ToList();

            return entities;
        }


        /// <summary>
        /// Retrieve entity list by parameteres
        /// </summary>
        /// <typeparam name="T">Entity</typeparam>
        /// <param name="orgService">Organization Service</param>
        /// <param name="param">Search parameters</param>
        /// <param name="value">Search parameters values</param>
        /// <param name="returnColumn">Column to return</param>
        /// <returns>Entity list</returns>
        public static List<T> RetrieveByParam<T>(this IOrganizationService orgService, string[] param, string[] value, ColumnSet returnColumn) where T : Entity
        {
            return orgService.RetrieveByParamBase<T>(param, value, returnColumn, condition: null, entityName: null);
        }


        /// <summary>
        /// Retrieve entity list by parameteres with condition operator
        /// </summary>
        /// <typeparam name="T">Entity</typeparam>
        /// <param name="orgService">Organization Service</param>
        /// <param name="entityName">Entity schema name</param>
        /// <param name="param">Search parameters</param>
        /// <param name="value">Search parameters values</param>
        /// <param name="returnColumn">Column to return</param>
        /// <param name="condition">Condition operator</param>
        /// <returns>Entity list</returns>
        public static List<T> RetrieveByParam<T>(this IOrganizationService orgService, string entityName, string[] param, string[] value, ColumnSet returnColumn, ConditionOperator[] condition) where T : Entity
        {
            return orgService.RetrieveByParamBase<T>(param, value, returnColumn, condition, entityName);
        }


        /// <summary>
        /// Retrieve Auto-Number Configuration entity list by Auto-Number Display Entity attribute value
        /// </summary>
        /// <param name="orgService">Organization Service</param>
        /// <param name="entityName">Entity schema name where display Auto-Number</param>
        /// <returns>Auto-Number Configuration entity list</returns>
        public static List<op_auto_number_config> RetrieveAutoNumberConfig(this IOrganizationService orgService, string entityName)
        {
            ColumnSet returnColumn = new ColumnSet(true);

            string[] param = { op_auto_number_config.op_entity_nameAttribute };
            string[] value = { entityName };

            List<op_auto_number_config> autoNumberConfigs = orgService.RetrieveByParam<op_auto_number_config>(param, value, returnColumn);

            return autoNumberConfigs;

        }


        /// <summary>
        /// Retrieve Auto-Number display entity list except execution entity
        /// </summary>
        /// <param name="orgService">Organization Service</param>
        /// <param name="entityName">Entity where display Auto-Number</param>
        /// <param name="attributeName">Attribute schema name where display Auto-Number</param>
        /// <returns>Entity list where display Auto-Number</returns>
        public static List<Entity> RetrieveAutoNumberDisplayEntitiesExceptCurrent(this IOrganizationService orgService, Entity entityName, string attributeName)
        {
            var entityLogicalName = entityName.LogicalName;
            var entityIdAttribute = $"{entityLogicalName.ToLower()}id";

            ColumnSet returnColumn = new ColumnSet
                (
                    entityIdAttribute,//Auto-Number display entity Id
                    attributeName//Auto-Number display attribute
                );

            string[] param = { entityIdAttribute };
            string[] value = { entityName.Id.ToString()};
            ConditionOperator[] notEqual = { ConditionOperator.NotEqual };

            List<Entity> autoNumberDisplayEntities = orgService.RetrieveByParam<Entity>(entityLogicalName, param, value, returnColumn, notEqual);

            return autoNumberDisplayEntities;
        }


        /// <summary>
        /// Retrieve Auto-Number display entity
        /// </summary>
        /// <param name="orgService">Organization Service</param>
        /// <param name="autoNumberDisplayEntity">Entity schema name where display Auto-Number</param>
        /// <param name="attributeName">Attribute schema name where display Auto-Number</param>
        /// <returns>Entity where display Auto-Number</returns>
        public static Entity RetrieveAutoNumberDisplayEntity(this IOrganizationService orgService, string entityName, Guid entityId, string attributeName)
        {
            var entityIdAttribute = $"{entityName.ToLower()}id";

            ColumnSet returnColumn = new ColumnSet
                (
                    entityIdAttribute,//Auto-Number display entity Id
                    attributeName//Auto-Number display attribute
                );

            Entity autoNumberDisplayEntity = orgService.RetrieveById<Entity>(entityName, entityId, returnColumn);

            return autoNumberDisplayEntity;
        }

        /// <summary>
        /// Retireve attribute display name
        /// </summary>
        /// <param name="orgService">Organization Service</param>
        /// <param name="entityName">Entity schema name</param>
        /// <param name="attributeName">Attribute schema name</param>
        /// <returns></returns>
        public static string RetrieveAttributeDisplayName(this IOrganizationService orgService, string entityName, string attributeName)
        {
            RetrieveAttributeRequest retrieveAttributeRequest = new RetrieveAttributeRequest
            {
                EntityLogicalName = entityName,
                LogicalName = attributeName,
                RetrieveAsIfPublished = true
            };
            RetrieveAttributeResponse retrieveAttributeResponse = (RetrieveAttributeResponse)orgService.Execute(retrieveAttributeRequest);
            AttributeMetadata retrievedAttributeMetadata = retrieveAttributeResponse.AttributeMetadata;

            return retrievedAttributeMetadata.DisplayName.UserLocalizedLabel.Label;
        }
    }
}
