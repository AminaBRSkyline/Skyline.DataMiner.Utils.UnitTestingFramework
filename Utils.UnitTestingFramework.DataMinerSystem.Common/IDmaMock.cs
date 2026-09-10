using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyline.DataMiner.Utils.UnitTestingFramework.DataMinerSystem.Common
{
    using System.Collections.Generic;
    using System;

    using Moq;
    using Skyline.DataMiner.Utils.UnitTestingFramework.DataMinerSystem.Common;
    using Skyline.DataMiner.Core.DataMinerSystem.Common;
    using System.Text.RegularExpressions;

    /// <summary>
    /// A pre-arranged mock of <see cref="IDma"/>.
    /// </summary>
    public class IDmaMock : Mock<IDma>
    {
        private readonly Cache cache;
        private AgentState state = AgentState.Running;
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
            Setup(dma => dma.HostName).Returns(() => HostName);

            Setup(dma => dma.State).Returns(() => State);

            Setup(dma => dma.Scheduler).Returns(() => Scheduler);

            Setup(dma => dma.VersionInfo).Returns(() => VersionInfo);

            Setup(dma => dma.GetElements()).Returns(() => cache.GetElements(id).Where(element => element.Object.State != ElementState.Deleted).Select(element => element.Object).ToList());
            Setup(dma => dma.GetServices()).Returns(() => cache.GetServices(id).Select(service => service.Object).ToList());

            Setup(dma => dma.ElementExists(It.IsAny<DmsElementId>())).Returns((DmsElementId elementId) =>
            {
                var elementMock = cache.GetElement(elementId.AgentId, elementId.ElementId);
                return elementMock != null && elementMock.Object.State != ElementState.Deleted;
            });
            Setup(dma => dma.ElementExists(It.IsAny<string>())).Returns((string elementName) =>
            {
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
                var elementMock = cache.GetElement(dmsElementId.AgentId, dmsElementId.ElementId);
                if (elementMock == null || elementMock.Object.State == ElementState.Deleted)
                {
                    throw new ElementNotFoundException(dmsElementId);
                }

                return elementMock.Object;

            });

            Setup(dma => dma.GetElement(It.IsAny<string>())).Returns((string elementName) =>
            {
                if (elementName == null){
                    throw new ArgumentNullException(nameof(elementName));
                }

                if (String.IsNullOrWhiteSpace(elementName)){
                    throw new ArgumentException("The element name cannot be empty or white space.", nameof(elementName));
                }

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
        /// <param name="pathToProtocolXml">The path to the protocol.xml file.</param>
        /// <param name="id">The element ID.</param>
        /// <param name="agentId">The DataMiner Agent ID.</param>
        /// <param name="name">The element name.</param>
        /// <returns>The created element mock.</returns>
        public IDmsElementMock CreateElement(string pathToProtocolXml, int id = 0, int agentId = 0, string name = "Element")
        {
            var elementMock = new IDmsElementMock(cache, pathToProtocolXml, id, agentId, name);

            elementMock.Setup(element => element.Host).Returns(Object);

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

            serviceMock.Setup(service => service.Host).Returns(Object);

            cache.AddService(serviceMock);

            return serviceMock;
        }

        internal int GetNextElementId()
        {
            var elementMocks = cache.GetElements(Object.Id);
            return elementMocks.Count == 0 ? 1 : elementMocks.Max(element => element.Object.Id) + 1;
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
