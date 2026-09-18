namespace Skyline.DataMiner.Utils.UnitTestingFramework.DataMinerSystem.Common
{
    using System;

    using Moq;

    using Skyline.DataMiner.CICD.Models.Protocol.Read.Interfaces;
    using Skyline.DataMiner.Core.DataMinerSystem.Common;
    using Skyline.DataMiner.Utils.UnitTestingFramework.Common;
    using Skyline.DataMiner.Utils.UnitTestingFramework.Common.Model.Table;

    /// <summary>
    /// A pre-arranged mock of <see cref="IDmsProtocol"/>.
    /// </summary>
    public class IDmsProtocolMock : Mock<IDmsProtocol>
    {
        internal IDmsProtocolMock(string name , string version = "1.0.0.1")
        {
            Name = name;
            ReferencedVersion = version;
            Definitions = new ParameterAndTableDefinitions();
        }

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

            // TODO convert protocol model to ParametersAndTableDefinitions

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

        internal ParameterAndTableDefinitions Definitions { get; }

        internal IProtocolModel ProtocolModel { get; }

        internal string PathToProtocolXml { get; }

        public string Name { get; set; }

        public string ReferencedVersion { get; set; }

        public ProtocolType Type { get; set; }


        public void AddTable(int tableId, TableSchema tableSchema)
        {
            Definitions.AddTableDefinition(tableId, tableSchema);
        }

        // TODO add method to add parameter definitions
    }
}
