using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using OP.MSCRM.AutoNumberGenerator.Plugins.Entities;
using System;
using System.Collections.Generic;
using System.Linq;


namespace OP.MSCRM.AutoNumberGenerator.Plugins.Extensions
{
    /// <summary>
    /// Organization Service operation extension
    /// </summary>
    public static class OrganizationServiceExtensions
    {

        /// <summary>
        /// Retrieve entity by Id
        /// </summary>
        /// <typeparam name="T">Entity</typeparam>
        /// <param name="orgService">Organization Service</param>
        /// <param name="entityName">Entity name</param>
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
        /// Retrieve entity list by parameters base
        /// </summary>
        /// <typeparam name="T">Entity</typeparam>
        /// <param name="orgService">Organization Service</param>
        /// <param name="param">Search parameters</param>
        /// <param name="value">Search parameters values</param>
        /// <param name="returnColumn">Column to return</param>
        /// <param name="condition">Condition operator</param>
        /// <param name="entityName">Entity name to return</param>
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
        /// <param name="entityName">Entity name</param>
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
        /// Retrieve op_auto_number_config entity list by op_entity_name value
        /// </summary>
        /// <param name="orgService">Organization Service</param>
        /// <param name="entityName">Entity name where display Auto Number</param>
        /// <returns>op_auto_number_config entity list</returns>
        public static List<op_auto_number_config> RetrieveAutoNumberConfig(this IOrganizationService orgService, string entityName)
        {
            ColumnSet returnColumn = new ColumnSet(true);

            string[] param = { op_auto_number_config.op_entity_nameAttribute };
            string[] value = { entityName };

            List<op_auto_number_config> autoNumberConfigs = orgService.RetrieveByParam<op_auto_number_config>(param, value, returnColumn);

            return autoNumberConfigs;

        }


        /// <summary>
        /// Retrieve Auto Number display entity list except execution entity
        /// </summary>
        /// <param name="orgService">Organization Service</param>
        /// <param name="autoNumberDisplayEntity">Entity name where display Auto Number</param>
        /// <param name="autoNumberDisplayField">Entity field where display Auto Number</param>
        /// <returns>Entity list</returns>
        public static List<Entity> RetrieveAutoNumberDisplayEntities(this IOrganizationService orgService, Entity autoNumberDisplayEntity, string autoNumberDisplayField)
        {
            var entityName = autoNumberDisplayEntity.LogicalName;
            var entityIdAttribute = $"{entityName.ToLower()}id";

            ColumnSet returnColumn = new ColumnSet
                (
                    entityIdAttribute,//Auto Number Display Entity Id
                    autoNumberDisplayField//Auto Number Display Entity Display Field
                );

            string[] param = { entityIdAttribute };
            string[] value = { autoNumberDisplayEntity.Id.ToString()};
            ConditionOperator[] notEqual = { ConditionOperator.NotEqual };

            List<Entity> autoNumberDisplayEntities = orgService.RetrieveByParam<Entity>(entityName, param, value, returnColumn, notEqual);

            return autoNumberDisplayEntities;
        }
    }
}
