namespace Skyline.DataMiner.Utils.UnitTestingFramework.DataMinerSystem.Common.Tests
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.Core.DataMinerSystem.Common;
    using Skyline.DataMiner.Core.DataMinerSystem.Common.Subscription.Monitors;
    using Skyline.DataMiner.Utils.UnitTestingFramework.DataMinerSystem.Common;

    [TestClass]
    [DeploymentItem("TestFiles/Model/Data/protocol.xml")]
    public class DmsColumnMockTests
    {
        private readonly string path = "protocol.xml";

        [TestMethod]
        public void GetValue_ReturnsStoredNumericValue_WithNumericColumn()
        {
            // Arrange
            var mock = new IDmsMock().CreateAgent(agentId: 0).CreateElement(path);
            var table = mock.Object.GetTable(900);
            table.AddRow(new object[] { "one", "one-desc", 3.0, 4.0, 5.0 });

            // Act
            var value = table.GetColumn<double?>(903).GetValue("one", KeyType.PrimaryKey);

            // Assert
            Assert.AreEqual(3.0, value);
        }

        [TestMethod]
        public void GetValue_ReturnsStoredStringValue_WithObsoleteOverload()
        {
            // Arrange
            var mock = new IDmsMock().CreateAgent(agentId: 0).CreateElement(path);
            var table = mock.Object.GetTable(900);
            table.AddRow(new object[] { "one", "one-desc", 3.0, 4.0, 5.0 });

            // Act
#pragma warning disable CS0618 // Type or member is obsolete - the obsolete overload is verified to remain usable.
            var value = table.GetColumn<string>(902).GetValue("one");
#pragma warning restore CS0618

            // Assert
            Assert.AreEqual("one-desc", value);
        }

        [TestMethod]
        public void GetValue_ReturnsStoredValue_WithKeyTypeOverload()
        {
            // Arrange
            var mock = new IDmsMock().CreateAgent(agentId: 0).CreateElement(path);
            var table = mock.Object.GetTable(900);
            table.AddRow(new object[] { "one", "one-desc", 3.0, 4.0, 5.0 });

            // Act
            var value = table.GetColumn<string>(902).GetValue("one", KeyType.PrimaryKey);

            // Assert
            Assert.AreEqual("one-desc", value);
        }

        [TestMethod]
        public void Id_ReturnsColumnPid_ForRequestedColumn()
        {
            // Arrange
            var mock = new IDmsMock().CreateAgent(agentId: 0).CreateElement(path);
            var table = mock.Object.GetTable(900);

            // Act
            var column = table.GetColumn<string>(902);

            // Assert
            Assert.AreEqual(902, column.Id);
        }

        [TestMethod]
        public void SetValue_PersistsValue_WithDefaultOverload()
        {
            // Arrange
            var mock = new IDmsMock().CreateAgent(agentId: 0).CreateElement(path);
            var table = mock.Object.GetTable(900);
            table.AddRow(new object[] { "one", "one-desc", 3.0, 4.0, 5.0 });

            // Act
            table.GetColumn<string>(902).SetValue("one", "changed-desc");

            // Assert
            Assert.AreEqual("changed-desc", table.GetColumn<string>(902).GetValue("one", KeyType.PrimaryKey));
        }

        [TestMethod]
        public void SetValue_PersistsValue_WithKeyTypeAndExpectedChangesOverload()
        {
            // Arrange
            var mock = new IDmsMock().CreateAgent(agentId: 0).CreateElement(path);
            var table = mock.Object.GetTable(900);
            table.AddRow(new object[] { "one", "one-desc", 3.0, 4.0, 5.0 });

            // Act
            table.GetColumn<string>(902).SetValue("one", KeyType.PrimaryKey, "changed-desc", TimeSpan.FromSeconds(1), null);

            // Assert
            Assert.AreEqual("changed-desc", table.GetColumn<string>(902).GetValue("one", KeyType.PrimaryKey));
        }

        [TestMethod]
        public void SetValue_PersistsValue_WithKeyTypeOverload()
        {
            // Arrange
            var mock = new IDmsMock().CreateAgent(agentId: 0).CreateElement(path);
            var table = mock.Object.GetTable(900);
            table.AddRow(new object[] { "one", "one-desc", 3.0, 4.0, 5.0 });

            // Act
            table.GetColumn<string>(902).SetValue("one", KeyType.PrimaryKey, "changed-desc");

            // Assert
            Assert.AreEqual("changed-desc", table.GetColumn<string>(902).GetValue("one", KeyType.PrimaryKey));
        }

        [TestMethod]
        public void StartValueMonitor_DoesNotInvokeCallback_WhenDifferentColumnChanges()
        {
            // Arrange
            var element = new IDmsMock().CreateAgent(agentId: 0).CreateElement(path);
            var table = element.Object.GetTable(900);
            table.AddRow(new object[] { "one", "one-desc", 3.0, 4.0, 5.0 });
            var monitoredColumn = table.GetColumn<string>(902);
            ColumnValueChange<string> received = null;
            monitoredColumn.StartValueMonitor("source", change => received = change, false);

            // Act
            table.GetColumn<double?>(903).SetValue("one", 10.0);

            // Assert
            Assert.IsNull(received);
        }

        [TestMethod]
        public void StartValueMonitor_InvokesCallbackOnCellChange_WithColumnOverload()
        {
            // Arrange
            var mock = new IDmsMock().CreateAgent(agentId: 0).CreateElement(path);
            var table = mock.Object.GetTable(900);
            table.AddRow(new object[] { "one", "one-desc", 3.0, 4.0, 5.0 });
            var column = table.GetColumn<string>(902);

            ColumnValueChange<string> received = null;
            column.StartValueMonitor("source", change => received = change, false);

            // Act
            column.SetValue("one", "changed-desc");

            // Assert
            Assert.IsNotNull(received);
            Assert.AreEqual("changed-desc", received.ColumnUpdates["one"]);
        }

        [TestMethod]
        public void StartValueMonitor_InvokesCallbackOnCellChange_WithColumnTimeSpanOverload()
        {
            // Arrange
            var mock = new IDmsMock().CreateAgent(agentId: 0).CreateElement(path);
            var table = mock.Object.GetTable(900);
            table.AddRow(new object[] { "one", "one-desc", 3.0, 4.0, 5.0 });
            var column = table.GetColumn<string>(902);

            ColumnValueChange<string> received = null;
            column.StartValueMonitor("source", change => received = change, TimeSpan.FromSeconds(1), false);

            // Act
            column.SetValue("one", "changed-desc");

            // Assert
            Assert.IsNotNull(received);
            Assert.AreEqual("changed-desc", received.ColumnUpdates["one"]);
        }

        [TestMethod]
        public void StartValueMonitor_InvokesCallbackOnMatchingCellChange_WithCellOverload()
        {
            // Arrange
            var mock = new IDmsMock().CreateAgent(agentId: 0).CreateElement(path);
            var table = mock.Object.GetTable(900);
            table.AddRow(new object[] { "one", "one-desc", 3.0, 4.0, 5.0 });
            table.AddRow(new object[] { "two", "two-desc", 6.0, 7.0, 8.0 });
            var column = table.GetColumn<string>(902);

            CellValueChange<string> received = null;
            column.StartValueMonitor("source", "one", change => received = change, false);

            // Act
            column.SetValue("two", "changed-two");
            Assert.IsNull(received, "Callback should not fire for a different primary key.");

            column.SetValue("one", "changed-one");

            // Assert
            Assert.IsNotNull(received);
            Assert.AreEqual("changed-one", received.Value);
        }

        [TestMethod]
        public void StartValueMonitor_InvokesCallbackOnMatchingCellChange_WithCellTimeSpanOverload()
        {
            // Arrange
            var mock = new IDmsMock().CreateAgent(agentId: 0).CreateElement(path);
            var table = mock.Object.GetTable(900);
            table.AddRow(new object[] { "one", "one-desc", 3.0, 4.0, 5.0 });
            var column = table.GetColumn<string>(902);

            CellValueChange<string> received = null;
            column.StartValueMonitor("source", "one", change => received = change, TimeSpan.FromSeconds(1), false);

            // Act
            column.SetValue("one", "changed-one");

            // Assert
            Assert.IsNotNull(received);
            Assert.AreEqual("changed-one", received.Value);
        }

        [TestMethod]
        public void StartValueMonitor_ReplacesExistingCallback_WithSameCellSourceId()
        {
            // Arrange
            var element = new IDmsMock().CreateAgent(agentId: 0).CreateElement(path);
            var table = element.Object.GetTable(900);
            table.AddRow(new object[] { "one", "one-desc", 3.0, 4.0, 5.0 });
            var column = table.GetColumn<string>(902);
            var firstInvocations = 0;
            var secondInvocations = 0;

            column.StartValueMonitor("source", "one", change => firstInvocations++, false);
            column.StartValueMonitor("source", "one", change => secondInvocations++, false);

            // Act
            column.SetValue("one", "changed");

            // Assert
            Assert.AreEqual(0, firstInvocations);
            Assert.AreEqual(1, secondInvocations);
        }

        [TestMethod]
        public void StartValueMonitor_ReplacesExistingCallback_WithSameColumnSourceId()
        {
            // Arrange
            var element = new IDmsMock().CreateAgent(agentId: 0).CreateElement(path);
            var table = element.Object.GetTable(900);
            table.AddRow(new object[] { "one", "one-desc", 3.0, 4.0, 5.0 });
            var column = table.GetColumn<string>(902);
            var firstInvocations = 0;
            var secondInvocations = 0;

            column.StartValueMonitor("source", change => firstInvocations++, false);
            column.StartValueMonitor("source", change => secondInvocations++, false);

            // Act
            column.SetValue("one", "changed");

            // Assert
            Assert.AreEqual(0, firstInvocations);
            Assert.AreEqual(1, secondInvocations);
        }

        [TestMethod]
        public void StartValueMonitor_ThrowsArgumentNullException_WithNullCellCallback()
        {
            var column = CreateStringColumn();

            Assert.ThrowsExactly<ArgumentNullException>(() =>
                column.StartValueMonitor("source", "one", (Action<CellValueChange<string>>)null, false));
        }

        [TestMethod]
        public void StartValueMonitor_ThrowsArgumentNullException_WithNullCellPrimaryKey()
        {
            var column = CreateStringColumn();

            Assert.ThrowsExactly<ArgumentNullException>(() =>
                column.StartValueMonitor("source", null, change => { }, false));
        }

        [TestMethod]
        public void StartValueMonitor_ThrowsArgumentNullException_WithNullCellSourceId()
        {
            var column = CreateStringColumn();

            Assert.ThrowsExactly<ArgumentNullException>(() =>
                column.StartValueMonitor(null, "one", change => { }, false));
        }

        [TestMethod]
        public void StartValueMonitor_ThrowsArgumentNullException_WithNullColumnCallback()
        {
            var column = CreateStringColumn();

            Assert.ThrowsExactly<ArgumentNullException>(() =>
                column.StartValueMonitor("source", (Action<ColumnValueChange<string>>)null, false));
        }

        [TestMethod]
        public void StartValueMonitor_ThrowsArgumentNullException_WithNullColumnSourceId()
        {
            var column = CreateStringColumn();

            Assert.ThrowsExactly<ArgumentNullException>(() =>
                column.StartValueMonitor(null, change => { }, false));
        }

        [TestMethod]
        public void StopValueMonitor_DoesNotInvokeCallbackAfterStop_WithCellOverload()
        {
            // Arrange
            var mock = new IDmsMock().CreateAgent(agentId: 0).CreateElement(path);
            var table = mock.Object.GetTable(900);
            table.AddRow(new object[] { "one", "one-desc", 3.0, 4.0, 5.0 });
            var column = table.GetColumn<string>(902);

            CellValueChange<string> received = null;
            column.StartValueMonitor("source", "one", change => received = change, false);
            column.StopValueMonitor("source", "one", false);

            // Act
            column.SetValue("one", "changed-one");

            // Assert
            Assert.IsNull(received);
        }

        [TestMethod]
        public void StopValueMonitor_DoesNotInvokeCallbackAfterStop_WithCellTimeSpanOverload()
        {
            // Arrange
            var mock = new IDmsMock().CreateAgent(agentId: 0).CreateElement(path);
            var table = mock.Object.GetTable(900);
            table.AddRow(new object[] { "one", "one-desc", 3.0, 4.0, 5.0 });
            var column = table.GetColumn<string>(902);

            CellValueChange<string> received = null;
            column.StartValueMonitor("source", "one", change => received = change, false);
            column.StopValueMonitor("source", "one", TimeSpan.FromSeconds(1), false);

            // Act
            column.SetValue("one", "changed-one");

            // Assert
            Assert.IsNull(received);
        }

        [TestMethod]
        public void StopValueMonitor_DoesNotInvokeCallbackAfterStop_WithColumnOverload()
        {
            // Arrange
            var mock = new IDmsMock().CreateAgent(agentId: 0).CreateElement(path);
            var table = mock.Object.GetTable(900);
            table.AddRow(new object[] { "one", "one-desc", 3.0, 4.0, 5.0 });
            var column = table.GetColumn<string>(902);

            ColumnValueChange<string> received = null;
            column.StartValueMonitor("source", change => received = change, false);
            column.StopValueMonitor("source", false);

            // Act
            column.SetValue("one", "changed-desc");

            // Assert
            Assert.IsNull(received);
        }

        [TestMethod]
        public void StopValueMonitor_DoesNotInvokeCallbackAfterStop_WithColumnTimeSpanOverload()
        {
            // Arrange
            var mock = new IDmsMock().CreateAgent(agentId: 0).CreateElement(path);
            var table = mock.Object.GetTable(900);
            table.AddRow(new object[] { "one", "one-desc", 3.0, 4.0, 5.0 });
            var column = table.GetColumn<string>(902);

            ColumnValueChange<string> received = null;
            column.StartValueMonitor("source", change => received = change, false);
            column.StopValueMonitor("source", TimeSpan.FromSeconds(1), false);

            // Act
            column.SetValue("one", "changed-desc");

            // Assert
            Assert.IsNull(received);
        }

        [TestMethod]
        public void Table_ReturnsOwningTable_ForRequestedColumn()
        {
            // Arrange
            var mock = new IDmsMock().CreateAgent(agentId: 0).CreateElement(path);
            var table = mock.Object.GetTable(900);

            // Act
            var column = table.GetColumn<string>(902);

            // Assert
            Assert.AreSame(table, column.Table);
        }

        private IDmsColumn<string> CreateStringColumn()
        {
            var element = new IDmsMock().CreateAgent(agentId: 0).CreateElement(path);
            return element.Object.GetTable(900).GetColumn<string>(902);
        }
    }
}
