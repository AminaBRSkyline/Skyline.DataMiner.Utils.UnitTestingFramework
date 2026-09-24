namespace Skyline.DataMiner.Utils.UnitTestingFramework.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.Utils.UnitTestingFramework.Common.Model;
    using Skyline.DataMiner.Utils.UnitTestingFramework.Common.Model.Table;

    internal class ParameterAndTableDefinitions
    {
        private readonly Dictionary<string, ParameterDefinition> parameterNameToDefinition = new Dictionary<string, ParameterDefinition>();
        private readonly Dictionary<int, ParameterDefinition> parameterIdToDefinition = new Dictionary<int, ParameterDefinition>();
        private readonly Dictionary<int, TableDefinition> tablesPerTablePid = new Dictionary<int, TableDefinition>();

        public ParameterAndTableDefinitions()
        {
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

        public void AddTableDefinition(int tableId, TableDefinition tableDefinition)
        {
            if (tableDefinition == null)
            {
                throw new ArgumentNullException(nameof(tableDefinition));
            }

            if (tablesPerTablePid.ContainsKey(tableId))
            {
                throw new ArgumentException($"There is already a table with ID '{tableId}'", nameof(tableId));
            }

            tablesPerTablePid.Add(tableId, tableDefinition);
        }

        public TableDefinition GetTableDefinition(int tableId)
        {
            if (!tablesPerTablePid.TryGetValue(tableId, out var tableDefinition))
            {
                throw new ArgumentException($"There is no table with ID '{tableId}'", nameof(tableId));
            }

            return tableDefinition;
        }

        internal ICollection<ParameterDefinition> GetParameterDefinitions()
        {
            return parameterIdToDefinition.Values.ToList();
        }

        internal ICollection<KeyValuePair<int, TableDefinition>> GetTableDefinitions()
        {
            return tablesPerTablePid.ToList();
        }

    }
}
