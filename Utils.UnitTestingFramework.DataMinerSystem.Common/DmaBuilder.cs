using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyline.DataMiner.Utils.UnitTestingFramework.DataMinerSystem.Common
{
    public class DmaBuilder
    {
        private readonly int id;
        private readonly string name;
        private readonly List<Action<IDmsMock, IDmaMock>> elements = new List<Action<IDmsMock, IDmaMock>>();

        internal DmaBuilder(int id, string name)
        {
            this.id = id;
            this.name = name;
        }

        public DmaBuilder WithElement(int id, string name, string protocolName, string protocolVersion = null, Action<DmsElementBuilder> configure = null)
        {
            elements.Add((dmsMock, dmaMock) =>
            {
                var elementBuilder = new DmsElementBuilder(id, name, protocolName, protocolVersion);
                configure?.Invoke(elementBuilder);
                elementBuilder.Build(dmsMock, dmaMock);
            });
            return this;
        }

        internal void Build(IDmsMock dmsMock)
        {
            var dmaMock = dmsMock.CreateAgent(id, name ?? $"Agent {id}");

            foreach (var element in elements)
            {
                element(dmsMock, dmaMock);
            }
        }
    }
}
