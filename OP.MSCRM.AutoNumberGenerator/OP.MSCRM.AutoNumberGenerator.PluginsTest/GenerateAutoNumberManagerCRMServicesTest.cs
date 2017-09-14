using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using System.Threading;
using System.Linq;
using Microsoft.Xrm.Sdk.Query;
using OP.MSCRM.AutoNumberGenerator.Plugins.Extensions;

namespace OP.MSCRM.AutoNumberGenerator.PluginsTest
{
    /// <summary>
    /// Auto Number generation manager test. Test MS CRM services.
    /// </summary>
    [TestClass]
    public class GenerateAutoNumberManagerCRMServicesTest: GenerateAutoNumberManagerTest
    {
        #region MS CRM Auto Number generation test

        /// <summary>
        /// Entity name where display Auto Number
        /// </summary>
        private const string entityLogicalName = "op_auto_number_test";


        /// <summary>
        /// Entity field name where display Auto Number
        /// </summary>
        private const string entityAttributeName = "op_auto_number";


        private IOrganizationService ActualOrgService
        {
            get
            {
                return MSCRMHelper.OrgService;
            }
        }


        public Entity CreateEntity()
        {
            Entity autoNumberDisplayEntity = new Entity();
            autoNumberDisplayEntity.LogicalName = entityLogicalName;
            autoNumberDisplayEntity[entityAttributeName] = "test";

            return autoNumberDisplayEntity;
        }


        [TestMethod]
        public void CreateAutoNumberDisplayEntity_Valid()
        {
            //Act
            Entity entity = CreateEntity();

            var actualEntityId = ActualOrgService.Create(entity);

            //Assert
            Assert.IsNotNull(actualEntityId);
        }


        [TestMethod]
        public void CreateAutoNumberDisplayEntity_NoDuplicateAutoNumberValid()
        {
            //Act
            Thread[] threads = new Thread[30];
            for (int i = 0; i < threads.Length; i++)
            {
                //Create entity
                Entity entity = CreateEntity();
                threads[i] = new Thread(() => ActualOrgService.Create(entity));
            }

            foreach (Thread thread in threads)
            {
                thread.Start();
            }

            foreach (Thread thread in threads)
            {
                thread.Join();
            }

            //Validate duplicate of Auto Number
            List<Entity> entities = ActualOrgService.RetrieveAll<Entity>(entityLogicalName, new ColumnSet(entityAttributeName));

            bool isDuplicate = entities.GroupBy(e => e.Attributes[entityAttributeName]).Any(a => a.Count() > 1);

            //Assert
            Assert.IsFalse(isDuplicate);
        }


        private void UpdateEntity(Entity entity)
        {
            Entity autoNumberDisplayEntity = new Entity();
            autoNumberDisplayEntity.Id = entity.Id;
            autoNumberDisplayEntity.LogicalName = entityLogicalName;
            autoNumberDisplayEntity[entityAttributeName] = "updated";
            ActualOrgService.Update(autoNumberDisplayEntity);
        }


        private void DeletePreviousEntities()
        {
            //Retrieve entities
            List<Entity> entitiesToClear = ActualOrgService.RetrieveAll<Entity>(entityLogicalName, new ColumnSet(entityAttributeName));
            if (entitiesToClear != null && entitiesToClear.Count > 0)
            {
                foreach (var entityToClear in entitiesToClear)
                {
                    //Delete previous created records
                    ActualOrgService.Delete(entityLogicalName, entityToClear.Id);
                }
            }
        }


        [TestMethod]
        public void UpdateAutoNumberDisplayEntity_Valid()
        {
            //Arrange
            var autoNumberConfigs = ActualOrgService.RetrieveAutoNumberConfig(entityLogicalName);
            string autoNumberFormat = string.Empty;
            int? autoNumberFormatLength = null;
            if (autoNumberConfigs != null)
            {
                foreach (var autoNumberConfig in autoNumberConfigs)
                {
                    //Get format
                    autoNumberFormat = autoNumberConfig.op_format;
                    autoNumberFormatLength = autoNumberConfig.op_format_number_length;
                }
            }

            var formatWithLength = AutoNumberManager.GenerateAutoNumber(autoNumberFormat, autoNumberFormatLength.Value, 9);

            var expectedAutoNumber = string.Format(formatWithLength, "10");

            //Act
            DeletePreviousEntities();

            for (int i = 0; i < 10; i++)
            {
                Entity entity = CreateEntity();
                //Create entity
                ActualOrgService.Create(entity);
            }

            //Retrieve entities
            List<Entity> createdEntities = ActualOrgService.RetrieveAll<Entity>(entityLogicalName, new ColumnSet(entityAttributeName));

            var lastEntity = createdEntities.LastOrDefault();
            //Update entity
            UpdateEntity(lastEntity);
            var actualAutoNumber = lastEntity.GetAttributeValue<string>(entityAttributeName);

            //Assert
            Assert.AreEqual(expectedAutoNumber, actualAutoNumber);

        }


