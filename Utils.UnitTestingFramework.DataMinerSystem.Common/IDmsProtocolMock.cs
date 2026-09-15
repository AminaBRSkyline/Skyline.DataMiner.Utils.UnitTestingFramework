using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyline.DataMiner.Utils.UnitTestingFramework.DataMinerSystem.Common
{
    using System;

    using Moq;

    using Skyline.DataMiner.CICD.Models.Protocol.Read.Interfaces;
    using Skyline.DataMiner.Core.DataMinerSystem.Common;

    /// <summary>
    /// A pre-arranged mock of <see cref="IDmsProtocol"/>.
    /// </summary>
    public class IDmsProtocolMock : Mock<IDmsProtocol>
    {
        internal IProtocolModel ProtocolModel { get; }

        internal string PathToProtocolXml { get; }

        public string Name { get; set; }

        public string ReferencedVersion { get; set; }

        public ProtocolType Type { get; set; }

        internal IDmsProtocolMock(IProtocolModel protocolModel, string pathToProtocolXml = null)
        {
            if (protocolModel == null)
            {
                throw new ArgumentNullException(nameof(protocolModel));
            }

            ProtocolModel = protocolModel;
            PathToProtocolXml = pathToProtocolXml;
            Name = protocolModel.Protocol.Name?.Value;
            ReferencedVersion = protocolModel.Protocol.Version?.Value;

            var typeName = protocolModel.Protocol.Type?.Value?.ToString();
            if (!Enum.TryParse(typeName, true, out ProtocolType protocolType))
            {
                throw new ArgumentException("The protocol type is missing or invalid.", nameof(protocolModel));
            }

            Type = protocolType;

            Setup(protocol => protocol.Name).Returns(() => Name);
            Setup(protocol => protocol.ReferencedVersion).Returns(() => ReferencedVersion);
            Setup(protocol => protocol.Type).Returns(() => Type);
        }
    }
}
