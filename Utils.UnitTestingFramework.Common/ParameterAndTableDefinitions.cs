namespace Skyline.DataMiner.Utils.UnitTestingFramework.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.CICD.Models.Protocol.Read.Interfaces;
    using Skyline.DataMiner.Utils.UnitTestingFramework.Common.Model;
    using Skyline.DataMiner.Utils.UnitTestingFramework.Common.Model.Table;

    public class ParameterAndTableDefinitions
    {
        private readonly Dictionary<string, ParameterDefinition> parameterNameToDefinition = new Dictionary<string, ParameterDefinition>();
        private readonly Dictionary<int, ParameterDefinition> parameterIdToDefinition = new Dictionary<int, ParameterDefinition>();
        private readonly Dictionary<int, object> initialParameterValues = new Dictionary<int, object>();

        private readonly Dictionary<int, TableSchema> tablesPerTablePid = new Dictionary<int, TableSchema>();

        public ParameterAndTableDefinitions()
        {
        }

        public ParameterAndTableDefinitions(IProtocolModel protocolModel)
        {
            Populate(protocolModel);
        }

        public void AddParameterDefinition(ParameterDefinition parameterDefinition)
        {
            if (parameterDefinition == null)
            {
                throw new ArgumentNullException(nameof(parameterDefinition));
            }

            if (parameterIdToDefinition.ContainsKey(parameterDefinition.Pid))
            {
                throw new ArgumentException($"There is already a parameter with ID '{parameterDefinition.Pid}'", nameof(parameterDefinition));
            }

            if (parameterNameToDefinition.ContainsKey(parameterDefinition.Name))
            {
                throw new ArgumentException($"There is already a parameter with name '{parameterDefinition.Name}'", nameof(parameterDefinition));
            }

            parameterIdToDefinition.Add(parameterDefinition.Pid, parameterDefinition);
            parameterNameToDefinition.Add(parameterDefinition.Name, parameterDefinition);
        }

        public ParameterDefinition GetParameterDefinition(int parameterId)
        {
            if (!parameterIdToDefinition.TryGetValue(parameterId, out var definition))
            {
                throw new ArgumentException($"There is no parameter with ID '{parameterId}'", nameof(parameterId));
            }

            return definition;
        }

        public ParameterDefinition GetParameterDefinition(string parameterName)
        {
            if (!parameterNameToDefinition.TryGetValue(parameterName, out var definition))
            {
                throw new ArgumentException($"There is no parameter with name '{parameterName}'", nameof(parameterName));
            }

            return definition;
        }

        public void AddTableDefinition(int tableId, TableSchema tableSchema)
        {
            if (tableSchema == null)
            {
                throw new ArgumentNullException(nameof(tableSchema));
            }

            if (tablesPerTablePid.ContainsKey(tableId))
            {
                throw new ArgumentException($"There is already a table with ID '{tableId}'", nameof(tableId));
            }

            tablesPerTablePid.Add(tableId, tableSchema);
        }

        public TableSchema GetTableDefinition(int tableId)
        {
            if (!tablesPerTablePid.TryGetValue(tableId, out var tableSchema))
            {
                throw new ArgumentException($"There is no table with ID '{tableId}'", nameof(tableId));
            }

            return tableSchema;
        }

        public void Populate(IProtocolModel protocolModel)
        {
            if (protocolModel == null)
            {
                throw new ArgumentNullException(nameof(protocolModel));
            }

            var parametersAndTables = ParametersAndTablesBuilder.Build(protocolModel);

            parameterNameToDefinition.Clear();
            parameterIdToDefinition.Clear();
            initialParameterValues.Clear();
            tablesPerTablePid.Clear();

            foreach (var parameter in parametersAndTables.GetParameters())
            {
                AddParameterDefinition(parameter.Definition);
                initialParameterValues[parameter.Definition.Pid] = parameter.Value;
            }

            foreach (var table in parametersAndTables.GetTables())
            {
                AddTableDefinition(table.TableId, table.Schema);
            }
        }

        internal ICollection<ParameterDefinition> GetParameterDefinitions()
        {
            return parameterIdToDefinition.Values.ToList();
        }

        internal ICollection<KeyValuePair<int, TableSchema>> GetTableDefinitions()
        {
            return tablesPerTablePid.ToList();
        }

        internal object GetInitialParameterValue(int parameterId)
        {
            initialParameterValues.TryGetValue(parameterId, out var value);
            return value;
        }
    }
}
