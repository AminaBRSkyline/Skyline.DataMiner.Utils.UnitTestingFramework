namespace Skyline.DataMiner.Utils.UnitTestingFramework.DataMinerSystem.Common
{
    using System;
    using System.Collections.Generic;

    using Moq;

    using Skyline.DataMiner.Core.DataMinerSystem.Common;

    /// <summary>
    /// A pre-arranged mock of <see cref="IDmsService"/>.
    /// </summary>
    public class IDmsServiceMock : Mock<IDmsService>
    {
        private readonly Cache cache;
        private readonly int agentId;
        private readonly int id;
        private string name;

        public string Description { get; set; } = String.Empty;

        public ISet<IDmsView> Views { get; set; } = new HashSet<IDmsView>();

        internal IDmsServiceMock(Cache cache, int id, int agentId, string name)
        {
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
            this.id = id;
            this.agentId = agentId;
            this.name = name;

            Setup(service => service.Id).Returns(id);
            Setup(service => service.AgentId).Returns(agentId);
            Setup(service => service.DmsServiceId).Returns(new DmsServiceId(agentId, id));
            Setup(service => service.Description).Returns(() => Description);
            SetupSet(service => service.Description = It.IsAny<string>()).Callback((string value) => Description = value);
            Setup(service => service.Name).Returns(() => this.name);
            SetupSet(service => service.Name = It.IsAny<string>()).Callback((string value) => this.name = value);
            Setup(service => service.Views).Returns(() => Views);
            Setup(service => service.Delete()).Callback(() => this.cache.RemoveService(agentId, id));
        }
    }
}
