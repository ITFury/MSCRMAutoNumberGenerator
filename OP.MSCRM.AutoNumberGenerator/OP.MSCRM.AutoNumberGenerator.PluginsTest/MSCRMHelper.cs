using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.Description;


namespace OP.MSCRM.AutoNumberGenerator.PluginsTest
{
    /// <summary>
    /// MS CRM Helper for testing
    /// </summary>
    public static class MSCRMHelper
    {
        /// <summary>
        /// Create client credentials
        /// </summary>
        /// <param name="userName">User name</param>
        /// <param name="password">User password</param>
        /// <param name="domainName">Domain name</param>
        /// <returns>Client credentials</returns>
        private static ClientCredentials Credentials(string userName, string password, string domainName)
        {

            ClientCredentials credentials = new ClientCredentials();
            credentials.Windows.ClientCredential = new NetworkCredential(userName, password, domainName);
            return credentials;

        }

        /// <summary>
        /// Connect to MS CRM
        /// </summary>
        /// <param name="userName">User name</param>
        /// <param name="password">User password</param>
        /// <param name="domainName">Domain name</param>
        /// <param name="soapOrgServiceUri">Organization Service uri</param>
        /// <returns>Organization Service</returns>
        private static IOrganizationService ConnectToMSCRM(string userName, string password, string domainName, string soapOrgServiceUri)
        {
            IOrganizationService orgService = null;
            try
            {
                ClientCredentials credentials = Credentials(userName, password, domainName);

                Uri serviceUri = new Uri(soapOrgServiceUri);
                OrganizationServiceProxy proxy = new OrganizationServiceProxy(serviceUri, null, credentials, null);
                proxy.EnableProxyTypes();
                orgService = proxy;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MS CRM connection error: {ex.Message}");
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
                return ConnectToMSCRM(
                    "userName",
                    "password",
                    "domainName",
                    "http://ServerName/OrganizationName/XRMServices/2011/Organization.svc");
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
