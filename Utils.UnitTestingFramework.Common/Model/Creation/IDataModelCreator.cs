namespace Skyline.DataMiner.Utils.UnitTestingFramework.Common.Model.Creation
{
    using Skyline.DataMiner.CICD.Models.Protocol.Read;

    internal interface IDataModelCreator
    {
        void CreateDefinitionAndAddToCollection(ParameterAndTableDefinitions definitions, IParamsParam parameter, IProtocolModelParameterFinder protocolModelParameterFinder);
    }
}
