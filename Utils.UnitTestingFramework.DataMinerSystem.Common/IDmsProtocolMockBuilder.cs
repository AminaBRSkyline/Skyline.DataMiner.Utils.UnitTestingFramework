namespace Skyline.DataMiner.Utils.UnitTestingFramework.DataMinerSystem.Common
{
    using System;
    using System.Collections.Generic;

    using Skyline.DataMiner.Utils.UnitTestingFramework.Common.Model;
    using Skyline.DataMiner.Utils.UnitTestingFramework.Common.Model.Table;

    public sealed class IDmsProtocolMockBuilder
    {
        private readonly string name;
        private readonly string version;
        private readonly List<ParameterDefinition> parameterDefinitions = new List<ParameterDefinition>();
        private readonly List<KeyValuePair<int, TableSchema>> tableDefinitions = new List<KeyValuePair<int, TableSchema>>();

        public IDmsProtocolMockBuilder(string name, string version = IDmsProtocolMock.DefaultVersion)
        {
            this.name = name;
            this.version = version;
        }

        public IDmsProtocolMockBuilder AddParameterDefinition(ParameterDefinition parameterDefinition)
        {
            if (parameterDefinition == null)
            {
                throw new ArgumentNullException(nameof(parameterDefinition));
            }

            parameterDefinitions.Add(parameterDefinition);
            return this;
        }

        public IDmsProtocolMockBuilder AddTableDefinition(int tableId, TableSchema tableSchema)
        {
            if (tableSchema == null)
            {
                throw new ArgumentNullException(nameof(tableSchema));
            }

            tableDefinitions.Add(new KeyValuePair<int, TableSchema>(tableId, tableSchema));
            return this;
        }

        public IDmsProtocolMock Build()
        {
            var protocolMock = new IDmsProtocolMock(name, version);

            foreach (var parameterDefinition in parameterDefinitions)
            {
                protocolMock.AddParameterDefinition(parameterDefinition);
            }

            foreach (var tableDefinition in tableDefinitions)
            {
                protocolMock.AddTableDefinition(tableDefinition.Key, tableDefinition.Value);
            }

            return protocolMock;
        }

        internal IDmsProtocolMock Build(IDmsMock dmsMock)
        {
            if (dmsMock == null)
            {
                throw new ArgumentNullException(nameof(dmsMock));
            }

            var protocolMock = Build();
            dmsMock.AddProtocol(protocolMock);
            return protocolMock;
        }
    }
}
