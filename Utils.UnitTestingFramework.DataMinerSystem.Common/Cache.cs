using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyline.DataMiner.Utils.UnitTestingFramework.DataMinerSystem.Common
{
    using System.Collections.Generic;

    internal sealed class Cache
    {
        private readonly List<IDmaMock> dmaMocks = new List<IDmaMock>();
        private readonly List<IDmsElementMock> elementMocks = new List<IDmsElementMock>();
        private readonly List<IDmsServiceMock> serviceMocks = new List<IDmsServiceMock>();
        private readonly List<IDmsViewMock> viewMocks = new List<IDmsViewMock>();

        internal void AddDma(IDmaMock dmaMock)
        {
            dmaMocks.Add(dmaMock);
        }
        internal void AddView(IDmsViewMock viewMock)
        {
            viewMocks.Add(viewMock);
        }
        internal IDmsViewMock GetView(int viewId)
        {
            return viewMocks.FirstOrDefault(view => view.Object.Id == viewId);
        }
        internal IDmsViewMock GetView(string name)
        {
            return viewMocks.FirstOrDefault(view => String.Equals(view.Object.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        internal ICollection<IDmsViewMock> GetViews()
        {
            return viewMocks.ToList();
        }

        internal bool RemoveView(int viewId)
        {
            var viewMock = GetView(viewId);

            if (viewMock == null)
            {
                return false;
            }

            foreach (var elementMock in elementMocks)
            {
                elementMock.Views.Remove(viewMock.Object);
            }

            foreach (var serviceMock in serviceMocks)
            {
                serviceMock.Views.Remove(viewMock.Object);
            }

            return viewMocks.Remove(viewMock);
        }
        internal IDmaMock GetDma(int agentId)
        {
            return dmaMocks.FirstOrDefault(dma => dma.Object.Id == agentId);
        }
        internal IDmaMock GetDma(string name)
        {
            return dmaMocks.FirstOrDefault(dma => dma.Object.Name == name);
        }
        internal bool RemoveDma(int agentId)
        {
            var dmaMock = GetDma(agentId);

            if (dmaMock == null)
            {
                return false;
            }

            elementMocks.RemoveAll(element => element.Object.AgentId == agentId);
            serviceMocks.RemoveAll(service => service.Object.AgentId == agentId);

            return dmaMocks.Remove(dmaMock);
        }
        internal void AddElement(IDmsElementMock elementMock)
        {
            elementMocks.Add(elementMock);
        }
        internal IDmsElementMock GetElement(int agentId, int elementId)
        {
            return elementMocks.FirstOrDefault(element => element.Object.AgentId == agentId && element.Object.Id == elementId);
        }
        internal IDmsElementMock GetElement(int agentId, string name)
        {
            return elementMocks.FirstOrDefault(element => element.Object.AgentId == agentId && element.Object.Name == name);
        }
        internal bool RemoveElement(int agentId, int elementId)
        {
            var elementMock = GetElement(agentId, elementId);

            return elementMock != null && elementMocks.Remove(elementMock);
        }
        internal ICollection<IDmsElementMock> GetElements(int agentId)
        {
            return elementMocks.Where(element => element.Object.AgentId == agentId).ToList();
        }
        internal IDmsElementMock GetElement(string name)
        {
            return elementMocks.FirstOrDefault(element => element.Object.Name == name);
        }
        internal ICollection<IDmaMock> GetDmas()
        {
            return dmaMocks.ToList();
        }
        internal ICollection<IDmsElementMock> GetElements()
        {
            return elementMocks.ToList();
        }

        internal void AddService(IDmsServiceMock serviceMock)
        {
            serviceMocks.Add(serviceMock);
        }

        internal IDmsServiceMock GetService(int agentId, int serviceId)
        {
            return serviceMocks.FirstOrDefault(service => service.Object.AgentId == agentId && service.Object.Id == serviceId);
        }

        internal IDmsServiceMock GetService(int agentId, string name)
        {
            return serviceMocks.FirstOrDefault(service => service.Object.AgentId == agentId && String.Equals(service.Object.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        internal IDmsServiceMock GetService(string name)
        {
            return serviceMocks.FirstOrDefault(service => String.Equals(service.Object.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        internal ICollection<IDmsServiceMock> GetServices(int agentId)
        {
            return serviceMocks.Where(service => service.Object.AgentId == agentId).ToList();
        }

        internal ICollection<IDmsServiceMock> GetServices()
        {
            return serviceMocks.ToList();
        }

        internal bool RemoveService(int agentId, int serviceId)
        {
            var serviceMock = GetService(agentId, serviceId);

            return serviceMock != null && serviceMocks.Remove(serviceMock);
        }
    }
}
