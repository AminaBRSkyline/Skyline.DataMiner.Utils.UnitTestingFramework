namespace Skyline.DataMiner.Utils.UnitTestingFramework.Common.Model.Creation
{
    using System;
    using System.Collections.Generic;
    using Skyline.DataMiner.CICD.Models.Protocol.Enums;
    using Skyline.DataMiner.CICD.Models.Protocol.Read;

    internal class FixedParameterModelCreator : ParameterModelCreatorBase
    {
        private readonly HashSet<int> excludedPids;

        public FixedParameterModelCreator(HashSet<int> excludedPids)
        {
            this.excludedPids = excludedPids ?? throw new ArgumentNullException(nameof(excludedPids));
        }

        protected override void ProcessString(ParameterAndTableDefinitions definitions, IParamsParam parameter)
        {
            int parameterId = (int)parameter.Id.Value.Value;

            if (excludedPids.Contains(parameterId))
            {
                return;
            }

            if (IsTitleParameter(parameter))
            {
                // Skip title parameters
                return;
            }

            var parameterDefinition = BuildDefinitionFromProtocolParameter(parameter);

            definitions.AddParameterDefinition(parameterDefinition);
        }

        protected override void ProcessDouble(ParameterAndTableDefinitions definitions, IParamsParam parameter)
        {
            int parameterId = (int)parameter.Id.Value.Value;

            if (excludedPids.Contains(parameterId))
            {
                return;
            }

            if (IsTitleParameter(parameter))
            {
                // Skip title parameters
                return;
            }

            var parameterDefinition = BuildDefinitionFromProtocolParameter(parameter);

            definitions.AddParameterDefinition(parameterDefinition);
        }

        private bool IsTitleParameter(IParamsParam parameter)
        {
            return parameter.Measurement?.Type?.Value == EnumParamMeasurementType.Title;
        }
    }
}
