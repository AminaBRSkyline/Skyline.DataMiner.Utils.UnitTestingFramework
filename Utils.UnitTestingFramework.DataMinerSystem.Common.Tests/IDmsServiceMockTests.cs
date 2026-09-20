namespace Skyline.DataMiner.Utils.UnitTestingFramework.DataMinerSystem.Common.Tests
{
    using System;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    using Skyline.DataMiner.Core.DataMinerSystem.Common;
    using Skyline.DataMiner.Core.DataMinerSystem.Common.Properties;

    [TestClass]
    public class IDmsServiceMockTests
    {
        [TestMethod]
        public void AddView_CreatesRelationshipOnBothSides_ExistingView()
        {
            // Arrange
            var dmsMock = new IDmsMock();
            var viewMock = dmsMock.CreateView(viewId: 10);
            var serviceMock = dmsMock.CreateAgent(agentId: 1).CreateService(serviceId: 2);

            // Act
            serviceMock.AddView(viewMock.Object.Id);

            // Assert
            Assert.AreSame(viewMock.Object, serviceMock.Object.Views.Single());
            Assert.AreSame(serviceMock.Object, viewMock.Object.Services.Single());
        }

        [TestMethod]
        public void AddView_DoesNotCreateDuplicateRelationship_SameViewTwice()
        {
            // Arrange
            var dmsMock = new IDmsMock();
            var viewMock = dmsMock.CreateView(viewId: 10);
            var serviceMock = dmsMock.CreateAgent(agentId: 1).CreateService(serviceId: 2);

            // Act
            serviceMock.AddView(viewMock.Object.Id);
            serviceMock.AddView(viewMock.Object.Id);

            // Assert
            Assert.AreEqual(1, serviceMock.Object.Views.Count);
            Assert.AreEqual(1, viewMock.Object.Services.Count);
        }

        [TestMethod]
        public void AddView_ThrowsServiceNotFoundException_DeletedService()
        {
            // Arrange
            var dmsMock = new IDmsMock();
            var viewMock = dmsMock.CreateView(viewId: 10);
            var serviceMock = dmsMock.CreateAgent(agentId: 1).CreateService(serviceId: 2);
            serviceMock.Object.Delete();

            // Act & Assert
            Assert.ThrowsExactly<ServiceNotFoundException>(() => serviceMock.AddView(viewMock.Object.Id));
        }

        [TestMethod]
        public void AddView_ThrowsViewNotFoundException_UnknownView()
        {
            // Arrange
            var serviceMock = new IDmsMock().CreateAgent(agentId: 1).CreateService(serviceId: 2);

            // Act & Assert
            Assert.ThrowsExactly<ViewNotFoundException>(() => serviceMock.AddView(10));
        }

        [TestMethod]
        public void AdvancedSettings_ReturnsProvidedInstance_CustomValue()
        {
            // Arrange
            var serviceMock = new IDmsMock().CreateAgent(agentId: 1).CreateService(serviceId: 2);
            var settingsMock = new Mock<IAdvancedServiceSettings>();
            serviceMock.AdvancedSettings = settingsMock.Object;

            // Act
            var settings = serviceMock.Object.AdvancedSettings;

            // Assert
            Assert.AreSame(settingsMock.Object, settings);
        }

        [TestMethod]
        public void Delete_RemovesServiceFromCacheAndRelationships_ExistingService()
        {
            // Arrange
            var dmsMock = new IDmsMock();
            var dmaMock = dmsMock.CreateAgent(agentId: 1);
            var viewMock = dmsMock.CreateView(viewId: 10);
            var serviceMock = dmaMock.CreateService(serviceId: 2, name: "Deleted Service");
            serviceMock.AddView(viewMock.Object.Id);

            // Act
            serviceMock.Object.Delete();

            // Assert
            Assert.IsFalse(dmsMock.Object.ServiceExists(new DmsServiceId(1, 2)));
            Assert.IsFalse(dmsMock.Object.ServiceExists("Deleted Service"));
            Assert.IsEmpty(dmaMock.Object.GetServices());
            Assert.IsEmpty(viewMock.Object.Services);
            Assert.ThrowsExactly<ServiceNotFoundException>(() => dmsMock.Object.GetService(new DmsServiceId(1, 2)));
        }

        [TestMethod]
        public void Delete_ThrowsServiceNotFoundException_DeletedService()
        {
            // Arrange
            var serviceMock = new IDmsMock().CreateAgent(agentId: 1).CreateService(serviceId: 2);
            serviceMock.Object.Delete();

            // Act & Assert
            Assert.ThrowsExactly<ServiceNotFoundException>(() => serviceMock.Object.Delete());
        }

        [TestMethod]
        public void Description_ReturnsEmptyString_DefaultValue()
        {
            var service = new IDmsMock().CreateAgent(1).CreateService(2).Object;

            Assert.AreEqual(String.Empty, service.Description);
        }

        [TestMethod]
        public void Description_ReturnsUpdatedValue_SetValue()
        {
            // Arrange
            var serviceMock = new IDmsMock().CreateAgent(agentId: 1).CreateService(serviceId: 2);

            // Act
            serviceMock.Object.Description = "Service Description";

            // Assert
            Assert.AreEqual("Service Description", serviceMock.Object.Description);
        }

        [TestMethod]
        public void Duplicate_CopiesConfigurationInstances_ValidTargetAgent()
        {
            // Arrange
            var dmsMock = new IDmsMock();
            var sourceAgentMock = dmsMock.CreateAgent(agentId: 1, name: "Source Agent");
            var targetAgentMock = dmsMock.CreateAgent(agentId: 2, name: "Target Agent");
            var sourceServiceMock = sourceAgentMock.CreateService(serviceId: 10, name: "Original Service");
            var advancedSettingsMock = new Mock<IAdvancedServiceSettings>();
            var parameterSettingsMock = new Mock<IServiceParamsSettings>();
            var propertiesMock = new Mock<IPropertyCollection<IDmsServiceProperty, IDmsServicePropertyDefinition>>();
            var replicationSettingsMock = new Mock<IReplicationServiceSettings>();
            sourceServiceMock.AdvancedSettings = advancedSettingsMock.Object;
            sourceServiceMock.ParameterSettings = parameterSettingsMock.Object;
            sourceServiceMock.Properties = propertiesMock.Object;
            sourceServiceMock.ReplicationSettings = replicationSettingsMock.Object;

            // Act
            var duplicate = sourceServiceMock.Object.Duplicate("Duplicated Service", targetAgentMock.Object);

            // Assert
            Assert.AreSame(advancedSettingsMock.Object, duplicate.AdvancedSettings);
            Assert.AreSame(parameterSettingsMock.Object, duplicate.ParameterSettings);
            Assert.AreSame(propertiesMock.Object, duplicate.Properties);
            Assert.AreSame(replicationSettingsMock.Object, duplicate.ReplicationSettings);
        }

        [TestMethod]
        public void Duplicate_CreatesServiceOnTargetAgent_ValidTargetAgent()
        {
            // Arrange
            var dmsMock = new IDmsMock();
            var sourceAgentMock = dmsMock.CreateAgent(agentId: 1, name: "Source Agent");
            var targetAgentMock = dmsMock.CreateAgent(agentId: 2, name: "Target Agent");
            var sourceServiceMock = sourceAgentMock.CreateService(serviceId: 10, name: "Original Service");
            sourceServiceMock.Description = "Description";

            // Act
            var duplicate = sourceServiceMock.Object.Duplicate("Duplicated Service", targetAgentMock.Object);

            // Assert
            Assert.AreSame(duplicate, targetAgentMock.Object.GetService("Duplicated Service"));
            Assert.AreSame(targetAgentMock.Object, duplicate.Host);
            Assert.AreEqual(2, duplicate.AgentId);
            Assert.AreEqual("Description", duplicate.Description);
        }

        [TestMethod]
        public void Duplicate_ThrowsAgentNotFoundException_UnknownAgent()
        {
            // Arrange
            var sourceServiceMock = new IDmsMock().CreateAgent(agentId: 1).CreateService(serviceId: 2);
            var unknownAgentMock = new Mock<IDma>();
            unknownAgentMock.Setup(agent => agent.Id).Returns(2);

            // Act & Assert
            Assert.ThrowsExactly<AgentNotFoundException>(() => sourceServiceMock.Object.Duplicate("Duplicated Service", unknownAgentMock.Object));
        }

        [TestMethod]
        public void Duplicate_ThrowsArgumentException_InvalidName()
        {
            // Arrange
            var dmsMock = new IDmsMock();
            var sourceServiceMock = dmsMock.CreateAgent(agentId: 1).CreateService(serviceId: 2);
            var targetAgentMock = dmsMock.CreateAgent(agentId: 2, name: "Target Agent");

            // Act & Assert
            Assert.ThrowsExactly<ArgumentException>(() => sourceServiceMock.Object.Duplicate("Invalid|Service", targetAgentMock.Object));
        }

        [TestMethod]
        public void Duplicate_ThrowsArgumentNullException_NullAgent()
        {
            // Arrange
            var sourceServiceMock = new IDmsMock().CreateAgent(agentId: 1).CreateService(serviceId: 2);

            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => sourceServiceMock.Object.Duplicate("Duplicated Service", null));
        }

        [TestMethod]
        public void Duplicate_ThrowsArgumentNullException_NullName()
        {
            // Arrange
            var dmsMock = new IDmsMock();
            var sourceServiceMock = dmsMock.CreateAgent(agentId: 1).CreateService(serviceId: 2);
            var targetAgentMock = dmsMock.CreateAgent(agentId: 2, name: "Target Agent");

            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => sourceServiceMock.Object.Duplicate(null, targetAgentMock.Object));
        }

        [TestMethod]
        public void Duplicate_ThrowsServiceNotFoundException_DeletedService()
        {
            // Arrange
            var dmsMock = new IDmsMock();
            var sourceServiceMock = dmsMock.CreateAgent(agentId: 1).CreateService(serviceId: 2);
            var targetAgentMock = dmsMock.CreateAgent(agentId: 2, name: "Target Agent");
            sourceServiceMock.Object.Delete();

            // Act & Assert
            Assert.ThrowsExactly<ServiceNotFoundException>(() => sourceServiceMock.Object.Duplicate("Duplicated Service", targetAgentMock.Object));
        }

        [TestMethod]
        public void Exists_ReturnsFalse_ServiceIsDeleted()
        {
            // Arrange
            var serviceMock = new IDmsMock().CreateAgent(agentId: 1).CreateService(serviceId: 2);

            // Act & Assert
            Assert.IsTrue(serviceMock.Object.Exists());

            serviceMock.Object.Delete();

            Assert.IsFalse(serviceMock.Object.Exists());
        }

        [TestMethod]
        public void GetState_ReturnsProvidedInstance_CustomValue()
        {
            // Arrange
            var serviceMock = new IDmsMock().CreateAgent(agentId: 1).CreateService(serviceId: 2);
            var stateMock = new Mock<IServiceState>();
            serviceMock.State = stateMock.Object;

            // Act
            var state = serviceMock.Object.GetState();

            // Assert
            Assert.AreSame(stateMock.Object, state);
        }

        [TestMethod]
        public void GetState_ThrowsServiceNotFoundException_DeletedService()
        {
            // Arrange
            var serviceMock = new IDmsMock().CreateAgent(agentId: 1).CreateService(serviceId: 2);
            serviceMock.Object.Delete();

            // Act & Assert
            Assert.ThrowsExactly<ServiceNotFoundException>(() => serviceMock.Object.GetState());
        }

        [TestMethod]
        public void Identity_ReturnsConfiguredValues_CustomValues()
        {
            // Arrange
            var dmaMock = new IDmsMock().CreateAgent(agentId: 1);
            var serviceMock = dmaMock.CreateService(serviceId: 2, name: "Main Service");

            // Act
            var service = serviceMock.Object;

            // Assert
            Assert.AreEqual(2, service.Id);
            Assert.AreEqual(1, service.AgentId);
            Assert.AreEqual(new DmsServiceId(1, 2), service.DmsServiceId);
            Assert.AreEqual("Main Service", service.Name);
            Assert.AreSame(dmaMock.Object, service.Host);
        }

        [TestMethod]
        public void Name_ReturnsUpdatedValue_SetTwoHundredCharacterValue()
        {
            // Arrange
            var serviceMock = new IDmsMock().CreateAgent(agentId: 1).CreateService(serviceId: 2);
            var validName = new string('a', 200);

            // Act
            serviceMock.Object.Name = validName;

            // Assert
            Assert.AreEqual(validName, serviceMock.Object.Name);
        }

        [TestMethod]
        public void Name_ReturnsUpdatedValue_SetValueWithOnePercentageCharacter()
        {
            // Arrange
            var serviceMock = new IDmsMock().CreateAgent(agentId: 1).CreateService(serviceId: 2);

            // Act
            serviceMock.Object.Name = "Service%Name";

            // Assert
            Assert.AreEqual("Service%Name", serviceMock.Object.Name);
        }

        [TestMethod]
        public void Name_ReturnsUpdatedValueAndUpdatesDmsLookup_SetValue()
        {
            // Arrange
            var dmsMock = new IDmsMock();
            var serviceMock = dmsMock.CreateAgent(agentId: 1).CreateService(serviceId: 2, name: "Original Service");

            // Act
            serviceMock.Object.Name = "Renamed Service";

            // Assert
            Assert.AreEqual("Renamed Service", serviceMock.Object.Name);
            Assert.IsFalse(dmsMock.Object.ServiceExists("Original Service"));
            Assert.AreSame(serviceMock.Object, dmsMock.Object.GetService("Renamed Service"));
        }

        [TestMethod]
        public void Name_ThrowsArgumentException_SetValueLongerThanTwoHundredCharacters()
        {
            // Arrange
            var serviceMock = new IDmsMock().CreateAgent(agentId: 1).CreateService(serviceId: 2);

            // Act & Assert
            Assert.ThrowsExactly<ArgumentException>(() => serviceMock.Object.Name = new string('a', 201));
        }

        [TestMethod]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow(".Service")]
        [DataRow("Service.")]
        [DataRow(" Service")]
        [DataRow("Service ")]
        [DataRow("Service|Name")]
        [DataRow("Service%Name%Again")]
        public void Name_ThrowsArgumentExceptionAndKeepsPreviousValue_SetInvalidValue(string invalidName)
        {
            // Arrange
            var serviceMock = new IDmsMock().CreateAgent(agentId: 1).CreateService(serviceId: 2, name: "Original Service");

            // Act & Assert
            Assert.ThrowsExactly<ArgumentException>(() => serviceMock.Object.Name = invalidName);
            Assert.AreEqual("Original Service", serviceMock.Object.Name);
        }

        [TestMethod]
        public void Name_ThrowsArgumentExceptionAndKeepsPreviousValue_SetToExistingName()
        {
            // Arrange
            var dmaMock = new IDmsMock().CreateAgent(agentId: 1);
            dmaMock.CreateService(serviceId: 2, name: "Existing Service");
            var serviceMock = dmaMock.CreateService(serviceId: 3, name: "Other Service");

            // Act & Assert
            Assert.ThrowsExactly<ArgumentException>(() => serviceMock.Object.Name = "existing service");
            Assert.AreEqual("Other Service", serviceMock.Object.Name);
        }

        [TestMethod]
        public void Name_ThrowsArgumentNullExceptionAndKeepsPreviousValue_SetNull()
        {
            // Arrange
            var serviceMock = new IDmsMock().CreateAgent(agentId: 1).CreateService(serviceId: 2, name: "Original Service");

            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => serviceMock.Object.Name = null);
            Assert.AreEqual("Original Service", serviceMock.Object.Name);
        }

        [TestMethod]
        public void ParameterSettings_ReturnsProvidedInstance_CustomValue()
        {
            // Arrange
            var serviceMock = new IDmsMock().CreateAgent(agentId: 1).CreateService(serviceId: 2);
            var settingsMock = new Mock<IServiceParamsSettings>();
            serviceMock.ParameterSettings = settingsMock.Object;

            // Act
            var settings = serviceMock.Object.ParameterSettings;

            // Assert
            Assert.AreSame(settingsMock.Object, settings);
        }

        [TestMethod]
        public void Properties_ReturnsEmptyCollection_DefaultValue()
        {
            // Arrange
            var serviceMock = new IDmsMock().CreateAgent(agentId: 1).CreateService(serviceId: 2);

            // Act
            var properties = serviceMock.Object.Properties;

            // Assert
            Assert.IsNotNull(properties);
            Assert.AreEqual(0, properties.Count);
        }

        [TestMethod]
        public void Properties_ReturnsProvidedInstance_CustomValue()
        {
            // Arrange
            var serviceMock = new IDmsMock().CreateAgent(agentId: 1).CreateService(serviceId: 2);
            var propertiesMock = new Mock<IPropertyCollection<IDmsServiceProperty, IDmsServicePropertyDefinition>>();
            serviceMock.Properties = propertiesMock.Object;

            // Act
            var properties = serviceMock.Object.Properties;

            // Assert
            Assert.AreSame(propertiesMock.Object, properties);
        }

        [TestMethod]
        public void ReplicationSettings_ReturnsProvidedInstance_CustomValue()
        {
            // Arrange
            var serviceMock = new IDmsMock().CreateAgent(agentId: 1).CreateService(serviceId: 2);
            var settingsMock = new Mock<IReplicationServiceSettings>();
            serviceMock.ReplicationSettings = settingsMock.Object;

            // Act
            var settings = serviceMock.Object.ReplicationSettings;

            // Assert
            Assert.AreSame(settingsMock.Object, settings);
        }

        [TestMethod]
        public void Settings_MultipleServices_ReturnIndependentInstances()
        {
            // Arrange
            var dmaMock = new IDmsMock().CreateAgent(agentId: 1);
            var firstServiceMock = dmaMock.CreateService(serviceId: 2, name: "First Service");
            var secondServiceMock = dmaMock.CreateService(serviceId: 3, name: "Second Service");

            // Act & Assert
            Assert.AreNotSame(firstServiceMock.Object.AdvancedSettings, secondServiceMock.Object.AdvancedSettings);
            Assert.AreNotSame(firstServiceMock.Object.ParameterSettings, secondServiceMock.Object.ParameterSettings);
            Assert.AreNotSame(firstServiceMock.Object.ReplicationSettings, secondServiceMock.Object.ReplicationSettings);
        }

        [TestMethod]
        public void Settings_ReturnStableNonNullInstances_DefaultValues()
        {
            // Arrange
            var serviceMock = new IDmsMock().CreateAgent(agentId: 1).CreateService(serviceId: 2);

            // Act
            var firstAdvancedSettings = serviceMock.Object.AdvancedSettings;
            var secondAdvancedSettings = serviceMock.Object.AdvancedSettings;
            var firstParameterSettings = serviceMock.Object.ParameterSettings;
            var secondParameterSettings = serviceMock.Object.ParameterSettings;
            var firstReplicationSettings = serviceMock.Object.ReplicationSettings;
            var secondReplicationSettings = serviceMock.Object.ReplicationSettings;

            // Assert
            Assert.IsNotNull(firstAdvancedSettings);
            Assert.AreSame(firstAdvancedSettings, secondAdvancedSettings);
            Assert.IsNotNull(firstParameterSettings);
            Assert.AreSame(firstParameterSettings, secondParameterSettings);
            Assert.IsNotNull(firstReplicationSettings);
            Assert.AreSame(firstReplicationSettings, secondReplicationSettings);
        }

        [TestMethod]
        public void Update_DoesNotThrowException_ExistingService()
        {
            // Arrange
            var serviceMock = new IDmsMock().CreateAgent(agentId: 1).CreateService(serviceId: 2);

            // Act
            serviceMock.Object.Update();

            // Assert
            Assert.IsTrue(serviceMock.Object.Exists());
        }

        [TestMethod]
        public void Update_ThrowsServiceNotFoundException_DeletedService()
        {
            // Arrange
            var serviceMock = new IDmsMock().CreateAgent(agentId: 1).CreateService(serviceId: 2);
            serviceMock.Object.Delete();

            // Act & Assert
            Assert.ThrowsExactly<ServiceNotFoundException>(() => serviceMock.Object.Update());
        }

        [TestMethod]
        public void Views_RemovesViewFromService_AssignedViewIsDeleted()
        {
            // Arrange
            var dmsMock = new IDmsMock();
            var viewMock = dmsMock.CreateView(viewId: 10);
            var serviceMock = dmsMock.CreateAgent(agentId: 1).CreateService(serviceId: 2);
            serviceMock.AddView(viewMock.Object.Id);

            // Act
            viewMock.Object.Delete();

            // Assert
            Assert.IsEmpty(serviceMock.Object.Views);
        }

        [TestMethod]
        public void Views_ReturnsEmptyCollection_DefaultValue()
        {
            // Arrange
            var serviceMock = new IDmsMock().CreateAgent(agentId: 1).CreateService(serviceId: 2);

            // Act
            var views = serviceMock.Object.Views;

            // Assert
            Assert.IsEmpty(views);
        }
    }
}
