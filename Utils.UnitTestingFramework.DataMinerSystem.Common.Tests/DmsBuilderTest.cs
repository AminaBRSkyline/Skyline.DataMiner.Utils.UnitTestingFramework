using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyline.DataMiner.Utils.UnitTestingFramework.DataMinerSystem.Common.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Skyline.DataMiner.Core.DataMinerSystem.Common;

    [TestClass]
    public class DmsBuilderTests
    {
        private const string ProtocolName = "UnitTestingFrameworkUseCases";

        [TestMethod]
        public void Build_WithViewAndDma_CreatesBothInSameDms()
        {
            // Act
            var dmsMock = new DmsBuilder()
                .WithView(viewId: 55)
                .WithDma(id: 1)
                .Build();

            // Assert
            Assert.IsTrue(dmsMock.Object.ViewExists(55));
            Assert.IsTrue(dmsMock.Object.AgentExists(1));
            Assert.AreSame(dmsMock.Object, dmsMock.Object.GetView(55).Dms);
            Assert.AreSame(dmsMock.Object, dmsMock.Object.GetAgent(1).Dms);
        }

        [DeploymentItem("TestFiles/Model/Data/protocol.xml")]
        [TestMethod]
        public void Build_WithElementsAndView_ConnectsElementsToDmaAndView()
        {
            // Act
            var dmsMock = new DmsBuilder()
                .WithProtocol("protocol.xml")
                .WithView(viewId: 55)
                .WithDma(id: 1, configure: dma => dma
                    .WithElement(id: 33, name: "Element 33", protocolName: ProtocolName, configure: element => element.UnderView(viewId: 55))
                    .WithElement(id: 44, name: "Element 44", protocolName: ProtocolName))
                .Build();

            // Assert
            var dma = dmsMock.Object.GetAgent(1);
            var firstElement = dma.GetElement("Element 33");
            var secondElement = dma.GetElement("Element 44");
            var view = dmsMock.Object.GetView(55);

            Assert.AreEqual(2, dma.GetElements().Count);
            Assert.AreSame(dma, firstElement.Host);
            Assert.AreSame(dma, secondElement.Host);
            Assert.AreSame(view, firstElement.Views.Single());
            Assert.AreSame(firstElement, view.Elements.Single());
            Assert.IsEmpty(secondElement.Views);
            Assert.AreSame(firstElement.Protocol, secondElement.Protocol);
            Assert.AreSame(firstElement.Protocol, dmsMock.Object.GetProtocols().Single());
        }

        [DeploymentItem("TestFiles/Model/Data/protocol.xml")]
        [TestMethod]
        public void Build_WithTable_AddsRowsToElementTable()
        {
            // Arrange
            var row = new object[] { "one", "one-desc", 3.0, 4.0, 5.0 };

            // Act
            var dmsMock = new DmsBuilder()
                .WithProtocol("protocol.xml")
                .WithDma(id: 1, configure: dma => dma.WithElement(id: 33, name: "Element 33", protocolName: ProtocolName, configure: element => element.WithTable(tableId: 900, rows: new object[][] { row })))
                .Build();

            // Assert
            var table = dmsMock.Object.GetAgent(1).GetElement("Element 33").GetTable(900);
            Assert.IsTrue(table.RowExists("one"));
            CollectionAssert.AreEqual(row, table.GetRow("one"));
        }

        [DeploymentItem("TestFiles/Model/Data/protocol.xml")]
        [TestMethod]
        public void Build_WithTableNullRows_ThrowsArgumentNullException()
        {
            // Arrange
            var builder = new DmsBuilder()
                .WithProtocol("protocol.xml")
                .WithDma(id: 1, configure: dma => dma.WithElement(id: 33, name: "Element 33", protocolName: ProtocolName, configure: element => element.WithTable(tableId: 900, rows: null)));

            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => builder.Build());
        }

        [DeploymentItem("TestFiles/Model/Data/protocol.xml")]
        [TestMethod]
        public void Build_WithProtocol_AddsProtocolFromXmlToDms()
        {
            // Act
            var dmsMock = new DmsBuilder().WithProtocol("protocol.xml").Build();

            // Assert
            var protocol = dmsMock.Object.GetProtocols().Single();
            Assert.AreEqual(ProtocolName, protocol.Name);
            Assert.AreEqual("1.0.0.1", protocol.ReferencedVersion);
            Assert.AreEqual(ProtocolType.Virtual, protocol.Type);
            Assert.IsTrue(dmsMock.Object.ProtocolExists(ProtocolName, "1.0.0.1"));
            Assert.AreSame(protocol, dmsMock.Object.GetProtocol(ProtocolName, "1.0.0.1"));
            Assert.IsFalse(dmsMock.Object.ProtocolExists(ProtocolName, "2.0.0.0"));
            Assert.ThrowsExactly<ProtocolNotFoundException>(() => dmsMock.Object.GetProtocol(ProtocolName, "2.0.0.0"));
        }

        [DeploymentItem("TestFiles/Model/Data/protocol.xml")]
        [TestMethod]
        public void Build_WithProtocolVersion_UsesMatchingProtocol()
        {
            // Act
            var dmsMock = new DmsBuilder()
                .WithProtocol("protocol.xml")
                .WithDma(id: 1, configure: dma => dma.WithElement(id: 33, name: "Element 33", protocolName: ProtocolName, protocolVersion: "1.0.0.1"))
                .Build();

            // Assert
            var protocol = dmsMock.Object.GetProtocol(ProtocolName, "1.0.0.1");
            var element = dmsMock.Object.GetElement("Element 33");
            Assert.AreSame(protocol, element.Protocol);
        }

        [DeploymentItem("TestFiles/Model/Data/protocol.xml")]
        [TestMethod]
        public void Build_UnknownProtocolVersion_ThrowsInvalidOperationException()
        {
            // Arrange
            var builder = new DmsBuilder()
                .WithProtocol("protocol.xml")
                .WithDma(id: 1, configure: dma => dma.WithElement(id: 33, name: "Element 33", protocolName: ProtocolName, protocolVersion: "2.0.0.0"));

            // Act & Assert
            Assert.ThrowsExactly<InvalidOperationException>(() => builder.Build());
        }

        [TestMethod]
        public void Build_UnknownProtocol_ThrowsInvalidOperationException()
        {
            // Arrange
            var builder = new DmsBuilder()
                .WithDma(id: 1, configure: dma => dma.WithElement(id: 33, name: "Element 33", protocolName: "Unknown"));

            // Act & Assert
            Assert.ThrowsExactly<InvalidOperationException>(() => builder.Build());
        }

        [TestMethod]
        public void Build_MissingProtocolXml_ThrowsFileNotFoundException()
        {
            // Arrange
            var builder = new DmsBuilder().WithProtocol("missing-protocol.xml");

            // Act & Assert
            Assert.ThrowsExactly<System.IO.FileNotFoundException>(() => builder.Build());
        }

        [TestMethod]
        public void WithDma_ConfigurationRunsOnBuild()
        {
            // Arrange
            var configured = false;
            var builder = new DmsBuilder().WithDma(id: 1, configure: dma => configured = true);

            Assert.IsFalse(configured);

            // Act
            var dmsMock = builder.Build();

            // Assert
            Assert.IsTrue(configured);
            Assert.IsTrue(dmsMock.Object.AgentExists(1));
        }

        [DeploymentItem("TestFiles/Model/Data/protocol.xml")]
        [TestMethod]
        public void WithElement_ConfigurationRunsOnBuild()
        {
            // Arrange
            var configured = false;
            var builder = new DmsBuilder()
                .WithProtocol("protocol.xml")
                .WithView(viewId: 55)
                .WithDma(id: 1, configure: dma => dma.WithElement(id: 33, name: "Element 33", protocolName: ProtocolName, configure: element =>
                {
                    configured = true;
                    element.UnderView(viewId: 55);
                }));

            Assert.IsFalse(configured);

            // Act
            var dmsMock = builder.Build();

            // Assert
            Assert.IsTrue(configured);
            Assert.AreSame(dmsMock.Object.GetView(55), dmsMock.Object.GetElement("Element 33").Views.Single());
        }

        [DeploymentItem("TestFiles/Model/Data/protocol.xml")]
        [TestMethod]
        public void Build_SameProtocolTwice_ThrowsArgumentException()
        {
            var builder = new DmsBuilder()
                .WithProtocol("protocol.xml")
                .WithProtocol("protocol.xml");

            Assert.ThrowsExactly<ArgumentException>(() => builder.Build());
        }

        [DeploymentItem("TestFiles/Model/Data/protocol.xml")]
        [TestMethod]
        public void Build_WithParameter_SetsStandaloneParameter()
        {
            var dmsMock = new DmsBuilder()
                .WithProtocol("protocol.xml")
                .WithDma(id: 1, configure: dma => dma.WithElement(id: 33, name: "Element 33", protocolName: ProtocolName, configure: element => element.WithParameter<int?>(800, 7)))
                .Build();

            var value = dmsMock.Object.GetElement("Element 33").GetStandaloneParameter<int?>(800).GetValue();

            Assert.AreEqual((int?)7, value);
        }
    }
}
