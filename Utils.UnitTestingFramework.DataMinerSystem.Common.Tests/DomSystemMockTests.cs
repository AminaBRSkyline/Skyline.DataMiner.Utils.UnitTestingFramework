namespace Skyline.DataMiner.Utils.UnitTestingFramework.DataMinerSystem.Common.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.Net;
    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.Net.Messages;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.Net.Sections;
    using Skyline.DataMiner.Utils.DOM.Builders;

    [TestClass]
    public class DomSystemMockTests
    {
        private const string ModuleId = "module";

        [TestMethod]
        public void DomBehaviorDefinitions_PersistChanges_WithCreateUpdateDelete()
        {
            // Arrange
            var helper = CreateHelper();
            var definitionId = Guid.NewGuid();
            var createdDefinition = new DomBehaviorDefinitionBuilder()
                .WithID(definitionId)
                .WithName("Created")
                .Build();

            // Act and assert
            helper.DomBehaviorDefinitions.Create(createdDefinition);
            Assert.AreEqual("Created", ReadBehaviorDefinition(helper, definitionId).Name);

            var updatedDefinition = new DomBehaviorDefinitionBuilder()
                .WithID(definitionId)
                .WithName("Updated")
                .Build();
            helper.DomBehaviorDefinitions.Update(updatedDefinition);
            Assert.AreEqual("Updated", ReadBehaviorDefinition(helper, definitionId).Name);

            helper.DomBehaviorDefinitions.Delete(updatedDefinition);
            Assert.IsFalse(helper.DomBehaviorDefinitions.Read(DomBehaviorDefinitionExposers.Id.Equal(definitionId)).Any());
        }

        [TestMethod]
        public void DomDefinitions_PersistChanges_WithCreateUpdateDelete()
        {
            // Arrange
            var helper = CreateHelper();
            var definitionId = Guid.NewGuid();
            var createdDefinition = CreateDefinition(definitionId, "Created");

            // Act and assert
            helper.DomDefinitions.Create(createdDefinition);
            Assert.AreEqual("Created", ReadDefinition(helper, definitionId).Name);

            var updatedDefinition = CreateDefinition(definitionId, "Updated");
            helper.DomDefinitions.Update(updatedDefinition);
            Assert.AreEqual("Updated", ReadDefinition(helper, definitionId).Name);

            helper.DomDefinitions.Delete(updatedDefinition);
            Assert.IsFalse(helper.DomDefinitions.Read(DomDefinitionExposers.Id.Equal(definitionId)).Any());
        }

        [TestMethod]
        public void DomInstances_PersistChanges_WithCreateUpdateDelete()
        {
            // Arrange
            var helper = CreateHelper();
            var definition = CreateDefinition(Guid.NewGuid(), "Definition");
            var instanceId = Guid.NewGuid();
            var createdInstance = new DomInstanceBuilder(definition)
                .WithID(instanceId)
                .Build();

            // Act and assert
            helper.DomInstances.Create(createdInstance);
            Assert.AreEqual(instanceId, ReadInstance(helper, instanceId).ID.Id);

            var updatedInstance = new DomInstanceBuilder(definition)
                .WithID(instanceId)
                .Build();
            helper.DomInstances.Update(updatedInstance);
            Assert.AreEqual(instanceId, ReadInstance(helper, instanceId).ID.Id);

            helper.DomInstances.Delete(updatedInstance);
            Assert.IsFalse(helper.DomInstances.Read(DomInstanceExposers.Id.Equal(instanceId)).Any());
        }

        [TestMethod]
        public void DomInstances_RaiseChangeNotifications_WithCreateUpdateDelete()
        {
            // Arrange
            var dmsMock = new IDmsMock();
            var helper = new DomHelper(dmsMock.Connection.Object.HandleMessages, ModuleId);
            var definition = CreateDefinition(Guid.NewGuid(), "Definition");
            var instance = new DomInstanceBuilder(definition).WithID(Guid.NewGuid()).Build();
            var notifications = new List<DomInstancesChangedEventMessage>();

            dmsMock.Connection.Object.OnNewMessage += (sender, args) =>
            {
                if (args.Message is DomInstancesChangedEventMessage message)
                {
                    notifications.Add(message);
                }
            };
            dmsMock.Connection.Object.AddSubscription(
                "DOM changes",
                new SubscriptionFilter[] { new SubscriptionFilterElement(typeof(DomInstancesChangedEventMessage), -1, -1) });

            // Act
            helper.DomInstances.Create(instance);
            helper.DomInstances.Update(instance);
            helper.DomInstances.Delete(instance);

            // Assert
            Assert.AreEqual(3, notifications.Count);
            Assert.AreEqual(instance.ID.Id, notifications[0].Created.Single().ID.Id);
            Assert.AreEqual(instance.ID.Id, notifications[1].Updated.Single().ID.Id);
            Assert.AreEqual(instance.ID.Id, notifications[2].Deleted.Single().ID.Id);
        }

        [TestMethod]
        public void HandleMessages_ThrowsArgumentNullException_WithNullMessage()
        {
            var domSystemMock = new DomSystemMock(_ => { });

            Assert.ThrowsExactly<ArgumentNullException>(() => domSystemMock.HandleMessages(new DMSMessage[] { null }));
        }

        [TestMethod]
        public void HandleMessages_ThrowsArgumentNullException_WithNullMessages()
        {
            var domSystemMock = new DomSystemMock(_ => { });

            Assert.ThrowsExactly<ArgumentNullException>(() => domSystemMock.HandleMessages(null));
        }

        [TestMethod]
        public void HandleMessages_ThrowsNotSupportedException_WithUnsupportedMessage()
        {
            var domSystemMock = new DomSystemMock(_ => { });

            Assert.ThrowsExactly<NotSupportedException>(() => domSystemMock.HandleMessages(new DMSMessage[] { new UnsupportedMessage() }));
        }

        [TestMethod]
        public void SectionDefinitions_PersistChanges_WithCreateUpdateDelete()
        {
            // Arrange
            var helper = CreateHelper();
            var definitionId = Guid.NewGuid();
            var createdDefinition = new SectionDefinitionBuilder()
                .WithID(definitionId)
                .WithName("Created")
                .Build();

            // Act and assert
            helper.SectionDefinitions.Create(createdDefinition);
            Assert.AreEqual("Created", ((CustomSectionDefinition)ReadSectionDefinition(helper, definitionId)).Name);

            var updatedDefinition = new SectionDefinitionBuilder()
                .WithID(definitionId)
                .WithName("Updated")
                .Build();
            helper.SectionDefinitions.Update(updatedDefinition);
            Assert.AreEqual("Updated", ((CustomSectionDefinition)ReadSectionDefinition(helper, definitionId)).Name);

            helper.SectionDefinitions.Delete(updatedDefinition);
            Assert.IsFalse(helper.SectionDefinitions.Read(SectionDefinitionExposers.ID.Equal(definitionId)).Any());
        }

        private static DomHelper CreateHelper()
        {
            var dmsMock = new IDmsMock();
            return new DomHelper(dmsMock.Connection.Object.HandleMessages, ModuleId);
        }

        private static DomDefinition CreateDefinition(Guid id, string name)
        {
            return new DomDefinitionBuilder()
                .WithID(id)
                .WithName(name)
                .Build();
        }

        private static DomBehaviorDefinition ReadBehaviorDefinition(DomHelper helper, Guid id)
        {
            return helper.DomBehaviorDefinitions.Read(DomBehaviorDefinitionExposers.Id.Equal(id)).Single();
        }

        private static DomDefinition ReadDefinition(DomHelper helper, Guid id)
        {
            return helper.DomDefinitions.Read(DomDefinitionExposers.Id.Equal(id)).Single();
        }

        private static DomInstance ReadInstance(DomHelper helper, Guid id)
        {
            return helper.DomInstances.Read(DomInstanceExposers.Id.Equal(id)).Single();
        }

        private static SectionDefinition ReadSectionDefinition(DomHelper helper, Guid id)
        {
            return helper.SectionDefinitions.Read(SectionDefinitionExposers.ID.Equal(id)).Single();
        }

        private sealed class UnsupportedMessage : DMSMessage
        {
        }
    }
}
