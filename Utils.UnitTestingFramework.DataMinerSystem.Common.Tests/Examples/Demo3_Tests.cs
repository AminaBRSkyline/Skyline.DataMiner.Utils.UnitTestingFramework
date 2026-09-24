namespace Utils.UnitTestingFramework.DataMinerSystem.Common.Tests.Examples
{
    using System;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.Net;
    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.Net.Messages;
    using Skyline.DataMiner.Net.Sections;

    [TestClass]
    public class Demo3_Tests
    {
        private const string ModuleId = "demo3-module";

        private static readonly Guid DomDefinitionId = Guid.NewGuid();

        private static readonly SectionDefinitionID SectionDefinitionId = new SectionDefinitionID(Guid.NewGuid());

        private static readonly FieldDescriptorID StatusFieldId = new FieldDescriptorID(Guid.NewGuid());
    }

    internal class DomFieldSetter
    {
        private readonly DomHelper domHelper;
        private readonly SectionDefinitionID sectionDefinitionId;
        private readonly FieldDescriptorID fieldId;

        public DomFieldSetter(DomHelper domHelper, SectionDefinitionID sectionDefinitionId, FieldDescriptorID fieldId)
        {
            this.domHelper = domHelper ?? throw new ArgumentNullException(nameof(domHelper));
            this.sectionDefinitionId = sectionDefinitionId ?? throw new ArgumentNullException(nameof(sectionDefinitionId));
            this.fieldId = fieldId ?? throw new ArgumentNullException(nameof(fieldId));
        }

        public void Set(DomInstanceId instanceId, string value)
        {
            var instance = domHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(instanceId)).SingleOrDefault()
                ?? throw new InvalidOperationException($"DOM instance '{instanceId}' was not found.");

            var section = instance.Sections.SingleOrDefault(s => s.SectionDefinitionID.Equals(sectionDefinitionId))
                ?? throw new InvalidOperationException($"Section '{sectionDefinitionId.Id}' was not found on DOM instance '{instanceId}'.");

            section.AddOrUpdateValue(fieldId, value);

            domHelper.DomInstances.Update(instance);
        }
    }

    internal class DomInstanceChangeCounter : IDisposable
    {
        private readonly IConnection connection;
        private readonly Guid domDefinitionId;
        private bool isWatching;
        private bool disposed;

        public DomInstanceChangeCounter(IConnection connection, Guid domDefinitionId)
        {
            this.connection = connection ?? throw new ArgumentNullException(nameof(connection));
            this.domDefinitionId = domDefinitionId;
        }

        public int NumberOfAdded { get; private set; }

        public int NumberOfUpdated { get; private set; }

        public int NumberOfDeleted { get; private set; }

        public void StartWatching()
        {
            if (isWatching)
            {
                return;
            }

            connection.OnNewMessage += Connection_OnNewMessage;
            isWatching = true;
        }

        public void StopWatching()
        {
            if (!isWatching)
            {
                return;
            }

            connection.OnNewMessage -= Connection_OnNewMessage;
            isWatching = false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    StopWatching();
                }

                disposed = true;
            }
        }

        private void Connection_OnNewMessage(object sender, NewMessageEventArgs e)
        {
            if (!(e.Message is DomInstancesChangedEventMessage message))
            {
                return;
            }

            NumberOfAdded += CountMatching(message.Created);
            NumberOfUpdated += CountMatching(message.Updated);
            NumberOfDeleted += CountMatching(message.Deleted);
        }

        private int CountMatching(System.Collections.Generic.IEnumerable<DomInstance> instances)
        {
            return instances.Count(instance => instance.DomDefinitionId.Id.Equals(domDefinitionId));
        }
    }
}