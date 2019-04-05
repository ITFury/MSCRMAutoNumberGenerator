using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using System.Threading;
using System.Linq;
using Microsoft.Xrm.Sdk.Query;
using OP.MSCRM.AutoNumberGenerator.Plugins.Extensions;
using System;

namespace OP.MSCRM.AutoNumberGenerator.PluginsTest
{
    /// <summary>
    /// Auto Number generation manager test. Test Auto-Number generation methods such as Create, Update, Delete
    /// </summary>
    [TestClass]
    public class GenerateAutoNumberManagerMethodsTest: GenerateAutoNumberManagerAutoNumbersTest
    {

        /// <summary>
        /// Entity schema name where display Auto-Number
        /// </summary>
        private const string entityLogicalName = "op_auto_number_test";


        /// <summary>
        /// Entity field schema name where display Auto-Number
        /// </summary>
        private const string entityAttributeName = "op_auto_number";


        /// <summary>
        /// Actual Organization Service
        /// </summary>
        private IOrganizationService ActualOrgService
        {
            get
            {
                return MSCRMHelper.OrgService;
            }
        }

        /// <summary>
        /// Create Auto-Number display entity
        /// </summary>
        /// <returns></returns>
        public Entity CreateEntity()
        {
            Entity autoNumberDisplayEntity = new Entity
            {
                LogicalName = entityLogicalName
            };
            autoNumberDisplayEntity[entityAttributeName] = "test";

            return autoNumberDisplayEntity;
        }

        /// <summary>
        /// Update Auto-Number display entity
        /// </summary>
        /// <param name="entity"></param>
        private void UpdateEntity(Entity entity)
        {
            Entity autoNumberDisplayEntity = new Entity
            {
                Id = entity.Id,
                LogicalName = entityLogicalName
            };
            autoNumberDisplayEntity[entityAttributeName] = "updated";
            ActualOrgService.Update(autoNumberDisplayEntity);
        }

        /// <summary>
        /// Delete Auto-Number display entities
        /// </summary>
        private void DeleteEntities()
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

        /// <summary>
        /// Expected Auto-Number from Auto-Number Configuration entity
        /// </summary>
        /// <returns></returns>
        private string ExpectedAutoNumber()
        {
            var autoNumberConfigs = ActualOrgService.RetrieveAutoNumberConfig(entityLogicalName);
            if (autoNumberConfigs == null)
            {
                return null;
            }

            var autoNumberConfig = autoNumberConfigs.FirstOrDefault();
            //Get params
            int? autoNumberLenght = autoNumberConfig.op_number_length;
            int? autoNumberSeq = autoNumberConfig.op_sequence;
            string autoNumberPrefix = autoNumberConfig.op_number_prefix;
            string autoNumberSuffix = autoNumberConfig.op_number_suffix;

            var expectedAutoNumber = AutoNumberManager.GenerateAutoNumber(autoNumberLenght, autoNumberSeq, autoNumberPrefix, autoNumberSuffix);

            return expectedAutoNumber;
        }

        [TestMethod]
        public void CreateAutoNumber_SetAutoNumberToDefault_Valid()
        {
            //Arrange
            DeleteEntities();

            //Create one entity
            Entity entity = CreateEntity();
            var entityId = ActualOrgService.Create(entity);

            //Expected Auto-Number from Auto-Number Configuration entity
            var expectedAutoNumber = ExpectedAutoNumber();
            if(expectedAutoNumber == null)
            {
                Assert.Fail("Current entity doesn't contain Auto-Number configuration.");
            }
            
            //Act
            //Retrieve Auto-Number display entity
            Entity createdEntity = ActualOrgService.Retrieve(entityLogicalName, entityId, new ColumnSet(entityAttributeName));

            var actualAutoNumber = createdEntity.GetAttributeValue<string>(entityAttributeName);

            //Assert
            Assert.AreEqual(expectedAutoNumber, actualAutoNumber);
        }

        [TestMethod]
        public void CreateAutoNumber_CheckLastAutoNumber_Valid()
        {
            //Arrange
            DeleteEntities();

            //Create entities
            for (int i = 0; i < 5; i++)
            {
                Entity entity = CreateEntity();
                ActualOrgService.Create(entity);
            }

            //Expected Auto-Number from Auto-Number Configuration entity
            var expectedAutoNumber = ExpectedAutoNumber();
            if (expectedAutoNumber == null)
            {
                Assert.Fail("Current entity doesn't contain Auto-Number configuration.");
            }

            //Act
            //Retrieve Auto-Number display entities
            List<Entity> createdEntities = ActualOrgService.RetrieveAll<Entity>(entityLogicalName, new ColumnSet(entityAttributeName));
            var lastEntity = createdEntities.OrderBy(e => e.GetAttributeValue<string>(entityAttributeName)).LastOrDefault();

            var actualAutoNumber = lastEntity.GetAttributeValue<string>(entityAttributeName);

            //Assert
            Assert.AreEqual(expectedAutoNumber, actualAutoNumber);
        }


