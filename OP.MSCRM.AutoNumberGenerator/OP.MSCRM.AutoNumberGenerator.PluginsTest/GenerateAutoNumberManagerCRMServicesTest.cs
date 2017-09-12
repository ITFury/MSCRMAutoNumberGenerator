using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using System.Threading;
using System.Linq;
using Microsoft.Xrm.Sdk.Query;


namespace OP.MSCRM.AutoNumberGenerator.PluginsTest
{
    /// <summary>
    /// Auto Number generation manager test. Test MS CRM services.
    /// </summary>
    [TestClass]
    public class GenerateAutoNumberManagerCRMServicesTest
    {
        #region MS CRM Auto Number generation test

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
            autoNumberDisplayEntity.LogicalName = "op_auto_number_test";
            autoNumberDisplayEntity["op_auto_number"] = "test";

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
            List<Entity> entities = ActualOrgService.RetrieveAll<Entity>("op_auto_number_test", new ColumnSet("op_auto_number"));

            bool isDuplicate = entities.GroupBy(e => e.Attributes["op_auto_number"]).Any(a => a.Count() > 1);

            //Assert
            Assert.IsFalse(isDuplicate);
        }


        private void UpdateEntity(Entity entity)
        {
            Entity autoNumberDisplayEntity = new Entity();
            autoNumberDisplayEntity.Id = entity.Id;
            autoNumberDisplayEntity.LogicalName = "op_auto_number_test";
            autoNumberDisplayEntity["op_auto_number"] = "updated";
            ActualOrgService.Update(autoNumberDisplayEntity);
        }


        [TestMethod]
        public void UpdateAutoNumberDisplayEntity_Valid()
        {
            //Arrange
            var expectedAutoNumber = "10";

            //Act
            for (int i = 0; i < 10; i++)
            {
                Entity entity = CreateEntity();
                //Create entity
                ActualOrgService.Create(entity);
            }

            //Retrieve entities
            List<Entity> createdEntities = ActualOrgService.RetrieveAll<Entity>("op_auto_number_test", new ColumnSet("op_auto_number"));

            var lastEntity = createdEntities.LastOrDefault();
            //Update entity
            UpdateEntity(lastEntity);
            var actualAutoNumber = lastEntity.GetAttributeValue<string>("op_auto_number");

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
            List<Entity> createdEntities = ActualOrgService.RetrieveAll<Entity>("op_auto_number_test", new ColumnSet("op_auto_number"));


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
            var updatedEntities = ActualOrgService.RetrieveAll<Entity>("op_auto_number_test", new ColumnSet("op_auto_number"));

            bool isDuplicate = updatedEntities.GroupBy(e => e.Attributes["op_auto_number"]).Any(a => a.Count() > 1);

            //Assert
            Assert.IsFalse(isDuplicate);
        }


        [TestMethod]
        public void DeleteAutoNumberDisplayEntity_Valid()
        {
            //Arrange
            var expectedAutoNumberCount = 5;

            //Act
            for (int i = 0; i < 10; i++)
            {
                Entity entity = CreateEntity();
                //Create entity
                ActualOrgService.Create(entity);
            }

            //Retrieve entities
            List<Entity> createdEntities = ActualOrgService.RetrieveAll<Entity>("op_auto_number_test", new ColumnSet("op_auto_number"));

            for (int i = 0; i < createdEntities.Count; i = i+2)
            {
                var entityId = createdEntities[i + 1].Id;
                ActualOrgService.Delete("op_auto_number_test", entityId);
            }

            //Retrieve entities
            List<Entity> entities = ActualOrgService.RetrieveAll<Entity>("op_auto_number_test", new ColumnSet("op_auto_number"));
            var actualAutoNumberCount = entities.Count();

            //Assert
            Assert.AreEqual(expectedAutoNumberCount, actualAutoNumberCount);

        }


        [TestMethod]
        public void DeleteAutoNumberDisplayEntity_NoDuplicateAutoNumberValid()
        {
            //Act

            //Create entity
            for (int i = 0; i < 20; i++)
            {
                Entity entity = CreateEntity();
                ActualOrgService.Create(entity);
            }

            //Retrieve entities
            List<Entity> createdEntities = ActualOrgService.RetrieveAll<Entity>("op_auto_number_test", new ColumnSet("op_auto_number"));


            Thread[] threads = new Thread[15];
            for (int i = 0; i < threads.Length; i++)
            {
                //Delete entity
                var entityId = createdEntities[i].Id;
                threads[i] = new Thread(() => ActualOrgService.Delete("op_auto_number_test", entityId));
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
            var updatedEntities = ActualOrgService.RetrieveAll<Entity>("op_auto_number_test", new ColumnSet("op_auto_number"));

            bool isDuplicate = updatedEntities.GroupBy(e => e.Attributes["op_auto_number"]).Any(a => a.Count() > 1);


            //Assert
            Assert.IsFalse(isDuplicate);

        }

        #endregion
    }
}
