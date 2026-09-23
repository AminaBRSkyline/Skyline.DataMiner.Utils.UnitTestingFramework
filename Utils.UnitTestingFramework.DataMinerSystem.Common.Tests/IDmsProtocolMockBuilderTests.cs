namespace Skyline.DataMiner.Utils.UnitTestingFramework.DataMinerSystem.Common.Tests
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.Utils.UnitTestingFramework.Common.Model;
    using Skyline.DataMiner.Utils.UnitTestingFramework.Common.Model.Creation;

    [TestClass]
    public class IDmsProtocolMockBuilderTests
    {
        [TestMethod]
        public void AddParameterDefinition_ThrowsArgumentException_WithDuplicateId()
        {
            var builder = new IDmsProtocolMockBuilder("Protocol")
                .AddParameterDefinition(new ParameterDefinition("First", typeof(double), 100));

            Assert.ThrowsExactly<ArgumentException>(() =>
                builder.AddParameterDefinition(new ParameterDefinition("Second", typeof(double), 100)));
        }

        [TestMethod]
        public void AddParameterDefinition_ThrowsArgumentNullException_WithNullDefinition()
        {
            var builder = new IDmsProtocolMockBuilder("Protocol");

            Assert.ThrowsExactly<ArgumentNullException>(() => builder.AddParameterDefinition(null));
        }

        [TestMethod]
        public void AddTableDefinition_ThrowsArgumentException_WithDuplicateId()
        {
            var firstTableBuilder = new TableModelBuilder(200);
            firstTableBuilder.AddColumn(columnPid: 201, columnIdx: 0, isKey: true);
            var secondTableBuilder = new TableModelBuilder(200);
            secondTableBuilder.AddColumn(columnPid: 202, columnIdx: 0, isKey: true);
            var builder = new IDmsProtocolMockBuilder("Protocol")
                .AddTableDefinition(200, firstTableBuilder.Build().Schema);

            Assert.ThrowsExactly<ArgumentException>(() =>
                builder.AddTableDefinition(200, secondTableBuilder.Build().Schema));
        }

        [TestMethod]
        public void AddTableDefinition_ThrowsArgumentNullException_WithNullSchema()
        {
            var builder = new IDmsProtocolMockBuilder("Protocol");

            Assert.ThrowsExactly<ArgumentNullException>(() => builder.AddTableDefinition(100, null));
        }

        [TestMethod]
        public void Build_AddsParameterAndTableDefinitions_ToProtocol()
        {
            // Arrange
            var parameterDefinition = new ParameterDefinition("Parameter", typeof(double), 100);
            var tableBuilder = new TableModelBuilder(200);
            tableBuilder.AddColumn(columnPid: 201, columnIdx: 0, isKey: true, columnName: "Key");
            var tableSchema = tableBuilder.Build().Schema;

            // Act
            var protocol = new IDmsProtocolMockBuilder("Protocol")
                .AddParameterDefinition(parameterDefinition)
                .AddTableDefinition(200, tableSchema)
                .Build();

            // Assert
            Assert.AreSame(parameterDefinition, protocol.Definitions.GetParameterDefinition(100));
            Assert.AreSame(tableSchema, protocol.Definitions.GetTableDefinition(200));
        }

        [TestMethod]
        public void Build_ReturnsDefaultVersion_WithoutExplicitVersion()
        {
            var protocol = new IDmsProtocolMockBuilder("Protocol").Build();

            Assert.AreEqual(IDmsProtocolMock.DefaultVersion, protocol.ReferencedVersion);
        }

        [TestMethod]
        public void Build_ReturnsProvidedNameAndVersion_WithCustomIdentifiers()
        {
            var protocol = new IDmsProtocolMockBuilder("Protocol", "2.0.0.0").Build();

            Assert.AreEqual("Protocol", protocol.Name);
            Assert.AreEqual("2.0.0.0", protocol.ReferencedVersion);
        }

        [TestMethod]
        public void Build_ThrowsArgumentException_WithEmptyName()
        {
            var builder = new IDmsProtocolMockBuilder(String.Empty);

            Assert.ThrowsExactly<ArgumentException>(() => builder.Build());
        }

        [TestMethod]
        public void Build_ThrowsArgumentException_WithEmptyVersion()
        {
            var builder = new IDmsProtocolMockBuilder("Protocol", String.Empty);

            Assert.ThrowsExactly<ArgumentException>(() => builder.Build());
        }

        [TestMethod]
        public void Build_ThrowsArgumentNullException_WithNullName()
        {
            var builder = new IDmsProtocolMockBuilder(null);

            Assert.ThrowsExactly<ArgumentNullException>(() => builder.Build());
        }

        [TestMethod]
        public void Build_ThrowsArgumentNullException_WithNullVersion()
        {
            var builder = new IDmsProtocolMockBuilder("Protocol", null);

            Assert.ThrowsExactly<ArgumentNullException>(() => builder.Build());
        }
    }
}
