using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyline.DataMiner.Utils.UnitTestingFramework.DataMinerSystem.Common
{
    public class DmsBuilder
    {
        private readonly List<string> protocolPaths = new List<string>();
        private readonly List<Action<IDmsMock>> views = new List<Action<IDmsMock>>();
        private readonly List<Action<IDmsMock>> dmas = new List<Action<IDmsMock>>();

        public DmsBuilder WithProtocol(string pathToProtocolXml)
        {
            protocolPaths.Add(pathToProtocolXml);
            return this;
        }

        public DmsBuilder WithView(int viewId, string name = null)
        {
            views.Add(dmsMock => dmsMock.CreateView(viewId, name ?? $"View {viewId}"));
            return this;
        }

        public DmsBuilder WithDma(int id, Action<DmaBuilder> configure = null, string name = null)
        {
            dmas.Add(dmsMock =>
            {
                var dmaBuilder = new DmaBuilder(id, name);
                configure?.Invoke(dmaBuilder);
                dmaBuilder.Build(dmsMock);
            });
            return this;
        }

        public IDmsMock Build()
        {
            var dmsMock = new IDmsMock();

            foreach (var path in protocolPaths)
            {
                dmsMock.AddProtocol(path);
            }

            foreach (var view in views)
            {
                view(dmsMock);
            }

            foreach (var dma in dmas)
            {
                dma(dmsMock);
            }

            return dmsMock;
        }
    }
}
