using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Skyline.DataMiner.Utils.UnitTestingFramework.Protocol")]
[assembly: InternalsVisibleTo("Skyline.DataMiner.Utils.UnitTestingFramework.DataMinerSystem.Common")]
[assembly: InternalsVisibleTo("Utils.UnitTestingFramework.Tests")]
[assembly: InternalsVisibleTo("Utils.UnitTestingFramework.Protocol.Tests")]
[assembly: InternalsVisibleTo("Utils.UnitTestingFramework.DataMinerSystem.Common.Tests")]
namespace Skyline.DataMiner.Utils.UnitTestingFramework.Common
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using Skyline.DataMiner.CICD.Models.Protocol.Enums;
    using Skyline.DataMiner.CICD.Models.Protocol.Read.Interfaces;
    using Skyline.DataMiner.Utils.UnitTestingFramework.Common.Model;
    using Skyline.DataMiner.Utils.UnitTestingFramework.Common.Model.Standalone;
    using Skyline.DataMiner.Utils.UnitTestingFramework.Common.Model.Table;

    internal class ParametersAndTables
    {
        private static readonly Regex HexString = new Regex(@"^(0x[0-9a-fA-F]{2})+$");
        private static readonly string[] HexStringSeparator = new[] { "0x" };

        // Dictionaries to allow fast lookup
        private readonly Dictionary<string, ParameterDefinition> parameterNameToDefinition = new Dictionary<string, ParameterDefinition>();
        private readonly Dictionary<int, ParameterDefinition> parameterIdToDefinition = new Dictionary<int, ParameterDefinition>();
        private readonly Dictionary<ParameterDefinition, IParameterModel> parametersToValues = new Dictionary<ParameterDefinition, IParameterModel>();

        private readonly Dictionary<int, ITableModel> tablesPerTablePid = new Dictionary<int, ITableModel>();

        public ParametersAndTables()
        {
        }

        public ParametersAndTables(ParameterAndTableDefinitions definitions)
        {
            if (definitions == null)
            {
                throw new ArgumentNullException(nameof(definitions));
            }

            foreach (var parameterDefinition in definitions.GetParameterDefinitions())
            {
                AddParameter(new ParameterModel(parameterDefinition, null));
            }

            foreach (var tableDefinition in definitions.GetTableDefinitions())
            {
                AddTable(new TableModel(tableDefinition.Key, tableDefinition.Value));
            }
        }

        public bool ParameterExists(int parameterId)
        {
            return parameterIdToDefinition.ContainsKey(parameterId);
        }

        public bool ParameterExists(string parameterName)
        {
            return parameterNameToDefinition.ContainsKey(parameterName);
        }

        /// <summary>
        /// Gets the parameter model.
        /// </summary>
        /// <param name="parameterId">The parameter ID.</param>
        /// <returns>The parameter model.</returns>
        /// <exception cref="ArgumentException">There is no parameter with ID <paramref name="parameterId"/>.</exception>
        public IParameterModel GetParameter(int parameterId)
        {
            if (!parameterIdToDefinition.TryGetValue(parameterId, out var parameterDefinition))
            {
                throw new ArgumentException($"There is no parameter with ID '{parameterId}'", nameof(parameterId));
            }

            return parametersToValues[parameterDefinition];
        }

        /// <summary>
        /// Gets the parameter model.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns>The parameter model.</returns>
        /// <exception cref="ArgumentException">There is no parameter with name <paramref name="parameterName"/>.</exception>
        public IParameterModel GetParameter(string parameterName)
        {
            if (!parameterNameToDefinition.TryGetValue(parameterName, out var parameterDefinition))
            {
                throw new ArgumentException($"There is no parameter with name '{parameterName}'", nameof(parameterName));
            }

            return parametersToValues[parameterDefinition];
        }

        /// <summary>
        /// Gets the table model for the table with the specified parameter ID.
        /// </summary>
        /// <param name="tablePid">The ID of the table parameter.</param>
        /// <returns>The table model for the table with the specified ID.</returns>
        /// <exception cref="ArgumentException">There is no table with ID " + tableId</exception>
        public ITableModel GetTable(int tablePid)
        {
            if (!tablesPerTablePid.TryGetValue(tablePid, out var tableModel))
            {
                throw new ArgumentException($"There is no table with ID '{tablePid}'");
            }

            return tableModel;
        }

        public bool TryGetParameter(int parameterId, out IParameterModel parameterModel)
        {
            if (ParameterExists(parameterId))
            {
                parameterModel = GetParameter(parameterId);
                return true;
            }
            else
            {
                parameterModel = null;
                return false;
            }
        }

        public bool TryGetParameter(string parameterName, out IParameterModel parameterModel)
        {
            if (ParameterExists(parameterName))
            {
                parameterModel = GetParameter(parameterName);
                return true;
            }
            else
            {
                parameterModel = null;
                return false;
            }
        }

        internal ICollection<IParameterModel> GetParameters()
        {
            return new List<IParameterModel>(parametersToValues.Values);
        }

        internal ICollection<ITableModel> GetTables()
        {
            return new List<ITableModel>(tablesPerTablePid.Values);
        }

        internal void ApplyInitialValues(IProtocolModel protocolModel)
        {
            if (protocolModel == null)
            {
                throw new ArgumentNullException(nameof(protocolModel));
            }

            foreach (var parameter in protocolModel.Protocol.Params)
            {
                int parameterId = (int)parameter.Id.Value.Value;
                if (!TryGetParameter(parameterId, out var parameterModel) || parameter.Interprete?.Type?.Value == null)
                {
                    continue;
                }

                object initialValue = null;
                switch (parameter.Type.Value.Value)
                {
                    case EnumParamType.Read:
                        initialValue = GetReadInitialValue(parameter.Interprete.Type.Value.Value, parameter.Interprete.DefaultValue?.Value);
                        break;

                    case EnumParamType.Fixed:
                        initialValue = GetFixedInitialValue(parameter.Interprete.Type.Value.Value, parameter.Interprete.ValueElement?.Value);
                        break;
                }

                if (initialValue != null)
                {
                    parameterModel.Update(initialValue);
                }
            }
        }

        private static object GetReadInitialValue(EnumParamInterpretType interpreteType, string value)
        {
            if (interpreteType == EnumParamInterpretType.String)
            {
                return value;
            }

            if (interpreteType == EnumParamInterpretType.Double && Double.TryParse(value, out double doubleValue))
            {
                return doubleValue;
            }

            return null;
        }

        private static object GetFixedInitialValue(EnumParamInterpretType interpreteType, string value)
        {
            if (interpreteType == EnumParamInterpretType.String)
            {
                return value;
            }

            if (interpreteType != EnumParamInterpretType.Double || value == null)
            {
                return null;
            }

            if (!HexString.IsMatch(value))
            {
                return -1;
            }

            string[] parts = value.Split(HexStringSeparator, StringSplitOptions.RemoveEmptyEntries);
            return Int32.Parse(parts[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Adds the specified model.
        /// </summary>
        /// <param name="tableModel">The table model.</param>
        /// <exception cref="ArgumentNullException"><paramref name="tableModel"/> is <see langword="null"/>.</exception>
        internal void AddTable(ITableModel tableModel)
        {
            if (tableModel == null)
            {
                throw new ArgumentNullException(nameof(tableModel));
            }

            if (tablesPerTablePid.ContainsKey(tableModel.TableId))
            {
                throw new ArgumentException($"There is already a table with ID '{tableModel.TableId}'", nameof(tableModel));
            }

            tablesPerTablePid.Add(tableModel.TableId, tableModel);
        }

        internal void AddParameter(IParameterModel parameterModel)
        {
            if (parameterModel is null)
            {
                throw new ArgumentNullException(nameof(parameterModel));
            }

            if (parameterIdToDefinition.ContainsKey(parameterModel.Definition.Pid))
            {
                throw new ArgumentException($"There is already a parameter with ID '{parameterModel.Definition.Pid}'", nameof(parameterModel));
            }


            if (parameterNameToDefinition.ContainsKey(parameterModel.Definition.Name))
            {
                throw new ArgumentException($"There is already a parameter with name '{parameterModel.Definition.Name}'", nameof(parameterModel));
            }

            parameterNameToDefinition.Add(parameterModel.Definition.Name, parameterModel.Definition);
            parameterIdToDefinition.Add(parameterModel.Definition.Pid, parameterModel.Definition);
            parametersToValues.Add(parameterModel.Definition, parameterModel);
        }
    }
}