        [TestMethod]
        public void UpdateAutoNumberDisplayEntity_NoDuplicateAutoNumberValid()
        {
            //Act

            //Create entity
            for (int i = 0; i < 20; i++)
            {
                Entity entity = CreateEntity();
                ActualOrgService.Create(entity);
            }

            //Retrieve entities
            List<Entity> createdEntities = ActualOrgService.RetrieveAll<Entity>(entityLogicalName, new ColumnSet(entityAttributeName));

            Thread[] threads = new Thread[20];
            for (int i = 0; i < threads.Length; i++)
            {
                //Update entity
                var entity = createdEntities[i];
                threads[i] = new Thread(() => UpdateEntity(entity));
            }

            foreach (Thread thread in threads)
            {
                thread.Start();
            }

            foreach (Thread thread in threads)
            {
                thread.Join();
            }

            //Validate duplicate of Auto Number
            var updatedEntities = ActualOrgService.RetrieveAll<Entity>(entityLogicalName, new ColumnSet(entityAttributeName));

            bool isDuplicate = updatedEntities.GroupBy(e => e.Attributes[entityAttributeName]).Any(a => a.Count() > 1);

            //Assert
            Assert.IsFalse(isDuplicate);
        }


        [TestMethod]
        public void DeleteAutoNumberDisplayEntity_Valid()
        {
            //Arrange
            var expectedAutoNumberCount = 5;

            //Act
            DeletePreviousEntities();

            //Retrieve entities
            List<Entity> entitiesToClear = ActualOrgService.RetrieveAll<Entity>(entityLogicalName, new ColumnSet(entityAttributeName));
            if(entitiesToClear != null && entitiesToClear.Count > 0)
            {
                foreach (var entityToClear in entitiesToClear)
                {
                    //Delete previous created records
                    ActualOrgService.Delete(entityLogicalName, entityToClear.Id);
                }
            }

            for (int i = 0; i < 10; i++)
            {
                Entity entity = CreateEntity();
                //Create entity
                ActualOrgService.Create(entity);
            }

            //Retrieve entities
            List<Entity> createdEntities = ActualOrgService.RetrieveAll<Entity>(entityLogicalName, new ColumnSet(entityAttributeName));

            for (int i = 0; i < createdEntities.Count; i = i+2)
            {
                var entityId = createdEntities[i + 1].Id;
                ActualOrgService.Delete(entityLogicalName, entityId);
            }

            //Retrieve entities
            List<Entity> entities = ActualOrgService.RetrieveAll<Entity>(entityLogicalName, new ColumnSet(entityAttributeName));
            var actualAutoNumberCount = entities.Count();

            //Assert
            Assert.AreEqual(expectedAutoNumberCount, actualAutoNumberCount);

        }


        [TestMethod]
        public void DeleteAutoNumberDisplayEntity_NoDuplicateAutoNumberValid()
        {
            //Act
            DeletePreviousEntities();
            //Create entity
            for (int i = 0; i < 20; i++)
            {
                Entity entity = CreateEntity();
                ActualOrgService.Create(entity);
            }

            //Retrieve entities
            List<Entity> createdEntities = ActualOrgService.RetrieveAll<Entity>(entityLogicalName, new ColumnSet(entityAttributeName));


            Thread[] threads = new Thread[15];
            for (int i = 0; i < threads.Length; i++)
            {
                //Delete entity
                var entityId = createdEntities[i].Id;
                threads[i] = new Thread(() => ActualOrgService.Delete(entityLogicalName, entityId));
            }

            foreach (Thread thread in threads)
            {
                thread.Start();
            }

            foreach (Thread thread in threads)
            {
                thread.Join();
            }

            //Validate duplicate of Auto Number
            var updatedEntities = ActualOrgService.RetrieveAll<Entity>(entityLogicalName, new ColumnSet(entityAttributeName));

            bool isDuplicate = updatedEntities.GroupBy(e => e.Attributes[entityAttributeName]).Any(a => a.Count() > 1);


            //Assert
            Assert.IsFalse(isDuplicate);

        }

        #endregion
    }
}
