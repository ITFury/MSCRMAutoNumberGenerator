using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Description;


namespace OP.MSCRM.AutoNumberGenerator.PluginsTest
{
    /// <summary>
    /// D365 CRM Helper for Tests
    /// </summary>
    public static class MSCRMHelper
    {
        /// <summary>
        /// Create client credentials
        /// </summary>
        /// <param name="userName">User name</param>
        /// <param name="password">User password</param>
        /// <returns>Client credentials</returns>
        private static ClientCredentials Credentials(string userName, string password)
        {

            ClientCredentials credentials = new ClientCredentials();
            credentials.UserName.UserName = userName;
            credentials.UserName.Password = password;

            return credentials;

        }

        /// <summary>
        /// Connect to D365 CRM
        /// </summary>
        /// <param name="userName">User name</param>
        /// <param name="password">User password</param>
        /// <param name="soapOrgServiceUri">Organization Service Endpoint</param>
        /// <returns>Organization Service</returns>
        private static IOrganizationService ConnectToD365CRM(string userName, string password, string soapOrgServiceUri)
        {
            IOrganizationService orgService = null;
            try
            {
                ClientCredentials credentials = Credentials(userName, password);
                
                Uri serviceUri = new Uri(soapOrgServiceUri);
                OrganizationServiceProxy proxy = new OrganizationServiceProxy(serviceUri, null, credentials, null);
                proxy.EnableProxyTypes();
                orgService = proxy;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"D365 CRM connection error: {ex.Message}");
            }

            return orgService;
        }

        /// <summary>
        /// Get Organization Services
        /// </summary>
        /// <remarks>Add personal credentials in get statement</remarks>    
        public static IOrganizationService OrgService
        {
            get
            {
                return ConnectToD365CRM(
                    "Username",
                    "Password",
                    "https://xxxxx.api.crm4.dynamics.com/XRMServices/2011/Organization.svc");
            }
        }


        /// <summary>
        /// Retrieve all entity records
        /// </summary>
        /// <typeparam name="T">Entity to retrieve</typeparam>
        /// <param name="orgService">Organization Service</param>
        /// <param name="entityName">Entity name</param>
        /// <param name="returnColumn">Column to return</param>
        /// <returns>Entity list</returns>
        public static List<T> RetrieveAll<T>(this IOrganizationService orgService, string entityName, ColumnSet returnColumn) where T : Entity
        {
            QueryExpression query = new QueryExpression
            {
                EntityName = entityName,
                ColumnSet = returnColumn
            };

            EntityCollection entityCollection = new EntityCollection();

            int pageNumber = 1;
            RetrieveMultipleRequest multipleRequest;
            RetrieveMultipleResponse multipleResponse = new RetrieveMultipleResponse();
            do
            {
                query.PageInfo.Count = 5000;

                query.PageInfo.PagingCookie = (pageNumber == 1) ? null : multipleResponse.EntityCollection.PagingCookie;
                query.PageInfo.PageNumber = pageNumber++;

                multipleRequest = new RetrieveMultipleRequest()
                {
                    Query = query
                };

                multipleResponse = (RetrieveMultipleResponse)orgService.Execute(multipleRequest);

                entityCollection.Entities.AddRange(multipleResponse.EntityCollection.Entities);
            }
            while (multipleResponse.EntityCollection.MoreRecords);

            if (entityCollection == null || entityCollection.Entities.Count == 0)
            {
                return null;
            }

            List<T> returnEntities = entityCollection.Entities.Select(e => e.ToEntity<T>()).ToList();

            return returnEntities;
        }
    }
}
