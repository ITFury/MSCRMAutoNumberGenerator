using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using System.Threading;
using System.Linq;
using Microsoft.Xrm.Sdk.Query;
using OP.MSCRM.AutoNumberGenerator.Plugins.Extensions;
using OP.MSCRM.AutoNumberGenerator.Plugins.Managers;
using NUnit.Framework;
using OP.MSCRM.AutoNumberGenerator.Plugins.ExceptionHandling;
using System.ServiceModel;

namespace OP.MSCRM.AutoNumberGenerator.PluginsTest
{
    /// <summary>
    /// Auto-Number generation manager Test. Test Auto-Number generation methods such as Generate, Create, Update, Delete, Rearrange
    /// </summary>
    [TestFixture]
    public class GenerateAutoNumberManagerTest
    {

        /// <summary>
        /// Entity schema name where display Auto-Number
        /// </summary>
        private const string EntitySchemaName = "op_autonumbertest";

        /// <summary>
        /// Entity field schema name where display Auto-Number
        /// </summary>
        private const string FieldSchemaName = "op_autonumber";

        public GenerateAutoNumberManager AutoNumberManager
        {
            get
            {
                return new GenerateAutoNumberManager();
            }
        }


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
        private Entity CreateEntity()
        {
            Entity autoNumberDisplayEntity = new Entity
            {
                LogicalName = EntitySchemaName
            };
            autoNumberDisplayEntity[FieldSchemaName] = "test";

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
                LogicalName = EntitySchemaName
            };
            autoNumberDisplayEntity[FieldSchemaName] = "updated";
            ActualOrgService.Update(autoNumberDisplayEntity);
        }

        /// <summary>
        /// Delete Auto-Number display entities
        /// </summary>
        private void DeleteEntities()
        {
            //Retrieve entities
            List<Entity> entitiesToClear = ActualOrgService.RetrieveAll<Entity>(EntitySchemaName, new ColumnSet(FieldSchemaName));
            if (entitiesToClear != null && entitiesToClear.Count > 0)
            {
                foreach (var entityToClear in entitiesToClear)
                {
                    //Delete previous created records
                    ActualOrgService.Delete(EntitySchemaName, entityToClear.Id);
                }
            }
        }

        /// <summary>
        /// Expected Auto-Number from Auto-Number Configuration entity
        /// </summary>
        /// <param name="isRearrange"></param>
        /// <returns></returns>
        private string ExpectedAutoNumber(bool isRearrange)
        {
            var configs = ActualOrgService.RetrieveAutoNumberConfig(EntitySchemaName);
            if (configs != null)
            {
                var config = configs.FirstOrDefault();

                if (config.IsRearrangeSequence != isRearrange)
                {
                    //Set Rearrange Sequence After Delete to Yes/No in Auto-Number Configuration entity
                    config.IsRearrangeSequence = isRearrange;
                    ActualOrgService.Update(config);
                }

                //Get params
                int? configLenght = config.Length;
                int? configSequence = config.Sequence;
                string configPrefix = config.Prefix;
                string configSuffix = config.Suffix;

                var expectedAutoNumber = AutoNumberManager.Generate(configLenght, configSequence, configPrefix, configSuffix);

                return expectedAutoNumber;
            }

            return null;
        }

        /// <summary>
        /// Update Auto-Number Configuration entity
        /// </summary>
        /// <param name="isRearrange"></param>
        private void ConfigureRearrange(bool isRearrange)
        {
            var configs = ActualOrgService.RetrieveAutoNumberConfig(EntitySchemaName);
            if (configs == null)
            {
                Assert.Fail("Current entity doesn't contain Auto-Number configuration.");
            }

            var config = configs.FirstOrDefault();
            if (config.IsRearrangeSequence != isRearrange)
            {
                //Set Rearrange Sequence After Delete to Yes/No in Auto-Number Configuration entity
                config.IsRearrangeSequence = isRearrange;
                ActualOrgService.Update(config);
            }
        }


        [TestCase(1, 1, null, null, "1")]
        [TestCase(4, 2, null, null, "0002")]
        [TestCase(5, 12, null, null, "00012")]
        [TestCase(1, 8, "A-", null, "A-8")]
        [TestCase(6, 20, "A-", "", "A-000020")]
        [TestCase(4, 11, " ", "-B", "0011-B")]
        [TestCase(3, 5, " ", null, "005")]
        [TestCase(7, 17, "A-", "-B", "A-0000017-B")]
        [TestCase(0, 1, "A-", "-B", "A-1-B")]
        [TestCase(6, 670, "A-", "-B", "A-000670-B")]
        [TestCase(null, 1, "A-", "-B", "A-1-B")]
        public void Generate(int? length, int? sequence, string prefix, string suffix, string expectedAutoNumber)
        {
            string actualAutoNumber = AutoNumberManager.Generate(length, sequence, prefix, suffix);

            Assert.AreEqual(expectedAutoNumber, actualAutoNumber);
        }


