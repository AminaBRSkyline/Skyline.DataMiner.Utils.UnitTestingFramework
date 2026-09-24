namespace Skyline.DataMiner.Utils.UnitTestingFramework.DataMinerSystem.Common
{
    using System;
    using System.Collections.Generic;
    using Skyline.DataMiner.Utils.UnitTestingFramework.Common.Model.Table;

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

        public DmsElementBuilder FillTable(int tableId, object[][] rows)
        {
            if (rows == null)
            {
                throw new ArgumentNullException(nameof(rows));
            }

            if (rows.Length == 0)
                return this;

            actions.Add(elementMock =>
            {
                var table = elementMock.GetDmsTableMock(tableId);
                foreach (var row in rows)
                {
                    table.TableModel.SetRow(row);
                }
            });

            return this;
        }

        public DmsElementBuilder FillTable(int tableId, params Action<RowBuilder>[] rowBuilderActions)
        {
            if (rowBuilderActions is null)
            {
                throw new ArgumentNullException(nameof(rowBuilderActions));
            }

            if(rowBuilderActions.Length == 0)
                return this;

            actions.Add(elementMock =>
            {
                var table = elementMock.GetDmsTableMock(tableId);
                foreach (var rowBuilderAction in rowBuilderActions)
                {
                    var rowBuilder = new RowBuilder(table.TableModel.Definition);
                    rowBuilderAction(rowBuilder);
                    table.SetRow(rowBuilder.Build());
                }
            });

            return this;
        }

        public DmsElementBuilder SetParameter<T>(int parameterId, T value)
        {
            actions.Add(elementMock => elementMock.GetStandaloneParameterMock<T>(parameterId).ParameterModel.Update(value));
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
