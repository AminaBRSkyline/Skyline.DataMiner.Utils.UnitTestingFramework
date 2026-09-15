namespace Utils.UnitTestingFramework.DataMinerSystem.Common.Tests.Examples
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Skyline.DataMiner.Core.DataMinerSystem.Common;
    using Skyline.DataMiner.Utils.UnitTestingFramework.DataMinerSystem.Common;
    using System.Collections.Generic;
    using System.Threading;
    using FluentAssertions;

    [TestClass]
    [DeploymentItem("Examples/protocol.xml", "Examples")]
    public class Demo_Tests
    {
        [TestMethod]
        public void FindSimilarOnSameDma_ReturnsMatchingElementsFromSameDma_WithoutBuilders()
        {
            // Arrange
            var dmsMock = new IDmsMock();
            var firstDma = dmsMock.CreateAgent(1, "DMA 1");
            var secondDma = dmsMock.CreateAgent(2, "DMA 2");
            var thirdDma = dmsMock.CreateAgent(3, "DMA 3");
            var path = "Examples/protocol.xml";

            var sourceMock = firstDma.CreateElement(path, id: 11, agentId: 1, name: "Source");
            var sameDmaMock = firstDma.CreateElement(path, id: 12, agentId: 1, name: "Same DMA");
            secondDma.CreateElement(path, id: 21, agentId: 2, name: "Second DMA");
            thirdDma.CreateElement(path, id: 31, agentId: 3, name: "Third DMA A");
            thirdDma.CreateElement(path, id: 32, agentId: 3, name: "Third DMA B");
            thirdDma.CreateElement(path, id: 33, agentId: 3, name: "Third DMA C");

            // Act
            var similarElements = ElementFinder.FindSimilarOnSameDma(sourceMock.Object);

            // Assert
            similarElements.Should().HaveCount(2);
            similarElements.Should().Contain(element => element.Name == "Source");
            similarElements.Should().Contain(element => element.Name == "Same DMA");
        }

        [TestMethod]
        public void FindSimilarOnSameDma_ReturnsMatchingElementsFromSameDma_WithBuilders()
        {
            // Arrange
            var dmsMock = new DmsBuilder()
                .WithProtocol("Examples/protocol.xml")
                .WithDma(id: 1, configure: dma => dma
                    .WithElement(id: 11, name: "Source", protocolName: "DemoProtocol")
                    .WithElement(id: 12, name: "Same DMA", protocolName: "DemoProtocol"))
                .WithDma(id: 2, configure: dma => dma
                    .WithElement(id: 21, name: "Second DMA", protocolName: "DemoProtocol"))
                .WithDma(id: 3, configure: dma => dma
                    .WithElement(id: 31, name: "Third DMA A", protocolName: "DemoProtocol")
                    .WithElement(id: 32, name: "Third DMA B", protocolName: "DemoProtocol")
                    .WithElement(id: 33, name: "Third DMA C", protocolName: "DemoProtocol"))
                .Build();

            var source = dmsMock.Object.GetAgent(1).GetElement("Source");

            // Act
            var similarElements = ElementFinder.FindSimilarOnSameDma(source);

            // Assert
            similarElements.Should().HaveCount(2);
            similarElements.Should().Contain(source);
            similarElements.Should().Contain(element => element.Name == "Same DMA");
        }

        [TestMethod]
        public void FindOtherViewsOnSameDms_ReturnsAllViewsExceptSelectedView_WithBuilders()
        {
            // Arrange
            var dmsMock = new DmsBuilder()
                .WithProtocol("Examples/protocol.xml")
                .WithView(10, "Other A")
                .WithView(20, "Selected")
                .WithView(30, "Other B")
                .WithDma(id: 1, configure: dma => dma
                    .WithElement(id: 11, name: "Element", protocolName: "DemoProtocol", configure: element =>
                        element.UnderView(20)))
                .Build();

            var otherA = dmsMock.Object.GetView(10);
            var selected = dmsMock.Object.GetView(20);
            var otherB = dmsMock.Object.GetView(30);

            // Act
            var otherViews = ElementFinder.FindOtherViews(selected);

            // Assert
            otherViews.Should().HaveCount(2);
            otherViews.Should().Contain(otherA);
            otherViews.Should().Contain(otherB);
            otherViews.Should().NotContain(selected);
        }

        [TestMethod]
        public void FindOtherViewsOnSameDms_ReturnsAllViewsExceptSelectedView_WithoutBuilders()
        {
            // Arrange
            var dmsMock = new IDmsMock();
            var dmaMock = dmsMock.CreateAgent(1, "DMA 1");

            var otherAMock = dmsMock.CreateView(10, "Other A");
            var selectedMock = dmsMock.CreateView(20, "Selected");
            var otherBMock = dmsMock.CreateView(30, "Other B");

            var elementMock = dmaMock.CreateElement("Examples/protocol.xml", id: 11, agentId: 1, name: "Element");
            elementMock.AddView(20);

            // Act
            var otherViews = ElementFinder.FindOtherViews(selectedMock.Object);

            // Assert
            otherViews.Should().HaveCount(2);
            otherViews.Should().Contain(otherAMock.Object);
            otherViews.Should().Contain(otherBMock.Object);
            otherViews.Should().NotContain(selectedMock.Object);
        }

        [TestMethod]
        public void Repoll_ReplacesExistingRowsWithPolledRows_WithBuilders()
        {
            // Arrange
            var dmsMock = new DmsBuilder()

                .WithProtocol("Examples/protocol.xml")
                .WithDma(id: 1, configure: dma => dma.WithElement(id: 11, name: "Element", protocolName: "DemoProtocol", configure: element => element.WithTable(100, new object[][] { new object[] { "old", "Old value" } })))
                .Build();

            var elementMock = dmsMock.Object.GetElement("Element");
            var table = elementMock.GetTable(100);
            var tableMock = Mock.Get(table);
            var connectorApi = new ConnectorApi(dmsMock.Object, "DemoProtocol");
            tableMock.Invocations.Clear();

            // Act
            connectorApi.Repoll();

            // Assert
            var expectedRows = new object[][]
            {
                new object[] { "1", "Value1" },
                new object[] { "2", "Value2" },
                new object[] { "3", "Value3" }
            };

            tableMock.Verify(t => t.AddRow(It.IsAny<object[]>()), Times.Exactly(3));
            table.GetRows().Should().BeEquivalentTo(expectedRows);
        }

        [TestMethod]
        public void Repoll_ReplacesExistingRowsWithPolledRows_WithoutBuilders()
        {
            // Arrange
            var dmsMock = new IDmsMock();
            var dmaMock = dmsMock.CreateAgent(1, "DMA 1");
            var elementMock = dmaMock.CreateElement("Examples/protocol.xml", id: 11, agentId: 1, name: "Element");
            var table = elementMock.Object.GetTable(100);
            table.AddRow(new object[] { "old", "Old value" });

            var tableMock = Mock.Get(table);
            var connectorApi = new ConnectorApi(dmsMock.Object, "DemoProtocol");
            tableMock.Invocations.Clear();

            // Act
            connectorApi.Repoll();

            // Assert
            var expectedRows = new object[][]
           {
                new object[] { "1", "Value1" },
                new object[] { "2", "Value2" },
                new object[] { "3", "Value3" }
           };

            tableMock.Verify(t => t.AddRow(It.IsAny<object[]>()), Times.Exactly(3));
            table.GetRows().Should().BeEquivalentTo(expectedRows);
        }

        [TestMethod]
        public void RestartAndEnablePolling_WhenStartupCompletes_SetsPollingStatusToEnabled_WithBuilders()
        {
            // Arrange
            var dmsMock = new DmsBuilder()
                .WithProtocol("Examples/protocol.xml")
                .WithDma(id: 1, configure: dma => dma
                    .WithElement(id: 11, name: "Element", protocolName: "DemoProtocol", configure: element => element
                            .WithParameter<int?>(10, 0)))
                .Build();

            var elementMock = (IDmsElementMock)Mock.Get(dmsMock.Object.GetElement("Element"));
            var parameter = elementMock.Object.GetStandaloneParameter<int?>(10);
            var parameterMock = Mock.Get(parameter);

            elementMock.Setup(e => e.IsStartupComplete()).Returns(() =>
            {
                elementMock.State = ElementState.Active;
                return true;
            });

            var connectorApi = new ConnectorApi(dmsMock.Object, "DemoProtocol");

            // Act
            connectorApi.RestartAndEnablePolling();

            // Assert
            elementMock.Verify(e => e.Restart(), Times.Once());
            elementMock.State.Should().Be(ElementState.Active);
            parameterMock.Verify(p => p.SetValue(1), Times.Once());
            parameter.GetValue().Should().Be(1);
        }

        [TestMethod]
        public void RestartAndEnablePolling_WhenStartupCompletes_SetsPollingStatusToEnabled_WithoutBuilders()
        {
            // Arrange
            var dmsMock = new IDmsMock();
            var dmaMock = dmsMock.CreateAgent(1, "DMA 1");
            var elementMock = dmaMock.CreateElement("Examples/protocol.xml", id: 11, agentId: 1, name: "Element");
            var parameter = elementMock.Object.GetStandaloneParameter<int?>(10);
            parameter.SetValue(0);
            var parameterMock = Mock.Get(parameter);

            elementMock.Setup(e => e.IsStartupComplete()).Returns(() =>
            {
                elementMock.State = ElementState.Active;
                return true;
            });

            var connectorApi = new ConnectorApi(dmsMock.Object, "DemoProtocol");

            // Act
            connectorApi.RestartAndEnablePolling();

            // Assert
            elementMock.Verify(e => e.Restart(), Times.Once());
            elementMock.State.Should().Be(ElementState.Active);
            parameterMock.Verify(p => p.SetValue(1), Times.Once());
            parameter.GetValue().Should().Be(1);
        }
    }

    internal static class ElementFinder
    {
        public static List<IDmsElement> FindSimilarOnSameDma(IDmsElement element)
        {
            if (element is null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            if (element.Protocol == null)
            {
                throw new ArgumentException($"Element '{element.Name}' does not have a protocol defined.", nameof(element));
            }

            var dma = element.Host ?? throw new InvalidOperationException($"Element '{element.Name}' is not hosted on any DMA.");

            return dma.GetElements().Where(el => el.Protocol.Name == element.Protocol.Name && el.Protocol.ReferencedVersion == element.Protocol.ReferencedVersion).ToList();
        }

        public static List<IDmsView> FindOtherViews(IDmsView view)
        {
            if (view is null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            var element = view.Elements.FirstOrDefault() ?? throw new InvalidOperationException($"View '{view.Name}' does not contain any elements.");

            var dma = element.Host ?? throw new InvalidOperationException($"Element '{element.Name}' is not hosted on any DMA.");

            var dms = dma.Dms ?? throw new InvalidOperationException($"DMA '{dma.Name}' is not hosted on any DMS.");

            var allViews = dms.GetViews();

            return allViews.Except(new[] { view }).ToList();
        }
    }

    internal class ConnectorApi
    {
        private readonly IDmsElement element;

        internal ConnectorApi(IDms dms, string protocolName)
        {
            element = dms.GetElements().FirstOrDefault(el => el.Protocol.Name == protocolName) ?? throw new InvalidOperationException($"Element with protocol name '{protocolName}' not found.");
        }

        public void Repoll()
        {
            var table = element.GetTable(100);

            table.DeleteRows(table.GetPrimaryKeys());

            var polledRows = new object[][]
            {
                new object[] { "1", "Value1" },
                new object[] { "2", "Value2" },
                new object[] { "3", "Value3" }
            };

            foreach (var row in polledRows)
            {
                table.AddRow(row);
            }
        }

        public void RestartAndEnablePolling()
        {
            if (element.State != ElementState.Active)
            {
                throw new InvalidOperationException($"Element is not active. Current state: {element.State}");
            }

            element.Restart();

            int retries = 0;
            while (!element.IsStartupComplete() && retries < 3)
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                retries++;
            }

            if (element.State != ElementState.Active)
            {
                throw new InvalidOperationException("Element did not complete startup within the expected time.");
            }

            element.GetStandaloneParameter<int?>(10).SetValue(1);
        }
    }
}
