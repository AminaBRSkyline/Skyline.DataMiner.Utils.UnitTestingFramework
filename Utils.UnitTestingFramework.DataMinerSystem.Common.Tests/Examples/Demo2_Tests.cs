namespace Utils.UnitTestingFramework.DataMinerSystem.Common.Tests.Examples
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Skyline.DataMiner.Core.DataMinerSystem.Common;
    using Skyline.DataMiner.Net;
    using Skyline.DataMiner.Net.Messages;

    [TestClass]
    [DeploymentItem("Examples/protocol.xml", "Examples")]
    public class Demo2_Tests
    {
        public void ConnectionListener_TrackParameterChanges_ShouldReturnNumberOfInvokations()
        {
            // Arrange
            // TODO: Setup DMS with few elements with same parameter to test parameter change events.

            // Act
            // TODO: Create a ConnectionListener and track one element for parameter change
            // Start tracking specific element for parameter changes, set parameter on all elements and stop tracking.

            // Assert
            // TODO: Assert that only once we got message for parameter change event for the tracked element, and not for other elements.
        }
    }


    internal class ConnectionListener
    {
        private int numberOfInvokations = 0;
        private readonly string updateSubscriptionId = "ParameterChangedSubscription";
        private readonly IConnection connection;
        private bool isTracking;

        public ConnectionListener(IConnection connection)
        {
            this.connection = connection;
        }

        public void StartTracking(DmsElementId elementId)
        {
            numberOfInvokations = 0;
            connection.OnNewMessage += Connection_OnNewMessage;
            connection.AddSubscription(updateSubscriptionId, BuildChannelUpdateFilter(elementId));
            connection.Subscribe();
            isTracking = true;
        }

        public int StopTracking()
        {
            return numberOfInvokations;
        }

        private void Connection_OnNewMessage(object sender, NewMessageEventArgs e)
        {
            if (isTracking && e.Message is ParameterChangeEventMessage parameterChangeEventMessage)
            {
                Interlocked.Increment(ref numberOfInvokations);
            }
        }

        private static SubscriptionFilter[] BuildChannelUpdateFilter(DmsElementId elementId)
        {
            var subscriptions = new List<SubscriptionFilter>
            {
                new SubscriptionFilterElement(typeof(ParameterChangeEventMessage), elementId.AgentId, elementId.ElementId)
            };

            return subscriptions.ToArray();
        }
    }
}
