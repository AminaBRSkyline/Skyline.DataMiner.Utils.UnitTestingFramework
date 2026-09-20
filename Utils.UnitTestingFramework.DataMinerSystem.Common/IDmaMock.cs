namespace Skyline.DataMiner.Utils.UnitTestingFramework.DataMinerSystem.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Moq;
    using Skyline.DataMiner.Core.DataMinerSystem.Common;
    using Skyline.DataMiner.Utils.UnitTestingFramework.Common;

    /// <summary>
    /// A pre-arranged mock of <see cref="IDma"/>.
    /// </summary>
    public class IDmaMock : Mock<IDma>
    {
        private readonly Cache cache;
        private AgentState state = AgentState.Running;

        internal List<int> ElementIds { get; } = new List<int>();

        internal List<int> ServiceIds { get; } = new List<int>();

        /// Gets or sets the host name returned by the mock.
        public AgentState State
        {
            get
            {
                return state;
            }

            set
            {
                if (!Enum.IsDefined(typeof(AgentState), value))
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                state = value;
            }
        }
        /// Gets or sets the host name returned by the mock.
        public string HostName { get; set; } = "localhost";
        /// Gets or sets the Scheduler component returned by the mock.
        public IDmsScheduler Scheduler { get; set; } = new Mock<IDmsScheduler>().Object;

        /// Gets or sets the version information returned by the mock.
        public string VersionInfo { get; set; } = "0.0.0.0";

        /// <summary>
        /// Initializes a new instance of the <see cref="IDmaMock"/> class.
        /// </summary>
        /// <param name="id">The ID of the DataMiner Agent.</param>
        /// <param name="name">The name of the DataMiner Agent.</param>
        internal IDmaMock(Cache cache, int id, string name = "Agent")
        {
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));

            Setup(dma => dma.Id).Returns(id);
            Setup(dma => dma.Name).Returns(name);
            Setup(dma => dma.Dms).Returns(() => cache.GetDms().Object);
            Setup(dma => dma.HostName).Returns(() => HostName);

            Setup(dma => dma.State).Returns(() => State);

            Setup(dma => dma.Scheduler).Returns(() => Scheduler);

            Setup(dma => dma.VersionInfo).Returns(() => VersionInfo);

            Setup(dma => dma.GetElements()).Returns(() => ElementIds.Select(elementId => cache.GetElement(id, elementId)).Where(elementMock => elementMock != null && elementMock.Object.State != ElementState.Deleted).Select(elementMock => elementMock.Object).ToList());
            Setup(dma => dma.GetServices()).Returns(() => ServiceIds.Select(serviceId => cache.GetService(id, serviceId)).Where(serviceMock => serviceMock != null).Select(serviceMock => serviceMock.Object).ToList());
            Setup(dma => dma.CreateService(It.IsAny<ServiceConfiguration>())).Returns((ServiceConfiguration configuration) => CreateService(configuration));
            Setup(dma => dma.ElementExists(It.IsAny<DmsElementId>())).Returns((DmsElementId elementId) =>
            {
                ValidateElementId(elementId);

                var elementMock = cache.GetElement(elementId.AgentId, elementId.ElementId);
                return elementId.AgentId == id && elementMock != null && elementMock.Object.State != ElementState.Deleted;
            });
            Setup(dma => dma.ElementExists(It.IsAny<string>())).Returns((string elementName) =>
            {
                ValidateElementName(elementName);

                var elementMock = cache.GetElement(id, elementName);
                return elementMock != null && elementMock.Object.State != ElementState.Deleted;
            });
            Setup(dma => dma.ServiceExists(It.IsAny<DmsServiceId>())).Returns((DmsServiceId serviceId) =>
            {
                ValidateServiceId(serviceId);

                return serviceId.AgentId == id && cache.GetService(serviceId.AgentId, serviceId.ServiceId) != null;
            });
            Setup(dma => dma.ServiceExists(It.IsAny<string>())).Returns((string serviceName) =>
            {
                ValidateServiceName(serviceName);

                var serviceMock = cache.GetService(serviceName);

                return serviceMock != null && serviceMock.Object.AgentId == id;
            });
            Setup(dma => dma.GetService(It.IsAny<DmsServiceId>())).Returns((DmsServiceId serviceId) =>
            {
                ValidateServiceId(serviceId);

                var serviceMock = cache.GetService(serviceId.AgentId, serviceId.ServiceId);

                if (serviceMock == null || serviceMock.Object.AgentId != id)
                {
                    throw new ServiceNotFoundException(serviceId);
                }

                return serviceMock.Object;
            });
            Setup(dma => dma.GetService(It.IsAny<string>())).Returns((string serviceName) =>
            {
                ValidateServiceName(serviceName);

                var serviceMock = cache.GetService(serviceName);

                if (serviceMock == null || serviceMock.Object.AgentId != id)
                {
                    throw new ServiceNotFoundException(serviceName);
                }

                return serviceMock.Object;
            });
            Setup(dma => dma.GetElement(It.IsAny<DmsElementId>())).Returns((DmsElementId dmsElementId) =>
            {
                ValidateElementId(dmsElementId);

                var elementMock = cache.GetElement(dmsElementId.AgentId, dmsElementId.ElementId);
                if (dmsElementId.AgentId != id || elementMock == null || elementMock.Object.State == ElementState.Deleted)
                {
                    throw new ElementNotFoundException(dmsElementId);
                }

                return elementMock.Object;

            });

            Setup(dma => dma.GetElement(It.IsAny<string>())).Returns((string elementName) =>
            {
                ValidateElementName(elementName);

                var elementMock = cache.GetElement(id, elementName);

                if (elementMock == null || elementMock.Object.State == ElementState.Deleted)
                {
                    throw new ElementNotFoundException(elementName);
                }

                return elementMock.Object;

            });

            Setup(dma => dma.Exists()).Returns(() => cache.GetDma(id) != null);

            Setup(dma => dma.IsVersionHigher(It.IsAny<string>())).Returns((string versionNumber) => ParseVersion(versionNumber).CompareTo(ParseVersion(VersionInfo)) > 0);
        }

        /// <summary>
        /// Creates an element mock hosted on this DataMiner Agent.
        /// </summary>
        /// <param name="protocolName">The protocol name, or a protocol XML path for backwards compatibility.</param>
        /// <param name="id">The element ID.</param>
        /// <param name="name">The element name.</param>
        /// <param name="protocolVersion">The protocol version.</param>
        /// <returns>The created element mock.</returns>
        public IDmsElementMock CreateElement(string protocolName, int id = 0, string name = "Element", string protocolVersion = IDmsProtocolMock.DefaultVersion)
        {
            var protocolMock = cache.GetProtocol(protocolName, protocolVersion);

            if (protocolMock == null && protocolName != null && protocolName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            {
                var protocolModel = ProtocolModelBuilder.Build(protocolName);
                var modelName = protocolModel.Protocol.Name?.Value;
                var modelVersion = protocolModel.Protocol.Version?.Value;

                protocolMock = cache.GetProtocol(modelName, modelVersion);
                if (protocolMock == null)
                {
                    protocolMock = new IDmsProtocolMock(protocolModel, protocolName);
                    cache.AddProtocol(protocolMock);
                }
            }

            if (protocolMock == null)
            {
                throw new ProtocolNotFoundException(protocolName, protocolVersion);
            }

            return CreateElement(protocolMock, id, name);
        }

        internal IDmsElementMock CreateElement(IDmsProtocolMock protocolMock, int id, string name)
        {
            if (protocolMock == null)
            {
                throw new ArgumentNullException(nameof(protocolMock));
            }

            var elementMock = new IDmsElementMock(cache, protocolMock.Name, protocolMock.ReferencedVersion, id, Object.Id, name);

            cache.AddElement(elementMock);

            return elementMock;
        }

        /// <summary>
        /// Creates a service mock hosted on this DataMiner Agent.
        /// </summary>
        /// <param name="serviceId">The service ID.</param>
        /// <param name="name">The service name.</param>
        /// <returns>The created service mock.</returns>
        public IDmsServiceMock CreateService(int serviceId, string name = "Service")
        {
            var serviceMock = new IDmsServiceMock(cache, serviceId, Object.Id, name);

            cache.AddService(serviceMock);

            return serviceMock;
        }

        private DmsServiceId CreateService(ServiceConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (cache.GetDma(Object.Id) == null)
            {
                throw new AgentNotFoundException(Object.Id);
            }

            foreach (var view in configuration.Views)
            {
                var viewMock = view == null ? null : cache.GetView(view.Id);

                if (viewMock == null || !ReferenceEquals(viewMock.Object, view))
                {
                    throw new IncorrectDataException("The service configuration contains a view that does not belong to this DataMiner System.");
                }
            }

            var serviceMock = CreateService(GetNextServiceId(), configuration.Name);
            serviceMock.Description = configuration.Description;

            foreach (var view in configuration.Views)
            {
                serviceMock.AddView(view.Id);
            }

            return serviceMock.Object.DmsServiceId;
        }

        internal int GetNextElementId()
        {
            return ElementIds.Count == 0 ? 1 : ElementIds.Max() + 1;
        }

        internal int GetNextServiceId()
        {
            return ServiceIds.Count == 0 ? 1 : ServiceIds.Max() + 1;
        }

        private static void ValidateElementId(DmsElementId elementId)
        {
            if (elementId.AgentId < 1 || elementId.ElementId < 1)
            {
                throw new ArgumentException("The DataMiner Agent ID and element ID must be positive.", nameof(elementId));
            }
        }

        private static void ValidateElementName(string elementName)
        {
            if (elementName == null)
            {
                throw new ArgumentNullException(nameof(elementName));
            }

            if (String.IsNullOrWhiteSpace(elementName))
            {
                throw new ArgumentException("The element name cannot be empty or white space.", nameof(elementName));
            }
        }

        private static void ValidateServiceId(DmsServiceId serviceId)
        {
            if (serviceId.AgentId < 1 || serviceId.ServiceId < 1)
            {
                throw new ArgumentException("The DataMiner Agent ID and service ID must be positive.", nameof(serviceId));
            }
        }

        private static void ValidateServiceName(string serviceName)
        {
            if (serviceName == null)
            {
                throw new ArgumentNullException(nameof(serviceName));
            }

            if (String.IsNullOrWhiteSpace(serviceName))
            {
                throw new ArgumentException("The service name cannot be empty or white space.", nameof(serviceName));
            }
        }

        private static Version ParseVersion(string versionNumber)
        {
            if (versionNumber == null)
            {
                throw new ArgumentNullException(nameof(versionNumber));
            }

            var match = Regex.Match(versionNumber, @"^(?<version>\d+\.\d+\.\d+\.\d+)(?:-CU\d+)?$");

            if (!match.Success || !Version.TryParse(match.Groups["version"].Value, out var version))
            {
                throw new ArgumentException("The version number has an invalid format.", nameof(versionNumber));
            }

            return version;
        }
    }
}
