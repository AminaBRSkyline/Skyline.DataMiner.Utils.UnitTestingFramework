namespace Skyline.DataMiner.Utils.UnitTestingFramework.DataMinerSystem.Common.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    using Skyline.DataMiner.Core.DataMinerSystem.Common;
    using Skyline.DataMiner.Core.DataMinerSystem.Common.Properties;

    [TestClass]
    [DeploymentItem("TestFiles/Model/Data/protocol.xml")]
    public class IDmsViewMockTests
    {
        private readonly string path = "protocol.xml";

        [TestMethod]
        public void AlarmLevel_ThrowsArgumentOutOfRangeException_InvalidValue()
        {
            // Arrange
            var viewMock = new IDmsMock().CreateView(viewId: 10);

            // Act & Assert
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => viewMock.AlarmLevel = (AlarmLevel)999);
            Assert.AreEqual(AlarmLevel.Undefined, viewMock.Object.GetAlarmLevel());
        }

        [TestMethod]
        public void ChildViews_IsReadOnly_ReturnedCollection()
        {
            // Arrange
            var dmsMock = new IDmsMock();
            var parentMock = dmsMock.CreateView(viewId: 10, name: "Parent");
            var childMock = dmsMock.CreateView(viewId: 11, name: "Child");
            childMock.Parent = parentMock.Object;

            // Act
            var childViews = parentMock.Object.ChildViews;

            // Assert
            Assert.ThrowsExactly<NotSupportedException>(() => childViews.Add(childMock.Object));
        }

        [TestMethod]
        public void ChildViews_ReturnsOnlyImmediateChildren_MultipleLevels()
        {
            // Arrange
            var dmsMock = new IDmsMock();
            var parentMock = dmsMock.CreateView(viewId: 10, name: "Parent");
            var childMock = dmsMock.CreateView(viewId: 11, name: "Child");
            var grandchildMock = dmsMock.CreateView(viewId: 12, name: "Grandchild");
            childMock.Parent = parentMock.Object;
            grandchildMock.Parent = childMock.Object;

            // Act
            var childViews = parentMock.Object.ChildViews;

            // Assert
            Assert.AreEqual(1, childViews.Count);
            Assert.AreSame(childMock.Object, childViews.Single());
        }

        [TestMethod]
        public void CreateView_ThrowsArgumentExceptionAndDoesNotAddView_InvalidName()
        {
            // Arrange
            var dmsMock = new IDmsMock();

            // Act & Assert
            Assert.ThrowsExactly<ArgumentException>(() => dmsMock.CreateView(viewId: 10, name: "Invalid|View"));
            Assert.IsFalse(dmsMock.Object.ViewExists(10));
            Assert.IsEmpty(dmsMock.Object.GetViews());
        }

        [TestMethod]
        public void CreateView_ThrowsArgumentExceptionAndKeepsExistingView_DuplicateName()
        {
            // Arrange
            var dmsMock = new IDmsMock();
            var existingViewMock = dmsMock.CreateView(viewId: 10, name: "Existing View");

            // Act & Assert
            Assert.ThrowsExactly<ArgumentException>(() => dmsMock.CreateView(viewId: 11, name: "Existing View"));
            Assert.AreSame(existingViewMock.Object, dmsMock.Object.GetView("Existing View"));
            Assert.AreEqual(1, dmsMock.Object.GetViews().Count);
        }

        [TestMethod]
        public void CreateView_ThrowsArgumentExceptionAndKeepsExistingView_DuplicateNameWithDifferentCasing()
        {
            // Arrange
            var dmsMock = new IDmsMock();
            var existingViewMock = dmsMock.CreateView(viewId: 10, name: "Existing View");

            // Act & Assert
            Assert.ThrowsExactly<ArgumentException>(() => dmsMock.CreateView(viewId: 11, name: "existing view"));
            Assert.AreSame(existingViewMock.Object, dmsMock.Object.GetView("EXISTING VIEW"));
            Assert.AreEqual(1, dmsMock.Object.GetViews().Count);
        }

        [TestMethod]
        public void Delete_DoesNotThrowException_DeletedView()
        {
            // Arrange
            var dmsMock = new IDmsMock();
            var viewMock = dmsMock.CreateView(viewId: 10);
            viewMock.Object.Delete();

            // Act
            viewMock.Object.Delete();

            // Assert
            Assert.IsFalse(dmsMock.Object.ViewExists(10));
        }

        [TestMethod]
        public void Delete_RemovesViewFromDms_ExistingView()
        {
            // Arrange
            var dmsMock = new IDmsMock();
            var viewMock = dmsMock.CreateView(viewId: 10, name: "Deleted View");

            // Act
            viewMock.Object.Delete();

            // Assert
            Assert.IsFalse(dmsMock.Object.ViewExists(10));
            Assert.IsFalse(dmsMock.Object.ViewExists("Deleted View"));
            Assert.IsEmpty(dmsMock.Object.GetViews());
            Assert.ThrowsExactly<ViewNotFoundException>(() => dmsMock.Object.GetView(10));
            Assert.ThrowsExactly<ViewNotFoundException>(() => dmsMock.Object.GetView("Deleted View"));
        }

        [TestMethod]
        public void Delete_RemovesViewFromElement_ViewContainingElement()
        {
            // Arrange
            var dmsMock = new IDmsMock();
            var viewMock = dmsMock.CreateView(viewId: 10);
            var elementMock = dmsMock.CreateAgent(agentId: 1).CreateElement(path, id: 2);
            elementMock.AddView(viewMock.Object.Id);

            // Act
            viewMock.Object.Delete();

            // Assert
            Assert.IsFalse(elementMock.Object.Views.Contains(viewMock.Object));
        }

        [TestMethod]
        public void Delete_RemovesViewFromParentChildViews_ChildView()
        {
            // Arrange
            var dmsMock = new IDmsMock();
            var parentMock = dmsMock.CreateView(viewId: 10, name: "Parent");
            var childMock = dmsMock.CreateView(viewId: 11, name: "Child");
            childMock.Parent = parentMock.Object;

            // Act
            childMock.Object.Delete();

            // Assert
            Assert.IsEmpty(parentMock.Object.ChildViews);
        }

        [TestMethod]
        public void Display_ReturnsEmptyString_DefaultValue()
        {
            // Arrange
            var viewMock = new IDmsMock().CreateView(viewId: 10);

            // Act
            var display = viewMock.Object.Display;

            // Assert
            Assert.AreEqual(String.Empty, display);
        }

        [TestMethod]
        public void Display_ReturnsProvidedValue_CustomValue()
        {
            // Arrange
            var viewMock = new IDmsMock().CreateView(viewId: 10);
            viewMock.Display = "Custom Display";

            // Act
            var display = viewMock.Object.Display;

            // Assert
            Assert.AreEqual("Custom Display", display);
        }

        [TestMethod]
        public void Elements_IsReadOnly_ReturnedCollection()
        {
            // Arrange
            var dmsMock = new IDmsMock();
            var viewMock = dmsMock.CreateView(viewId: 10);
            var elementMock = dmsMock.CreateAgent(agentId: 1).CreateElement(path, id: 2);

            // Act
            var elements = viewMock.Object.Elements;

            // Assert
            Assert.ThrowsExactly<NotSupportedException>(() => elements.Add(elementMock.Object));
        }

        [TestMethod]
        public void Elements_RemovesElementFromView_AssignedElementIsDeleted()
        {
            // Arrange
            var dmsMock = new IDmsMock();
            var viewMock = dmsMock.CreateView(viewId: 10);
            var elementMock = dmsMock.CreateAgent(agentId: 1).CreateElement(path, id: 2);
            elementMock.AddView(viewMock.Object.Id);

            // Act
            elementMock.Object.Delete();

            // Assert
            Assert.IsEmpty(viewMock.Object.Elements);
        }

        [TestMethod]
        public void Elements_ReturnsElement_ElementContainsView()
        {
            // Arrange
            var dmsMock = new IDmsMock();
            var viewMock = dmsMock.CreateView(viewId: 10);
            var elementMock = dmsMock.CreateAgent(agentId: 1).CreateElement(path, id: 2);
            elementMock.AddView(viewMock.Object.Id);

            // Act
            var elements = viewMock.Object.Elements;

            // Assert
            Assert.HasCount(1, elements);
            Assert.AreSame(elementMock.Object, elements.Single());
            Assert.IsTrue(elementMock.Object.Views.Contains(viewMock.Object));
        }

        [TestMethod]
        public void Elements_ReturnsElementForEachView_ElementBelongsToMultipleViews()
        {
            // Arrange
            var dmsMock = new IDmsMock();
            var firstViewMock = dmsMock.CreateView(viewId: 10, name: "First View");
            var secondViewMock = dmsMock.CreateView(viewId: 11, name: "Second View");
            var elementMock = dmsMock.CreateAgent(agentId: 1).CreateElement(path, id: 2);
            elementMock.AddView(firstViewMock.Object.Id);
            elementMock.AddView(secondViewMock.Object.Id);

            // Act & Assert
            Assert.AreSame(elementMock.Object, firstViewMock.Object.Elements.Single());
            Assert.AreSame(elementMock.Object, secondViewMock.Object.Elements.Single());
        }

        [TestMethod]
        public void Elements_ReturnsEmptyCollection_DefaultValue()
        {
            // Arrange
            var viewMock = new IDmsMock().CreateView(viewId: 10);

            // Act
            var elements = viewMock.Object.Elements;

            // Assert
            Assert.IsEmpty(elements);
        }

        [TestMethod]
        public void Exists_ReturnsFalse_ViewIsDeleted()
        {
            // Arrange
            var viewMock = new IDmsMock().CreateView(viewId: 10);

            // Act & Assert
            Assert.IsTrue(viewMock.Object.Exists());

            viewMock.Object.Delete();

            Assert.IsFalse(viewMock.Object.Exists());
        }

        [TestMethod]
        [DataRow(AlarmLevel.Normal)]
        [DataRow(AlarmLevel.Warning)]
        [DataRow(AlarmLevel.Minor)]
        [DataRow(AlarmLevel.Major)]
        [DataRow(AlarmLevel.Critical)]
        [DataRow(AlarmLevel.Timeout)]
        [DataRow(AlarmLevel.Information)]
        [DataRow(AlarmLevel.Initial)]
        [DataRow(AlarmLevel.Masked)]
        [DataRow(AlarmLevel.Error)]
        [DataRow(AlarmLevel.Notice)]
        [DataRow(AlarmLevel.Suggestion)]
        public void GetAlarmLevel_ReturnsProvidedValue_CustomValue(AlarmLevel expectedAlarmLevel)
        {
            // Arrange
            var viewMock = new IDmsMock().CreateView(viewId: 10);
            viewMock.AlarmLevel = expectedAlarmLevel;

            // Act
            var alarmLevel = viewMock.Object.GetAlarmLevel();

            // Assert
            Assert.AreEqual(expectedAlarmLevel, alarmLevel);
        }

        [TestMethod]
        public void GetAlarmLevel_ReturnsUndefined_DefaultValue()
        {
            // Arrange
            var viewMock = new IDmsMock().CreateView(viewId: 10);

            // Act
            var alarmLevel = viewMock.Object.GetAlarmLevel();

            // Assert
            Assert.AreEqual(AlarmLevel.Undefined, alarmLevel);
        }

        [TestMethod]
        public void Identity_ReturnsConfiguredValues_CustomValues()
        {
            var dmsMock = new IDmsMock();
            var view = dmsMock.CreateView(10, "Main View").Object;

            Assert.AreEqual(10, view.Id);
            Assert.AreEqual("Main View", view.Name);
            Assert.AreSame(dmsMock.Object, view.Dms);
        }

        [TestMethod]
        public void Name_ReturnsUpdatedValue_SetTwoHundredCharacterValue()
        {
            // Arrange
            var viewMock = new IDmsMock().CreateView(viewId: 10);
            var validName = new string('a', 200);

            // Act
            viewMock.Object.Name = validName;

            // Assert
            Assert.AreEqual(validName, viewMock.Object.Name);
        }

        [TestMethod]
        public void Name_ReturnsUpdatedValue_SetValueWithOnePercentageCharacter()
        {
            // Arrange
            var viewMock = new IDmsMock().CreateView(viewId: 10);

            // Act
            viewMock.Object.Name = "View%Name";

            // Assert
            Assert.AreEqual("View%Name", viewMock.Object.Name);
        }

        [TestMethod]
        public void Name_ReturnsUpdatedValueAndUpdatesDmsLookup_SetValue()
        {
            // Arrange
            var dmsMock = new IDmsMock();
            var viewMock = dmsMock.CreateView(viewId: 10, name: "Original View");

            // Act
            viewMock.Object.Name = "Renamed View";

            // Assert
            Assert.AreEqual("Renamed View", viewMock.Object.Name);
            Assert.IsFalse(dmsMock.Object.ViewExists("Original View"));
            Assert.AreSame(viewMock.Object, dmsMock.Object.GetView("Renamed View"));
        }

        [TestMethod]
        public void Name_ThrowsArgumentException_SetValueLongerThanTwoHundredCharacters()
        {
            // Arrange
            var viewMock = new IDmsMock().CreateView(viewId: 10);

            // Act & Assert
            Assert.ThrowsExactly<ArgumentException>(() => viewMock.Object.Name = new string('a', 201));
        }

        [TestMethod]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow(".View")]
        [DataRow("View.")]
        [DataRow(" View")]
        [DataRow("View ")]
        [DataRow("View|Name")]
        [DataRow("View%Name%Again")]
        public void Name_ThrowsArgumentExceptionAndKeepsPreviousValue_SetInvalidValue(string invalidName)
        {
            // Arrange
            var viewMock = new IDmsMock().CreateView(viewId: 10, name: "Original View");

            // Act & Assert
            Assert.ThrowsExactly<ArgumentException>(() => viewMock.Object.Name = invalidName);
            Assert.AreEqual("Original View", viewMock.Object.Name);
        }

        [TestMethod]
        public void Name_ThrowsArgumentExceptionAndKeepsPreviousValue_SetToExistingName()
        {
            // Arrange
            var dmsMock = new IDmsMock();
            dmsMock.CreateView(viewId: 10, name: "Existing View");
            var viewMock = dmsMock.CreateView(viewId: 11, name: "Other View");

            // Act & Assert
            Assert.ThrowsExactly<ArgumentException>(() => viewMock.Object.Name = "Existing View");
            Assert.AreEqual("Other View", viewMock.Object.Name);
        }

        [TestMethod]
        public void Name_ThrowsArgumentNullExceptionAndKeepsPreviousValue_SetNull()
        {
            // Arrange
            var viewMock = new IDmsMock().CreateView(viewId: 10, name: "Original View");

            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => viewMock.Object.Name = null);
            Assert.AreEqual("Original View", viewMock.Object.Name);
        }

        [TestMethod]
        public void Parent_ReturnsNullAndEmptyChildViews_DefaultValue()
        {
            // Arrange
            var viewMock = new IDmsMock().CreateView(viewId: 10);

            // Act & Assert
            Assert.IsNull(viewMock.Object.Parent);
            Assert.IsEmpty(viewMock.Object.ChildViews);
        }

        [TestMethod]
        public void Parent_ThrowsArgumentNullExceptionAndKeepsPreviousParent_SetNull()
        {
            // Arrange
            var dmsMock = new IDmsMock();
            var parentMock = dmsMock.CreateView(viewId: 10, name: "Parent");
            var childMock = dmsMock.CreateView(viewId: 11, name: "Child");
            childMock.Parent = parentMock.Object;

            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => childMock.Object.Parent = null);
            Assert.AreSame(parentMock.Object, childMock.Object.Parent);
        }

        [TestMethod]
        public void Parent_ThrowsNotSupportedException_SetOnRootView()
        {
            // Arrange
            var dmsMock = new IDmsMock();
            var rootMock = dmsMock.CreateView(viewId: -1, name: "Root View");
            var parentMock = dmsMock.CreateView(viewId: 10, name: "Parent");

            // Act & Assert
            Assert.ThrowsExactly<NotSupportedException>(() => rootMock.Object.Parent = parentMock.Object);
        }

        [TestMethod]
        public void Parent_ThrowsNotSupportedException_SetToDifferentReferenceWithSameId()
        {
            // Arrange
            var viewMock = new IDmsMock().CreateView(viewId: 10);
            var sameIdViewMock = new IDmsMock().CreateView(viewId: 10);

            // Act & Assert
            Assert.ThrowsExactly<NotSupportedException>(() => viewMock.Object.Parent = sameIdViewMock.Object);
        }

        [TestMethod]
        public void Parent_ThrowsNotSupportedException_SetToSelf()
        {
            // Arrange
            var viewMock = new IDmsMock().CreateView(viewId: 10);

            // Act & Assert
            Assert.ThrowsExactly<NotSupportedException>(() => viewMock.Object.Parent = viewMock.Object);
        }

        [TestMethod]
        public void Parent_UpdatesBothParentsChildViews_Changed()
        {
            // Arrange
            var dmsMock = new IDmsMock();
            var firstParentMock = dmsMock.CreateView(viewId: 10, name: "First Parent");
            var secondParentMock = dmsMock.CreateView(viewId: 11, name: "Second Parent");
            var childMock = dmsMock.CreateView(viewId: 12, name: "Child");
            childMock.Object.Parent = firstParentMock.Object;

            // Act
            childMock.Object.Parent = secondParentMock.Object;

            // Assert
            Assert.IsEmpty(firstParentMock.Object.ChildViews);
            Assert.AreSame(childMock.Object, secondParentMock.Object.ChildViews.Single());
        }

        [TestMethod]
        public void Parent_UpdatesParentAndChildViews_SetToView()
        {
            // Arrange
            var dmsMock = new IDmsMock();
            var parentMock = dmsMock.CreateView(viewId: 10, name: "Parent");
            var childMock = dmsMock.CreateView(viewId: 11, name: "Child");

            // Act
            childMock.Object.Parent = parentMock.Object;

            // Assert
            Assert.AreSame(parentMock.Object, childMock.Object.Parent);
            Assert.AreSame(childMock.Object, parentMock.Object.ChildViews.Single());
            Assert.IsEmpty(childMock.Object.ChildViews);
        }

        [TestMethod]
        public void Properties_ReturnsProvidedCollection_CustomValue()
        {
            // Arrange
            var viewMock = new IDmsMock().CreateView(viewId: 10);
            var expectedProperties = new Mock<IPropertyCollection<IDmsViewProperty, IDmsViewPropertyDefinition>>().Object;
            viewMock.Properties = expectedProperties;

            // Act
            var properties = viewMock.Object.Properties;

            // Assert
            Assert.AreSame(expectedProperties, properties);
        }

        [TestMethod]
        public void Properties_ReturnsStableEmptyCollection_DefaultValue()
        {
            // Arrange
            var viewMock = new IDmsMock().CreateView(viewId: 10);

            // Act
            var firstProperties = viewMock.Object.Properties;
            var secondProperties = viewMock.Object.Properties;

            // Assert
            Assert.AreEqual(0, firstProperties.Count);
            Assert.IsEmpty(firstProperties.Cast<IDmsViewProperty>());
            Assert.AreSame(firstProperties, secondProperties);
        }

        [TestMethod]
        public void Services_RemovesServiceFromView_AssignedServiceIsDeleted()
        {
            // Arrange
            var dmsMock = new IDmsMock();
            var viewMock = dmsMock.CreateView(viewId: 10);
            var serviceMock = dmsMock.CreateAgent(agentId: 1).CreateService(serviceId: 2);
            serviceMock.AddView(viewMock.Object.Id);

            // Act
            serviceMock.Object.Delete();

            // Assert
            Assert.IsEmpty(viewMock.Object.Services);
        }

        [TestMethod]
        public void Services_ReturnsEmptyCollection_DefaultValue()
        {
            // Arrange
            var viewMock = new IDmsMock().CreateView(viewId: 10);

            // Act
            var services = viewMock.Object.Services;

            // Assert
            Assert.IsEmpty(services);
        }

        [TestMethod]
        public void Services_ReturnsService_ServiceContainsView()
        {
            // Arrange
            var dmsMock = new IDmsMock();
            var viewMock = dmsMock.CreateView(viewId: 10);
            var serviceMock = dmsMock.CreateAgent(agentId: 1).CreateService(serviceId: 2);
            serviceMock.AddView(viewMock.Object.Id);

            // Act
            var services = viewMock.Object.Services;

            // Assert
            Assert.AreSame(serviceMock.Object, services.Single());
        }

        [TestMethod]
        public void Update_DoesNotThrowException_DeletedView()
        {
            var view = new IDmsMock().CreateView(10, "View").Object;
            view.Delete();

            view.Update();
        }

        [TestMethod]
        public void Update_DoesNotThrowException_ExistingView()
        {
            // Arrange
            var viewMock = new IDmsMock().CreateView(viewId: 10);

            // Act
            viewMock.Object.Update();

            // Assert
            Assert.IsTrue(viewMock.Object.Exists());
        }
    }
}
