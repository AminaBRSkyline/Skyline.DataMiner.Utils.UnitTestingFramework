namespace Skyline.DataMiner.Utils.UnitTestingFramework.Common
{
    using System;
    using System.Collections.Generic;
    using Skyline.DataMiner.CICD.Models.Protocol.Read.Interfaces;

    using Skyline.DataMiner.Utils.UnitTestingFramework.Common.Model.Creation;

    internal static class ParameterAndTablesDefinitionsBuilder
    {
        public static ParameterAndTableDefinitions Build(string customPathToProtocolXml)
        {
            var protocolModel = ProtocolModelBuilder.Build(customPathToProtocolXml);

            return Build(protocolModel);
        }

        /// <summary>
        /// Creates parameter and table definitions from a protocol model.
        /// </summary>
        public static ParameterAndTableDefinitions Build(IProtocolModel protocolModel)
        {
            if (protocolModel is null)
            {
                throw new ArgumentNullException(nameof(protocolModel));
            }

            var definitions = new ParameterAndTableDefinitions();
            var excludedPids = new HashSet<int>();

            var protocolModelParameterFinder = new ProtocolModelParameterFinder(protocolModel);

            foreach (var parameter in protocolModel.Protocol.Params)
            {
                try
                {
                    var parameterType = parameter.Type.Value.Value;

                    var modelCreator = DataModelCreatorFactory.Create(parameterType, excludedPids);
                    modelCreator.CreateDefinitionAndAddToCollection(definitions, parameter, protocolModelParameterFinder);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"An exception occurred while processing protocol parameter '{parameter.Name.Value}' (ID: {(int)parameter.Id.Value.Value}). See inner exception for more details.", ex);
                }
            }

            return definitions;
        }
    }
}
