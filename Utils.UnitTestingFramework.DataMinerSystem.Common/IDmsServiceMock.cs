namespace Skyline.DataMiner.Utils.UnitTestingFramework.DataMinerSystem.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Moq;

    using Skyline.DataMiner.Core.DataMinerSystem.Common;
    using Skyline.DataMiner.Core.DataMinerSystem.Common.Properties;

    /// <summary>
    /// A pre-arranged mock of <see cref="IDmsService"/>.
    /// </summary>
    public class IDmsServiceMock : Mock<IDmsService>
    {
        private readonly Cache cache;
        private readonly int agentId;
        private readonly int id;
        private string name;

        internal List<int> ViewIds { get; } = new List<int>();

        /// <summary>
        /// Gets or sets the advanced settings returned by the mock.
        /// </summary>
        public IAdvancedServiceSettings AdvancedSettings { get; set; } = new Mock<IAdvancedServiceSettings>().Object;

        public string Description { get; set; } = String.Empty;

        /// <summary>
        /// Gets or sets the parameter settings returned by the mock.
        /// </summary>
        public IServiceParamsSettings ParameterSettings { get; set; } = new Mock<IServiceParamsSettings>().Object;

        /// <summary>
        /// Gets or sets the properties returned by the mock.
        /// </summary>
        public IPropertyCollection<IDmsServiceProperty, IDmsServicePropertyDefinition> Properties { get; set; } = CreateEmptyPropertyCollection();

        /// <summary>
        /// Gets or sets the replication settings returned by the mock.
        /// </summary>
        public IReplicationServiceSettings ReplicationSettings { get; set; } = new Mock<IReplicationServiceSettings>().Object;

        /// <summary>
        /// Gets or sets the state returned by the mock.
        /// </summary>
        public IServiceState State { get; set; } = new Mock<IServiceState>().Object;

        /// <summary>
        /// Adds this service to the specified view.
        /// </summary>
        /// <param name="viewId">The view ID.</param>
        public void AddView(int viewId)
        {
            EnsureServiceExists();

            var viewMock = cache.GetView(viewId);

            if (viewMock == null)
            {
                throw new ViewNotFoundException(viewId);
            }

            if (!ViewIds.Contains(viewId))
            {
                ViewIds.Add(viewId);
            }

            if (!viewMock.ServiceIds.Any(serviceId => serviceId.AgentId == agentId && serviceId.ServiceId == id))
            {
                viewMock.ServiceIds.Add(new DmsServiceId(agentId, id));
            }
        }

        internal IDmsServiceMock(Cache cache, int id, int agentId, string name)
        {
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
            this.id = id;
            this.agentId = agentId;

            ValidateName(name);
            this.name = name;

            Setup(service => service.AdvancedSettings).Returns(() => AdvancedSettings);
            Setup(service => service.Id).Returns(id);
            Setup(service => service.AgentId).Returns(agentId);
            Setup(service => service.DmsServiceId).Returns(new DmsServiceId(agentId, id));
            Setup(service => service.Host).Returns(() => cache.GetDma(agentId)?.Object);
            Setup(service => service.Description).Returns(() => Description);
            SetupSet(service => service.Description = It.IsAny<string>()).Callback((string value) => Description = value);
            Setup(service => service.ParameterSettings).Returns(() => ParameterSettings);
            Setup(service => service.Properties).Returns(() => Properties);
            Setup(service => service.ReplicationSettings).Returns(() => ReplicationSettings);
            Setup(service => service.Name).Returns(() => this.name);
            SetupSet(service => service.Name = It.IsAny<string>()).Callback((string value) => { ValidateName(value); this.cache.UpdateServiceName(agentId, id, this.name, value); this.name = value; });
            Setup(service => service.Views).Returns(() => new HashSet<IDmsView>(ViewIds.Select(viewId => cache.GetView(viewId)).Where(viewMock => viewMock != null).Select(viewMock => viewMock.Object)));
            Setup(service => service.Exists()).Returns(() => cache.GetService(agentId, id) != null);
            Setup(service => service.Update()).Callback(() => EnsureServiceExists());
            Setup(service => service.GetState()).Returns(() => GetState());
            Setup(service => service.Delete()).Callback(() => Delete());
            Setup(service => service.Duplicate(It.IsAny<string>(), It.IsAny<IDma>())).Returns((string newServiceName, IDma agent) => Duplicate(newServiceName, agent));
        }

        private void Delete()
        {
            EnsureServiceExists();
            cache.RemoveService(agentId, id);
        }

        private IDmsService Duplicate(string newServiceName, IDma agent)
        {
            EnsureServiceExists();
            ValidateName(newServiceName);

            if (agent == null)
            {
                throw new ArgumentNullException(nameof(agent));
            }

            var targetAgentMock = cache.GetDma(agent.Id);

            if (targetAgentMock == null || !ReferenceEquals(targetAgentMock.Object, agent))
            {
                throw new AgentNotFoundException(agent.Id);
            }

            var duplicate = targetAgentMock.CreateService(targetAgentMock.GetNextServiceId(), newServiceName);
            duplicate.AdvancedSettings = AdvancedSettings;
            duplicate.Description = Description;
            duplicate.ParameterSettings = ParameterSettings;
            duplicate.Properties = Properties;
            duplicate.ReplicationSettings = ReplicationSettings;

            return duplicate.Object;
        }

        private void EnsureServiceExists()
        {
            if (cache.GetService(agentId, id) == null)
            {
                throw new ServiceNotFoundException(new DmsServiceId(agentId, id));
            }
        }

        private IServiceState GetState()
        {
            EnsureServiceExists();

            return State;
        }

        private void ValidateName(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (String.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("The service name cannot be empty or white space.", nameof(value));
            }

            if (value.Length > 200)
            {
                throw new ArgumentException("The service name cannot exceed 200 characters.", nameof(value));
            }

            if (value[0] == '.' || value[value.Length - 1] == '.' || value[0] == ' ' || value[value.Length - 1] == ' ')
            {
                throw new ArgumentException("The service name cannot start or end with a dot or space.", nameof(value));
            }

            if (value.IndexOfAny(new[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|', '°', ';' }) >= 0)
            {
                throw new ArgumentException("The service name contains a forbidden character.", nameof(value));
            }

            if (value.IndexOf('%') != value.LastIndexOf('%'))
            {
                throw new ArgumentException("The service name cannot contain more than one percentage character.", nameof(value));
            }

            var existingServiceMock = cache.GetService(value);

            if (existingServiceMock != null && !ReferenceEquals(existingServiceMock, this))
            {
                throw new ArgumentException("A service with the specified name already exists.", nameof(value));
            }
        }

        private static IPropertyCollection<IDmsServiceProperty, IDmsServicePropertyDefinition> CreateEmptyPropertyCollection()
        {
            var propertiesMock = new Mock<IPropertyCollection<IDmsServiceProperty, IDmsServicePropertyDefinition>>();

            propertiesMock.Setup(properties => properties.Count).Returns(0);
            propertiesMock.Setup(properties => properties.GetEnumerator()).Returns(() => new List<IDmsServiceProperty>().GetEnumerator());

            return propertiesMock.Object;
        }
    }
}
