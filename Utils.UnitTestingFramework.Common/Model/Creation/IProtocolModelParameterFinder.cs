namespace Skyline.DataMiner.Utils.UnitTestingFramework.Common.Model.Creation
{
    using Skyline.DataMiner.CICD.Models.Protocol.Read;

    public interface IProtocolModelParameterFinder
    {
        IParamsParam FindParameter(int parameterId);
    }
}