        [TestCase(null, null, null, null)]
        [TestCase(4, null, null, null)]
        [TestCase(null, null, "A-", "-B")]
        public void Generate_ThrowException_WhenNoValidParams(int? length, int? sequence, string prefix, string suffix)
        {
            void actualAutoNumber() => AutoNumberManager.Generate(length, sequence, prefix, suffix);

            Assert.Throws<PluginException>(actualAutoNumber);
        }


        [TestCase(null, null, "1", 1)]
        [TestCase("", "", "000001", 1)]
        [TestCase(null, null, "000062", 62)]
        [TestCase("AAA-", null, "AAA-122", 122)]
        [TestCase("", "-BBB", "000120-BBB", 120)]
        [TestCase("AAA-", "-BBB", "AAA-000000900-BBB", 900)]
        [TestCase("AAA", null, "AAA", 0)]
        [TestCase("42", null, "420001", 1)]
        [TestCase("42", "0534", "4200040534", 4)]
        [TestCase("4A 2", "0Bs q", "4A 2000000200Bs q", 20)]
        [TestCase("4A%-#$ 2", null, "4A%-#$ 20000201", 201)]
        [TestCase("4A%-#$ 2", " w,!*^", "4A%-#$ 200002010 w,!*^", 2010)]
        public void GetNumber(string prefix, string suffix, string autoNumber, int expectedNumber)
        {
            int actualNumber = AutoNumberManager.GetNumber(autoNumber, prefix, suffix);

            Assert.AreEqual(expectedNumber, actualNumber);
        }


        [Test]
        public void Create_SetAutoNumberToDefault()
        {
            //Arrange
            DeleteEntities();

            //Create one entity
            Entity entity = CreateEntity();
            var entityId = ActualOrgService.Create(entity);

            //Expected Auto-Number from Auto-Number Configuration entity
            var expectedAutoNumber = ExpectedAutoNumber(true);
            if(expectedAutoNumber == null)
            {
                Assert.Fail("Current entity doesn't contain Auto-Number configuration.");
            }
            
            //Act
            //Retrieve Auto-Number display entity
            Entity createdEntity = ActualOrgService.Retrieve(EntitySchemaName, entityId, new ColumnSet(FieldSchemaName));

            var actualAutoNumber = createdEntity.GetAttributeValue<string>(FieldSchemaName);

            //Assert
            Assert.AreEqual(expectedAutoNumber, actualAutoNumber);
        }


        [Test]
        public void Create_CheckLastAutoNumber()
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
            var expectedAutoNumber = ExpectedAutoNumber(true);
            if (expectedAutoNumber == null)
            {
                Assert.Fail("Current entity doesn't contain Auto-Number configuration.");
            }

            //Act
            //Retrieve Auto-Number display entities
            List<Entity> createdEntities = ActualOrgService.RetrieveAll<Entity>(EntitySchemaName, new ColumnSet(FieldSchemaName));
            var lastEntity = createdEntities.OrderBy(e => e.GetAttributeValue<string>(FieldSchemaName)).LastOrDefault();

            var actualAutoNumber = lastEntity.GetAttributeValue<string>(FieldSchemaName);

