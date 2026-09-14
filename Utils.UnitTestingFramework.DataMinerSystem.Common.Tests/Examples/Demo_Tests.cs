namespace Utils.UnitTestingFramework.DataMinerSystem.Common.Tests.Examples
{
    using System;
    using System.Linq;
    using Moq;
    using Skyline.DataMiner.Core.DataMinerSystem.Common;

    [TestClass]
    internal class Demo_Tests
    {
        public void FindSimilarOnSameDma_ArrangeWithBuilders()
        {
            // Arrange
            // TODO arrange WITHOUT using builders: one dms with 3 dmas, each with with different amount of elements with "DemoProtocol" protocol.

            // Act
            var similarElements = ElementFinder.FindSimilarOnSameDma(new Mock<IDmsElement>().Object);

            // Assert
            // TODO assert
        }

        public void FindSimilarOnSameDma_ArrangeWithoutBuilders()
        {
            // Arrange
            // TODO arrange WITH using builders: one dms with 3 dmas, each with different amount of elements with "DemoProtocol" protocol.

            // Act
            var similarElements = ElementFinder.FindSimilarOnSameDma(new Mock<IDmsElement>().Object);

            // Assert
            // TODO assert
        }

        public void Repoll_ArrangeWithBuilders()
        {
            // Arrange
            // TODO arrange WITHOUT using builders: one dms with one dma with one element with "DemoProtocol" protocol, fill table 1 with some random rows.

            // Act
            var connectorApi = new ConnectorApi(new Mock<IDms>().Object, "DemoProtocol");
            connectorApi.Repoll();

            // Assert
            // TODO assert by accessing the IDmsElementMock and checking if the correct row was added to the table (without going via .Object).
        }

        public void Repoll_ArrangeWithoutBuilders()
        {
            // Arrange
            // TODO arrange WITH using the builders: one dms with one dma with one element with "DemoProtocol" protocol, fill table 1 with some random rows.

            // Act
            var connectorApi = new ConnectorApi(new Mock<IDms>().Object, "DemoProtocol");
            connectorApi.Repoll();

            // Assert
            // TODO assert by accessing the IDmsElementMock and checking if the correct row was added to the table (without going via .Object).
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
            element = dms.GetElements().FirstOrDefault(el  => el.Protocol.Name == protocolName) ?? throw new InvalidOperationException($"Element with protocol name '{protocolName}' not found.");
        }

        public void Repoll()
        {
            var table = element.GetTable(1);

            table.DeleteRows(table.GetPrimaryKeys());

            var polledRows = new object[][]
            {
                new object[] { 1, "Value1" },
                new object[] { 2, "Value2" },
                new object[] { 3, "Value3" }
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
            while(!element.IsStartupComplete() && retries < 3)
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
