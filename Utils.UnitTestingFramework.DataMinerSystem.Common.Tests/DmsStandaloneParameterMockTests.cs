namespace Skyline.DataMiner.Utils.UnitTestingFramework.DataMinerSystem.Common.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.Core.DataMinerSystem.Common;
    using Skyline.DataMiner.Core.DataMinerSystem.Common.Subscription.Monitors;
    using Skyline.DataMiner.Utils.UnitTestingFramework.DataMinerSystem.Common;

    [TestClass]
    [DeploymentItem("TestFiles/Model/Data/protocol.xml")]
    public class DmsStandaloneParameterMockTests
    {
        private readonly string path = "protocol.xml";

        [TestMethod]
        public void Constructor_ThrowsArgumentNullException_WithNullParameterModel()
        {
            Assert.ThrowsExactly<System.ArgumentNullException>(() =>
                new DmsStandaloneParameterMock<string>(null, null));
        }

        [TestMethod]
        public void Element_ReturnsOwningElement_ForRequestedParameter()
        {
            // Arrange
            var mock = new IDmsMock().CreateAgent(agentId: 0).CreateElement(path);

            // Act
            var parameter = mock.Object.GetStandaloneParameter<string>(1001);

            // Assert
            Assert.AreSame(mock.Object, parameter.Element);
        }

        [TestMethod]
        public void GetValue_ReturnsProtocolDefault_BeforeSet()
        {
            // Arrange
            var mock = new IDmsMock().CreateAgent(agentId: 0).CreateElement(path);

            // Act
            var value = mock.Object.GetStandaloneParameter<double?>(1000).GetValue();

            // Assert
            Assert.AreEqual(10.0, value);
        }

        [TestMethod]
        public void Id_ReturnsParameterId_ForRequestedParameter()
        {
            // Arrange
            var mock = new IDmsMock().CreateAgent(agentId: 0).CreateElement(path);

            // Act
            var parameter = mock.Object.GetStandaloneParameter<string>(1001);

            // Assert
            Assert.AreEqual(1001, parameter.Id);
        }

        [TestMethod]
        public void SetValue_PersistsDoubleValue_WithDefaultOverload()
        {
            // Arrange
            var mock = new IDmsMock().CreateAgent(agentId: 0).CreateElement(path);
            var parameter = mock.Object.GetStandaloneParameter<double?>(1000);

            // Act
            parameter.SetValue(42.5);

            // Assert
            Assert.AreEqual(42.5, parameter.GetValue());
        }

        [TestMethod]
        public void SetValue_PersistsNullableIntValue_WithDefaultOverload()
        {
            // Arrange
            var mock = new IDmsMock().CreateAgent(agentId: 0).CreateElement(path);
            var parameter = mock.Object.GetStandaloneParameter<int?>(800);

            // Act
            parameter.SetValue(7);

            // Assert
            Assert.AreEqual(7, parameter.GetValue());
        }

        [TestMethod]
        public void SetValue_PersistsStringValue_WithDefaultOverload()
        {
            // Arrange
            var mock = new IDmsMock().CreateAgent(agentId: 0).CreateElement(path);
            var parameter = mock.Object.GetStandaloneParameter<string>(1001);

            // Act
            parameter.SetValue("new value");

            // Assert
            Assert.AreEqual("new value", parameter.GetValue());
        }

        [TestMethod]
        public void SetValue_PersistsValue_AcrossParameterInstances()
        {
            // Arrange
            var mock = new IDmsMock().CreateAgent(agentId: 0).CreateElement(path);

            // Act
            mock.Object.GetStandaloneParameter<string>(1001).SetValue("persisted");

            // Assert
            Assert.AreEqual("persisted", mock.Object.GetStandaloneParameter<string>(1001).GetValue());
        }

        [TestMethod]
        public void SetValue_PersistsValue_WithExpectedChangesOverload()
        {
            // Arrange
            var mock = new IDmsMock().CreateAgent(agentId: 0).CreateElement(path);
            var parameter = mock.Object.GetStandaloneParameter<string>(1001);

            // Act
            parameter.SetValue("changed", System.TimeSpan.FromSeconds(1), null);

            // Assert
            Assert.AreEqual("changed", parameter.GetValue());
        }

        [TestMethod]
        public void StartValueMonitor_InvokesBothCallbacks_WithDifferentSourceIds()
        {
            // Arrange
            var parameter = new IDmsMock()
                .CreateAgent(agentId: 0)
                .CreateElement(path)
                .Object
                .GetStandaloneParameter<string>(1001);
            var firstInvocations = 0;
            var secondInvocations = 0;
            parameter.StartValueMonitor("first", change => firstInvocations++, false);
            parameter.StartValueMonitor("second", change => secondInvocations++, false);

            // Act
            parameter.SetValue("changed");

            // Assert
            Assert.AreEqual(1, firstInvocations);
            Assert.AreEqual(1, secondInvocations);
        }

        [TestMethod]
        public void StartValueMonitor_InvokesCallback_WhenTimestampChangesWithSameValue()
        {
            // Arrange
            var parameterMock = new IDmsMock()
                .CreateAgent(agentId: 0)
                .CreateElement(path)
                .GetStandaloneParameterMock<string>(1001);
            var numberOfInvocations = 0;
            parameterMock.Object.StartValueMonitor("source", change => numberOfInvocations++, false);

            // Act
            parameterMock.UpdateValue(parameterMock.Value, System.DateTime.MinValue);

            // Assert
            Assert.AreEqual(1, numberOfInvocations);
        }

        [TestMethod]
        public void StartValueMonitor_InvokesCallbackOnChange_WithDefaultOverload()
        {
            // Arrange
            var mock = new IDmsMock().CreateAgent(agentId: 0).CreateElement(path);
            var parameter = mock.Object.GetStandaloneParameter<string>(1001);

            ParamValueChange<string> received = null;
            parameter.StartValueMonitor("source", change => received = change, false);

            // Act
            parameter.SetValue("monitored value");

            // Assert
            Assert.IsNotNull(received);
            Assert.AreEqual("monitored value", received.Value);
            Assert.AreEqual("source", received.MonitorSource);
        }

        [TestMethod]
        public void StartValueMonitor_InvokesCallbackOnChange_WithTimeSpanOverload()
        {
            // Arrange
            var mock = new IDmsMock().CreateAgent(agentId: 0).CreateElement(path);
            var parameter = mock.Object.GetStandaloneParameter<string>(1001);

            ParamValueChange<string> received = null;
            parameter.StartValueMonitor("source", change => received = change, System.TimeSpan.FromSeconds(1), false);

            // Act
            parameter.SetValue("monitored value");

            // Assert
            Assert.IsNotNull(received);
            Assert.AreEqual("monitored value", received.Value);
        }

        [TestMethod]
        public void StartValueMonitor_ReplacesExistingCallback_WithSameSourceId()
        {
            // Arrange
            var parameter = new IDmsMock()
                .CreateAgent(agentId: 0)
                .CreateElement(path)
                .Object
                .GetStandaloneParameter<string>(1001);
            var firstInvocations = 0;
            var secondInvocations = 0;
            parameter.StartValueMonitor("source", change => firstInvocations++, false);
            parameter.StartValueMonitor("source", change => secondInvocations++, false);

            // Act
            parameter.SetValue("changed");

            // Assert
            Assert.AreEqual(0, firstInvocations);
            Assert.AreEqual(1, secondInvocations);
        }

        [TestMethod]
        public void StartValueMonitor_ThrowsArgumentNullException_WithNullCallback()
        {
            var parameter = new IDmsMock()
                .CreateAgent(agentId: 0)
                .CreateElement(path)
                .Object
                .GetStandaloneParameter<string>(1001);

            Assert.ThrowsExactly<System.ArgumentNullException>(() =>
                parameter.StartValueMonitor("source", null, false));
        }

        [TestMethod]
        public void StartValueMonitor_ThrowsArgumentNullException_WithNullSourceId()
        {
            var parameter = new IDmsMock()
                .CreateAgent(agentId: 0)
                .CreateElement(path)
                .Object
                .GetStandaloneParameter<string>(1001);

            Assert.ThrowsExactly<System.ArgumentNullException>(() =>
                parameter.StartValueMonitor(null, change => { }, false));
        }

        [TestMethod]
        public void StopValueMonitor_DoesNotInvokeCallbackAfterStop_WithDefaultOverload()
        {
            // Arrange
            var mock = new IDmsMock().CreateAgent(agentId: 0).CreateElement(path);
            var parameter = mock.Object.GetStandaloneParameter<string>(1001);

            ParamValueChange<string> received = null;
            parameter.StartValueMonitor("source", change => received = change, false);
            parameter.StopValueMonitor("source", false);

            // Act
            parameter.SetValue("value after stop");

            // Assert
            Assert.IsNull(received);
        }

        [TestMethod]
        public void StopValueMonitor_DoesNotInvokeCallbackAfterStop_WithTimeSpanOverload()
        {
            // Arrange
            var mock = new IDmsMock().CreateAgent(agentId: 0).CreateElement(path);
            var parameter = mock.Object.GetStandaloneParameter<string>(1001);

            ParamValueChange<string> received = null;
            parameter.StartValueMonitor("source", change => received = change, false);
            parameter.StopValueMonitor("source", System.TimeSpan.FromSeconds(1), false);

            // Act
            parameter.SetValue("value after stop");

            // Assert
            Assert.IsNull(received);
        }

        [TestMethod]
        public void UpdateValue_UpdatesPublicValue_WithNewValue()
        {
            var parameterMock = new IDmsMock()
                .CreateAgent(agentId: 0)
                .CreateElement(path)
                .GetStandaloneParameterMock<string>(1001);

            parameterMock.UpdateValue("updated");

            Assert.AreEqual("updated", parameterMock.Value);
        }

        [TestMethod]
        public void Value_ReturnsCurrentValue_BeforeUpdate()
        {
            var parameterMock = new IDmsMock()
                .CreateAgent(agentId: 0)
                .CreateElement(path)
                .GetStandaloneParameterMock<double?>(1000);

            Assert.AreEqual(10.0, parameterMock.Value);
        }
    }
}
