namespace Utils.UnitTestingFramework.DataMinerSystem.Common.Tests.Examples
{
    using System;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Skyline.DataMiner.Analytics.GenericInterface.QueryBuilder;
    using Skyline.DataMiner.Core.DataMinerSystem.Common;
    using Skyline.DataMiner.Core.DataMinerSystem.Common.Subscription.Monitors;
    using Skyline.DataMiner.Protobuf.Data.Api.v1;
    using Skyline.DataMiner.Utils.UnitTestingFramework.DataMinerSystem.Common;

    [TestClass]
    [DeploymentItem("Examples/protocol.xml", "Examples")]
    public class Demo2_Tests
    {
        private const int ParameterId = 10;
        private const int TableId = 100;
        private const string ProtocolName = "DemoProtocol";
        private const string ProtocolPath = "Examples/protocol.xml";
        private Action<TableValueChange> onChangeCallback;

        [TestMethod]
        public void OldSetup_TableWatcher_ReturnsUpdateCounts()
        {
            // Arrange
            var evsCerebrumElementMock = new Mock<IDmsElement>();
            var dataminerDmsTableMock = new Mock<IDmsTable>();
            dataminerDmsTableMock.Setup(x => x.StartValueMonitor(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Action<TableValueChange>>(), It.IsAny<bool>()))
                .Callback((string _, int __, Action<TableValueChange> onChange, bool ____) =>
                {
                    onChangeCallback = onChange;
                });

            var dmsMock = new Mock<IDms>();

            dmsMock.Setup(x => x.GetElement(It.IsAny<DmsElementId>())).Returns(evsCerebrumElementMock.Object);
            evsCerebrumElementMock.Setup(x => x.GetTable(It.IsAny<int>())).Returns(dataminerDmsTableMock.Object);

            var tableWatcher = new TableWatcher(dmsMock.Object, new DmsElementId(1, 1), 100);
            tableWatcher.Start();
            onChangeCallback?.Invoke(new TableValueChange(null, null, null, 1, null, null));

            tableWatcher.NumberOfTableValueChanges.Should().Be(1);
        }


        [TestMethod]
        public void TableWatcher_IncrementsNumberOfTableValueChanges_WhenRowIsAdded()
        {
            // Arrange
            var elementId = new DmsElementId(1, 11);
            var dmsMock = new DmsBuilder()
                .WithProtocol(ProtocolPath)
                .WithDma(id: elementId.AgentId, dma => dma
                    .WithElement(id: elementId.ElementId, name: "Element", protocolName: ProtocolName))
                .Build();

            var watcher = new TableWatcher(dmsMock.Object, elementId, TableId);

            // Act
            watcher.Start();
            dmsMock.Object.GetElement(elementId).GetTable(TableId).AddRow(new object[] { "1", "Test" });
            dmsMock.Object.GetElement(elementId).GetTable(TableId).AddRow(new object[] { "2", "Test2" });
            dmsMock.Object.GetElement(elementId).GetTable(TableId).AddRow(new object[] { "3", "Tes3" });
            dmsMock.Object.GetElement(elementId).GetTable(TableId).AddRow(new object[] { "4", "Tes4" });

            // Assert
            watcher.NumberOfTableValueChanges.Should().Be(4);
        }
    }


    internal class TableWatcher
    {
        private readonly IDmsTable table;
        private readonly int primaryKeyColumnPid;
        private Guid sourceId = new Guid();

        public TableWatcher(IDms dms, DmsElementId elementId, int tablePid)
        {
            this.table = dms.GetElement(elementId).GetTable(tablePid);
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