        [TestMethod]
        public void CreateAutoNumber_SetAutoNumberInThreadWithoutDuplicates_Valid()
        {
            //Arrange
            DeleteEntities();

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

            //Act
            //Validate duplicate of Auto-Number
            List<Entity> entities = ActualOrgService.RetrieveAll<Entity>(entityLogicalName, new ColumnSet(entityAttributeName));

            bool isDuplicate = entities.GroupBy(e => e.Attributes[entityAttributeName]).Any(a => a.Count() > 1);

            //Assert
            Assert.IsFalse(isDuplicate);
        }


        [TestMethod]
        public void NoUpdateAutoNumber_ThrowExceptionWhenUpdateManually_Valid()
        {
            //Arrange
            DeleteEntities();

            //Create entities
            for (int i = 0; i < 5; i++)
            {
                Entity entity = CreateEntity();
                ActualOrgService.Create(entity);
            }

            //Act
            //Retrieve entities
            List<Entity> createdEntities = ActualOrgService.RetrieveAll<Entity>(entityLogicalName, new ColumnSet(entityAttributeName));
            var lastEntity = createdEntities.OrderBy(e => e.GetAttributeValue<string>(entityAttributeName)).LastOrDefault();

            try
            {
                //Update entity
                UpdateEntity(lastEntity);
                //Assert
                Assert.Fail("Exception not thrown.");
            }
            catch (Exception ex)
            {
                //Assert
                Assert.IsNotNull(ex);
            }
        }


        [TestMethod]
        public void DeleteAutoNumber_RearrangeAutoNumberToDefault_Valid()
        {
            //Arrange
            DeleteEntities();

            //Retrieve Auto-Number Config
            var autoNumberConfigs = ActualOrgService.RetrieveAutoNumberConfig(entityLogicalName);
            if (autoNumberConfigs == null)
            {
                Assert.Fail("Current entity doesn't contain Auto-Number configuration.");
            }

            var autoNumberConfig = autoNumberConfigs.FirstOrDefault();
            if (!autoNumberConfig.op_rearrange_sequence)
            {
                //Set Rearrange Sequence After Delete to Yes in Auto-Number Configuration entity
                autoNumberConfig.op_rearrange_sequence = true;
                ActualOrgService.Update(autoNumberConfig);
            }
            //Get params
            int? autoNumberLength = autoNumberConfig.op_number_length;
            int? autoNumberStartNumber = autoNumberConfig.op_start_number;
            string autoNumberPrefix = autoNumberConfig.op_number_prefix;
            string autoNumberSuffix = autoNumberConfig.op_number_suffix;

            var expectedAutoNumber = AutoNumberManager.GenerateAutoNumber(autoNumberLength, autoNumberStartNumber, autoNumberPrefix, autoNumberSuffix);

            //Create entities
            for (int i = 0; i < 5; i++)
            {
                Entity entity = CreateEntity();
                ActualOrgService.Create(entity);
            }

            //Act
            //Retrieve Auto-Number display entities
            List<Entity> createdEntities = ActualOrgService
                .RetrieveAll<Entity>(entityLogicalName, new ColumnSet(entityAttributeName))
                .OrderBy(e => e.GetAttributeValue<string>(entityAttributeName))
                .ToList();

            var createdFourEntities = createdEntities.Take(4);

            //Delete entities, except last
            foreach (var createdEntity in createdFourEntities)
            {
                ActualOrgService.Delete(entityLogicalName, createdEntity.Id);
            }

            //Retrieve Auto-Number display entity after delete
            var lastEntity = ActualOrgService
                .RetrieveAll<Entity>(entityLogicalName, new ColumnSet(entityAttributeName))
                .LastOrDefault();

            var actualAutoNumber = lastEntity.GetAttributeValue<string>(entityAttributeName);

            //Assert
            Assert.AreEqual(expectedAutoNumber, actualAutoNumber);
        }

        [TestMethod]
        public void DeleteAutoNumber_RearrangeAutoNumberToPrevious_Valid()
        {
            //Arrange
            DeleteEntities();

            //Retrieve Auto-Number Config
            var autoNumberConfigs = ActualOrgService.RetrieveAutoNumberConfig(entityLogicalName);
            if (autoNumberConfigs == null)
            {
                Assert.Fail("Current entity doesn't contain Auto-Number configuration.");
            }

            var autoNumberConfig = autoNumberConfigs.FirstOrDefault();
            if (!autoNumberConfig.op_rearrange_sequence)
            {
                //Set Rearrange Sequence After Delete to Yes in Auto-Number Configuration entity
                autoNumberConfig.op_rearrange_sequence = true;
                ActualOrgService.Update(autoNumberConfig);
            }

            //Create Auto-Number display entities
            for (int i = 0; i < 5; i++)
            {
                Entity entity = CreateEntity();
                ActualOrgService.Create(entity);
            }

            //Retrieve entities
            List<Entity> createdEntities = ActualOrgService.RetrieveAll<Entity>(entityLogicalName, new ColumnSet(entityAttributeName));
            var fourthEntity = createdEntities.OrderBy(e => e.GetAttributeValue<string>(entityAttributeName)).Skip(3).Take(1).FirstOrDefault();

            var expectedAutoNumber = fourthEntity.GetAttributeValue<string>(entityAttributeName);

            //Act
            //Delete fourth entity
            ActualOrgService.Delete(entityLogicalName, fourthEntity.Id);
            
            //Retrieve entities
            List<Entity> entities = ActualOrgService.RetrieveAll<Entity>(entityLogicalName, new ColumnSet(entityAttributeName));
            var lastEntity = entities.OrderBy(e => e.GetAttributeValue<string>(entityAttributeName)).LastOrDefault();
            var actualAutoNumber = lastEntity.GetAttributeValue<string>(entityAttributeName);

            //Assert
            Assert.AreEqual(expectedAutoNumber, actualAutoNumber);
        }

