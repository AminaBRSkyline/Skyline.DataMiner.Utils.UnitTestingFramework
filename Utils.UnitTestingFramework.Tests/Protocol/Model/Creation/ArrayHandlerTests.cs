namespace Skyline.DataMiner.Utils.UnitTestingFramework.Tests.Protocol.Model.Creation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Skyline.DataMiner.Utils.UnitTestingFramework.Common;
    using Skyline.DataMiner.Utils.UnitTestingFramework.Common.Model.Creation;

    [TestClass]
    public class ArrayHandlerTests
    {
        [TestMethod]
        [DeploymentItem(@"TestFiles\Model\Data\protocol.xml")]
        public void LoadDefaultForParameterTest_ValidInputTable_IsEqual()
        {
            // Arrange
            var path = @"protocol.xml";
            var protocolModel = ProtocolModelBuilder.Build(path);
            var parameter = protocolModel.Protocol.Params.FirstOrDefault(x => x.Id.Value == 900);
            Assert.IsNotNull(parameter);
            HashSet<int> excludedPids = new HashSet<int>();

            // Act
            var arrayHandler = new TableModelCreator(excludedPids);
            var protocolModelParameterFinder = new ProtocolModelParameterFinder(protocolModel);
            var tableModel = arrayHandler.CreateTableModelFromArrayOptions(parameter, protocolModelParameterFinder);

            // Assert
            Assert.AreEqual(5, tableModel.Definition.ColumnDefinitions.Count);
            Assert.AreEqual(901, tableModel.Definition.ColumnDefinitions[0].Pid);
            Assert.AreEqual(902, tableModel.Definition.ColumnDefinitions[1].Pid);
            Assert.AreEqual(903, tableModel.Definition.ColumnDefinitions[2].Pid);
            Assert.AreEqual(904, tableModel.Definition.ColumnDefinitions[3].Pid);
            Assert.AreEqual(905, tableModel.Definition.ColumnDefinitions[4].Pid);
            Assert.AreEqual(900, tableModel.TableId);
            Assert.AreEqual(0, tableModel.Definition.PrimaryKeyColumn.Idx);
            Assert.AreEqual(901, tableModel.Definition.PrimaryKeyColumn.Pid);
        }

        [TestMethod]
        [DeploymentItem("TestFiles/Model/Data/protocol_With_Failures.xml")]
        public void LoadDefaultForParameterTest_SamePidDifferentIdx_AddsTwoEntries()
        {
            // Arrange
            var path = @"protocol_With_Failures.xml";
            var protocolModel = ProtocolModelBuilder.Build(path);
            var parameter = protocolModel.Protocol.Params.FirstOrDefault(x => x.Id.Value == 910);
            Assert.IsNotNull(parameter);
            HashSet<int> excludedPids = new HashSet<int>();

            // Act
            var arrayHandler = new TableModelCreator(excludedPids);
            var protocolModelParameterFinder = new ProtocolModelParameterFinder(protocolModel);
            var tableModel = arrayHandler.CreateTableModelFromArrayOptions(parameter, protocolModelParameterFinder);

            // Assert
            Assert.AreEqual(2, tableModel.Definition.ColumnDefinitions.Count);
            Assert.AreEqual(901, tableModel.Definition.ColumnDefinitions[1].Pid);
            Assert.AreEqual(910, tableModel.TableId);
            Assert.AreEqual(1, tableModel.Definition.PrimaryKeyColumn.Idx);
            Assert.AreEqual(901, tableModel.Definition.PrimaryKeyColumn.Pid);
        }

        [TestMethod]
        [DeploymentItem("TestFiles/Model/Data/protocol_With_Failures.xml")]
        public void LoadDefaultForParameterTest_SameIdxDifferentPid_AddsFirstEntry()
        {
            // Arrange
            var path = @"protocol_With_Failures.xml";
            var protocolModel = ProtocolModelBuilder.Build(path);
            var parameter = protocolModel.Protocol.Params.FirstOrDefault(x => x.Id.Value == 920);
            Assert.IsNotNull(parameter);
            HashSet<int> excludedPids = new HashSet<int>();

            // Act
            var arrayHandler = new TableModelCreator(excludedPids);

            // Act & Assert
            var protocolModelParameterFinder = new ProtocolModelParameterFinder(protocolModel);
            Assert.ThrowsExactly<InvalidOperationException>(
                () => arrayHandler.CreateTableModelFromArrayOptions(parameter, protocolModelParameterFinder));
        }

        [TestMethod]
        [DeploymentItem("TestFiles/Model/Data/protocol_With_Failures.xml")]
        public void LoadDefaultForParameterTest_NoIdxCorrespondingToPK_()
        {
            // Arrange
            var path = @"protocol_With_Failures.xml";
            var protocolModel = ProtocolModelBuilder.Build(path);
            var parameter = protocolModel.Protocol.Params.FirstOrDefault(x => x.Id.Value == 930);
            Assert.IsNotNull(parameter);
            HashSet<int> excludedPids = new HashSet<int>();

            // Act
            var arrayHandler = new TableModelCreator(excludedPids);

            // Act & Assert
            var protocolModelParameterFinder = new ProtocolModelParameterFinder(protocolModel);

            Assert.ThrowsExactly<InvalidOperationException>(
                () => arrayHandler.CreateTableModelFromArrayOptions(parameter, protocolModelParameterFinder));
        }
    }
}