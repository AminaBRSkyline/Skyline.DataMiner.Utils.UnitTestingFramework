namespace Skyline.DataMiner.Utils.UnitTestingFramework.DataMinerSystem.Common.Tests
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.Utils.UnitTestingFramework.Common;
    using Skyline.DataMiner.Utils.UnitTestingFramework.Common.Model;
    using Skyline.DataMiner.Utils.UnitTestingFramework.Common.Model.Creation;
    using Skyline.DataMiner.Utils.UnitTestingFramework.Common.Model.Table;

    [TestClass]
    public class ParameterAndTableDefinitionsTests
    {
        [TestMethod]
        public void AddParameterDefinition_MakesDefinitionAvailable_ByIdAndName()
        {
            var definitions = new ParameterAndTableDefinitions();
            var definition = new ParameterDefinition("Parameter", typeof(string), 100);

            definitions.AddParameterDefinition(definition);

            Assert.AreSame(definition, definitions.GetParameterDefinition(100));
            Assert.AreSame(definition, definitions.GetParameterDefinition("Parameter"));
        }

        [TestMethod]
        public void AddParameterDefinition_ThrowsArgumentException_WithDuplicateId()
        {
            var definitions = new ParameterAndTableDefinitions();
            definitions.AddParameterDefinition(new ParameterDefinition("First", typeof(string), 100));

            Assert.ThrowsExactly<ArgumentException>(() =>
                definitions.AddParameterDefinition(new ParameterDefinition("Second", typeof(string), 100)));
        }

        [TestMethod]
        public void AddParameterDefinition_ThrowsArgumentException_WithDuplicateName()
        {
            var definitions = new ParameterAndTableDefinitions();
            definitions.AddParameterDefinition(new ParameterDefinition("Parameter", typeof(string), 100));

            Assert.ThrowsExactly<ArgumentException>(() =>
                definitions.AddParameterDefinition(new ParameterDefinition("Parameter", typeof(string), 101)));
        }

        [TestMethod]
        public void AddParameterDefinition_ThrowsArgumentNullException_WithNullDefinition()
        {
            var definitions = new ParameterAndTableDefinitions();

            Assert.ThrowsExactly<ArgumentNullException>(() => definitions.AddParameterDefinition(null));
        }

        [TestMethod]
        public void AddTableDefinition_MakesDefinitionAvailable_ByTableId()
        {
            var definitions = new ParameterAndTableDefinitions();
            var schema = CreateTableSchema();

            definitions.AddTableDefinition(200, schema);

            Assert.AreSame(schema, definitions.GetTableDefinition(200));
        }

        [TestMethod]
        public void AddTableDefinition_ThrowsArgumentException_WithDuplicateId()
        {
            var definitions = new ParameterAndTableDefinitions();
            definitions.AddTableDefinition(200, CreateTableSchema());

            Assert.ThrowsExactly<ArgumentException>(() => definitions.AddTableDefinition(200, CreateTableSchema()));
        }

        [TestMethod]
        public void AddTableDefinition_ThrowsArgumentNullException_WithNullSchema()
        {
            var definitions = new ParameterAndTableDefinitions();

            Assert.ThrowsExactly<ArgumentNullException>(() => definitions.AddTableDefinition(200, null));
        }

        [TestMethod]
        public void Constructor_ThrowsArgumentNullException_WithNullProtocolModel()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => new ParameterAndTableDefinitions(null));
        }

        [TestMethod]
        public void GetParameterDefinition_ThrowsArgumentException_WithUnknownId()
        {
            var definitions = new ParameterAndTableDefinitions();

            Assert.ThrowsExactly<ArgumentException>(() => definitions.GetParameterDefinition(100));
        }

        [TestMethod]
        public void GetParameterDefinition_ThrowsArgumentException_WithUnknownName()
        {
            var definitions = new ParameterAndTableDefinitions();

            Assert.ThrowsExactly<ArgumentException>(() => definitions.GetParameterDefinition("Unknown"));
        }

        [TestMethod]
        public void GetTableDefinition_ThrowsArgumentException_WithUnknownId()
        {
            var definitions = new ParameterAndTableDefinitions();

            Assert.ThrowsExactly<ArgumentException>(() => definitions.GetTableDefinition(200));
        }

        [TestMethod]
        public void Populate_ThrowsArgumentNullException_WithNullProtocolModel()
        {
            var definitions = new ParameterAndTableDefinitions();

            Assert.ThrowsExactly<ArgumentNullException>(() => definitions.Populate(null));
        }

        private static TableSchema CreateTableSchema()
        {
            var builder = new TableModelBuilder(200);
            builder.AddColumn(columnPid: 201, columnIdx: 0, isKey: true, columnName: "Key");
            return builder.Build().Schema;
        }
    }
}
