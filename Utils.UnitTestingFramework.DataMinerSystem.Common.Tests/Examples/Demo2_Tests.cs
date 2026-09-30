namespace Utils.UnitTestingFramework.DataMinerSystem.Common.Tests.Examples
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.Core.DataMinerSystem.Common;
    using Skyline.DataMiner.Core.DataMinerSystem.Common.Subscription.Monitors;
    using Skyline.DataMiner.Utils.UnitTestingFramework.DataMinerSystem.Common;

    [TestClass]
    [DeploymentItem("Examples/protocol.xml", "Examples")]
    public class Demo2_Tests
    {
        private const int ParameterId = 10;
        private const string ProtocolPath = "Examples/protocol.xml";

        [TestMethod]
        public void ConnectionListener_ReturnsInvocationCounts_WhenTrackingStandaloneAndTableChanges()
        {
            // Arrange
            var dmsMock = new IDmsMock();
            var firstDma = dmsMock.CreateAgent(1, "DMA 1");
            var secondDma = dmsMock.CreateAgent(2, "DMA 2");

            var trackedElement = firstDma.CreateElement(ProtocolPath, id: 11, name: "Tracked element");
            var otherElementOnSameDma = firstDma.CreateElement(ProtocolPath, id: 12, name: "Other element on same DMA");
            var elementWithSameIdOnOtherDma = secondDma.CreateElement(ProtocolPath, id: 11, name: "Element with same ID on other DMA");

            var trackedTable = trackedElement.GetDmsTableMock(100);
            var otherTableOnSameDma = otherElementOnSameDma.GetDmsTableMock(100);
            var tableOnOtherDma = elementWithSameIdOnOtherDma.GetDmsTableMock(100);

            trackedTable.SetRow(new object[] { "row-1", "Initial value" });
            otherTableOnSameDma.SetRow(new object[] { "row-1", "Initial value" });
            tableOnOtherDma.SetRow(new object[] { "row-1", "Initial value" });

            var listener = new ConnectionListener(dmsMock.Connection.Object);

            // Act
            listener.StartTracking(trackedElement.Object.DmsElementId);

            otherElementOnSameDma.GetStandaloneParameterMock<int?>(ParameterId).UpdateValue(1);
            elementWithSameIdOnOtherDma.GetStandaloneParameterMock<int?>(ParameterId).UpdateValue(2);
            trackedElement.GetStandaloneParameterMock<int?>(ParameterId).UpdateValue(3);

            otherTableOnSameDma.SetCell("row-1", 102, "Other value");
            tableOnOtherDma.SetCell("row-1", 102, "Other DMA value");
            trackedTable.SetCell("row-1", 102, "Tracked value");

            listener.StopTracking();

            // Assert
            Assert.AreEqual(1, listener.NumberOfStandaloneParameterInvokations);
            Assert.AreEqual(1, listener.NumberOfTableParameterInvokations);
        }
    }

    internal class TableWatcher
    {
        private readonly IDmsTable table;
        private readonly int primaryKeyColumnPid;
        private Guid sourceId = new Guid();

        public TableWatcher(IDmsTable table)
        {
            this.table = table;
        }

        public int NumberOfTableValueChanges { get; private set; }

        public void Start()
        {
            table.StartValueMonitor(sourceId.ToString(), primaryKeyColumnPid, OnTableValueChanged, includeCurrentValues: false);
        }

        private void OnTableValueChanged(TableValueChange change)
        {
            NumberOfTableValueChanges++;
        }
    }
}
