namespace Skyline.DataMiner.Utils.UnitTestingFramework.Common
{
    using System;
    using System.Collections.Generic;
    using Skyline.DataMiner.Utils.UnitTestingFramework.Common.Model;
    using Skyline.DataMiner.Utils.UnitTestingFramework.Common.Model.Table;

    public class ParameterAndTableDefinitions
    {
        private readonly Dictionary<string, ParameterDefinition> parameterNameToDefinition = new Dictionary<string, ParameterDefinition>();
        private readonly Dictionary<int, ParameterDefinition> parameterIdToDefinition = new Dictionary<int, ParameterDefinition>();

        private readonly Dictionary<int, TableSchema> tablesPerTablePid = new Dictionary<int, TableSchema>();

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

        // TODO Add public method to add parameter definition

        // TODO add internal methods to get all parameter definitions and get table definitions. So that ParametersAndTables can use this class to build its models.
    }
}