            //Assert
            Assert.AreEqual(expectedAutoNumber, actualAutoNumber);
        }


        [Test]
        public void Create_SetAutoNumberInThreadWithoutDuplicates()
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
            List<Entity> entities = ActualOrgService.RetrieveAll<Entity>(EntitySchemaName, new ColumnSet(FieldSchemaName));

            bool isDuplicate = entities.GroupBy(e => e.Attributes[FieldSchemaName]).Any(a => a.Count() > 1);

            //Assert
            Assert.IsFalse(isDuplicate);
        }


        [Test]
        public void Update_ThrowException_WhenUpdateManually()
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
            List<Entity> createdEntities = ActualOrgService.RetrieveAll<Entity>(EntitySchemaName, new ColumnSet(FieldSchemaName));
            var lastEntity = createdEntities.OrderBy(e => e.GetAttributeValue<string>(FieldSchemaName)).LastOrDefault();

            void actual() => UpdateEntity(lastEntity);

            //Assert
            Assert.Throws<FaultException<OrganizationServiceFault>>(actual);
        }


        [Test]
        public void Delete_RearrangeAutoNumberToDefault()
        {
            //Arrange
            DeleteEntities();

            var expectedAutoNumber = ExpectedAutoNumber(true);

            //Create entities
            for (int i = 0; i < 5; i++)
            {
                Entity entity = CreateEntity();
                ActualOrgService.Create(entity);
            }

            //Act
            //Retrieve Auto-Number display entities
            List<Entity> createdEntities = ActualOrgService
                .RetrieveAll<Entity>(EntitySchemaName, new ColumnSet(FieldSchemaName))
                .OrderBy(e => e.GetAttributeValue<string>(FieldSchemaName))
                .ToList();

            var createdFourEntities = createdEntities.Take(4);

            //Delete entities, except last
            foreach (var createdEntity in createdFourEntities)
            {
                ActualOrgService.Delete(EntitySchemaName, createdEntity.Id);
            }

            //Retrieve Auto-Number display entity after delete
            var lastEntity = ActualOrgService
                .RetrieveAll<Entity>(EntitySchemaName, new ColumnSet(FieldSchemaName))
                .LastOrDefault();

            var actualAutoNumber = lastEntity.GetAttributeValue<string>(FieldSchemaName);

            //Assert
            Assert.AreEqual(expectedAutoNumber, actualAutoNumber);
        }


        [Test]
        public void Delete_RearrangeAutoNumberToPrevious()
        {
            //Arrange
            DeleteEntities();

            ConfigureRearrange(true);

            //Create Auto-Number display entities
            for (int i = 0; i < 5; i++)
            {
                Entity entity = CreateEntity();
                ActualOrgService.Create(entity);
            }

            //Retrieve entities
            List<Entity> createdEntities = ActualOrgService.RetrieveAll<Entity>(EntitySchemaName, new ColumnSet(FieldSchemaName));
            var fourthEntity = createdEntities.OrderBy(e => e.GetAttributeValue<string>(FieldSchemaName)).Skip(3).Take(1).FirstOrDefault();

            var expectedAutoNumber = fourthEntity.GetAttributeValue<string>(FieldSchemaName);

            //Act
            //Delete fourth entity
            ActualOrgService.Delete(EntitySchemaName, fourthEntity.Id);
            
            //Retrieve entities
            List<Entity> entities = ActualOrgService.RetrieveAll<Entity>(EntitySchemaName, new ColumnSet(FieldSchemaName));
            var lastEntity = entities.OrderBy(e => e.GetAttributeValue<string>(FieldSchemaName)).LastOrDefault();
            var actualAutoNumber = lastEntity.GetAttributeValue<string>(FieldSchemaName);

            //Assert
            Assert.AreEqual(expectedAutoNumber, actualAutoNumber);
        }


        [Test]
        public void Delete_NoRearrangeAutoNumber()
        {
            //Arrange
            DeleteEntities();

            ConfigureRearrange(false);

            //Create Auto-Number display entities
            for (int i = 0; i < 5; i++)
            {
                Entity entity = CreateEntity();
                ActualOrgService.Create(entity);
            }

            //Retrieve entities
            List<Entity> createdEntities = ActualOrgService.RetrieveAll<Entity>(EntitySchemaName, new ColumnSet(FieldSchemaName));
            var fourthEntity = createdEntities.OrderBy(e => e.GetAttributeValue<string>(FieldSchemaName)).Skip(3).Take(1).FirstOrDefault();

            var lastCreatedEntity = createdEntities.OrderBy(e => e.GetAttributeValue<string>(FieldSchemaName)).LastOrDefault();

            var expectedAutoNumber = lastCreatedEntity.GetAttributeValue<string>(FieldSchemaName);

            //Act
            //Delete fourth entity
            ActualOrgService.Delete(EntitySchemaName, fourthEntity.Id);

            //Retrieve entities after delete
            List<Entity> entities = ActualOrgService.RetrieveAll<Entity>(EntitySchemaName, new ColumnSet(FieldSchemaName));
            var lastEntity = entities.OrderBy(e => e.GetAttributeValue<string>(FieldSchemaName)).LastOrDefault();
            var actualAutoNumber = lastEntity.GetAttributeValue<string>(FieldSchemaName);

            //Assert
            Assert.AreEqual(expectedAutoNumber, actualAutoNumber);
        }


        [Test]
        public void Delete_RearrangeAutoNumberInThreadWithoutDuplicates()
        {
            //Arrange
            DeleteEntities();

            ConfigureRearrange(true);

            //Create entities
            for (int i = 0; i < 10; i++)
            {
                Entity entity = CreateEntity();
                ActualOrgService.Create(entity);
            }

            //Retrieve created entities
            List<Entity> createdEntities = ActualOrgService
                .RetrieveAll<Entity>(EntitySchemaName, new ColumnSet(FieldSchemaName))
                .OrderBy(e => e.GetAttributeValue<string>(FieldSchemaName))
                .ToList();

            //Delete half of created entities
            Thread[] threads = new Thread[5];
            for (int i = 0; i < threads.Length; i++)
            {
                var entityId = createdEntities[i].Id;
                threads[i] = new Thread(() => ActualOrgService.Delete(EntitySchemaName, entityId));
            }

            foreach (Thread thread in threads)
            {
                thread.Start();
            }

            //Act
            //Validate duplicate of rearranged Auto-Number
            var updatedRearrangedEntities = ActualOrgService.RetrieveAll<Entity>(EntitySchemaName, new ColumnSet(FieldSchemaName));

            bool isDuplicate = updatedRearrangedEntities.GroupBy(e => e.Attributes[FieldSchemaName]).Any(a => a.Count() > 1);

            //Assert
            Assert.IsFalse(isDuplicate);
        }

        [TestCase(null, null)]
        [TestCase(null, "asdfgh")]
        public void UpdateAutoNumber_ThrowException_WhenNoValidParams(Entity displayEntity, string autoNumber)
        {
            void actual() => AutoNumberManager.UpdateAutoNumber(ActualOrgService, displayEntity, FieldSchemaName, autoNumber);

            Assert.Throws<PluginException>(actual);
        }
    }
}