        [TestMethod]
        public void DeleteAutoNumber_NoRearrangeAutoNumber_Valid()
        {
            //Arrange
            DeleteEntities();

            //Retrieve Auto-Number Config
            var autoNumberConfigs = ActualOrgService.RetrieveAutoNumberConfig(entityLogicalName);
            if (autoNumberConfigs == null)
            {
                Assert.Fail("Current entity doesn't contain Auto-Number configuration.");
            }

            var autoNumberConfig = autoNumberConfigs.FirstOrDefault();
            if (autoNumberConfig.op_rearrange_sequence)
            {
                //Set Rearrange Sequence After Delete to No in Auto-Number Configuration entity
                autoNumberConfig.op_rearrange_sequence = false;
                ActualOrgService.Update(autoNumberConfig);
            }

            //Create Auto-Number display entities
            for (int i = 0; i < 5; i++)
            {
                Entity entity = CreateEntity();
                ActualOrgService.Create(entity);
            }

            //Retrieve entities
            List<Entity> createdEntities = ActualOrgService.RetrieveAll<Entity>(entityLogicalName, new ColumnSet(entityAttributeName));
            var fourthEntity = createdEntities.OrderBy(e => e.GetAttributeValue<string>(entityAttributeName)).Skip(3).Take(1).FirstOrDefault();

            var lastCreatedEntity = createdEntities.OrderBy(e => e.GetAttributeValue<string>(entityAttributeName)).LastOrDefault();

            var expectedAutoNumber = lastCreatedEntity.GetAttributeValue<string>(entityAttributeName);

            //Act
            //Delete fourth entity
            ActualOrgService.Delete(entityLogicalName, fourthEntity.Id);

            //Retrieve entities after delete
            List<Entity> entities = ActualOrgService.RetrieveAll<Entity>(entityLogicalName, new ColumnSet(entityAttributeName));
            var lastEntity = entities.OrderBy(e => e.GetAttributeValue<string>(entityAttributeName)).LastOrDefault();
            var actualAutoNumber = lastEntity.GetAttributeValue<string>(entityAttributeName);

            //Assert
            Assert.AreEqual(expectedAutoNumber, actualAutoNumber);
        }


        [TestMethod]
        public void DeleteAutoNumber_RearrangeAutoNumberInThreadWithoutDuplicates_Valid()
        {
            //Arrange
            DeleteEntities();

            //Retrieve Auto-Number Config
            var autoNumberConfigs = ActualOrgService.RetrieveAutoNumberConfig(entityLogicalName);
            if (autoNumberConfigs == null)
            {
                Assert.Fail("Current entity doesn't contain Auto-Number configuration.");
            }

            var autoNumberConfig = autoNumberConfigs.FirstOrDefault();
            if (!autoNumberConfig.op_rearrange_sequence)
            {
                //Set Rearrange Sequence After Delete to Yes in Auto-Number Configuration entity
                autoNumberConfig.op_rearrange_sequence = true;
                ActualOrgService.Update(autoNumberConfig);
            }

            //Create entities
            for (int i = 0; i < 10; i++)
            {
                Entity entity = CreateEntity();
                ActualOrgService.Create(entity);
            }

            //Retrieve created entities
            List<Entity> createdEntities = ActualOrgService
                .RetrieveAll<Entity>(entityLogicalName, new ColumnSet(entityAttributeName))
                .OrderBy(e => e.GetAttributeValue<string>(entityAttributeName))
                .ToList();

            //Delete half of created entities
            Thread[] threads = new Thread[5];
            for (int i = 0; i < threads.Length; i++)
            {
                var entityId = createdEntities[i].Id;
                threads[i] = new Thread(() => ActualOrgService.Delete(entityLogicalName, entityId));
            }

            foreach (Thread thread in threads)
            {
                thread.Start();
            }

            //Act
            //Validate duplicate of rearranged Auto-Number
            var updatedRearrangedEntities = ActualOrgService.RetrieveAll<Entity>(entityLogicalName, new ColumnSet(entityAttributeName));

            bool isDuplicate = updatedRearrangedEntities.GroupBy(e => e.Attributes[entityAttributeName]).Any(a => a.Count() > 1);

            //Assert
            Assert.IsFalse(isDuplicate);
        }
    }
}
