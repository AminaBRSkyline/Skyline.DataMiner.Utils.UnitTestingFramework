using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyline.DataMiner.Utils.UnitTestingFramework.DataMinerSystem.Common
{
    public class DmsElementBuilder
    {
        private readonly int id;
        private readonly string name;
        private readonly string protocolName;
        private readonly string protocolVersion;
        private readonly List<Action<IDmsElementMock>> actions = new List<Action<IDmsElementMock>>();

        internal DmsElementBuilder(int id, string name, string protocolName, string protocolVersion)
        {
            this.id = id;
            this.name = name;
            this.protocolName = protocolName;
            this.protocolVersion = protocolVersion;
        }

        public DmsElementBuilder UnderView(int viewId)
        {
            actions.Add(elementMock => elementMock.AddView(viewId));
            return this;
        }

        public DmsElementBuilder WithTable(int tableId, object[][] rows)
        {
            actions.Add(elementMock =>
            {
                if (rows == null)
                {
                    throw new ArgumentNullException(nameof(rows));
                }

                var table = elementMock.Object.GetTable(tableId);

                foreach (var row in rows)
                {
                    table.AddRow(row);
                }
            });

            return this;
        }

        internal void Build(IDmsMock dmsMock, IDmaMock dmaMock)
        {
            var protocolMock = dmsMock.GetProtocolMock(protocolName, protocolVersion);

            if (protocolMock == null)
            {
                var version = protocolVersion == null ? String.Empty : $" with version '{protocolVersion}'";
                throw new InvalidOperationException($"Protocol '{protocolName}'{version} is not available in this DataMiner System.");
            }

            var elementMock = dmaMock.CreateElement(protocolMock, id, name ?? $"Element {id}");

            foreach (var action in actions)
            {
                action(elementMock);
            }
        }
    }
}
