namespace Skyline.DataMiner.Utils.UnitTestingFramework.Common.Model.Creation
{
    using System.Collections.Generic;

    using Skyline.DataMiner.CICD.Models.Protocol.Read;

    internal class WriteParameterModelCreator : ParameterModelCreatorBase
    {
        private readonly HashSet<int> excludedPids;

        public WriteParameterModelCreator(HashSet<int> excludedPids)
        {
            this.excludedPids = excludedPids ?? throw new System.ArgumentNullException(nameof(excludedPids));
        }

        protected override void ProcessString(ParameterAndTableDefinitions definitions, IParamsParam parameter)
        {
            ProcessAny(definitions, parameter);
        }

        protected override void ProcessDouble(ParameterAndTableDefinitions definitions, IParamsParam parameter)
        {
            ProcessAny(definitions, parameter);
        }

        private void ProcessAny(ParameterAndTableDefinitions definitions, IParamsParam parameter)
        {
            int parameterId = (int)parameter.Id.Value.Value;

            if (excludedPids.Contains(parameterId))
            {
                return;
            }

            var parameterDefinition = BuildDefinitionFromProtocolParameter(parameter);

            definitions.AddParameterDefinition(parameterDefinition);
        }
    }
}
