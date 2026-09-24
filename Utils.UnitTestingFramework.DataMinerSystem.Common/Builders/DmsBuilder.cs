using System;
using System.Collections.Generic;
using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
using Skyline.DataMiner.Net.Sections;

namespace Skyline.DataMiner.Utils.UnitTestingFramework.DataMinerSystem.Common
{
    public class DmsBuilder
    {
        private readonly List<string> protocolPaths = new List<string>();
        private readonly List<Action<IDmsMock>> protocols = new List<Action<IDmsMock>>();
        private readonly List<Action<IDmsMock>> views = new List<Action<IDmsMock>>();
        private readonly List<Action<IDmsMock>> dmas = new List<Action<IDmsMock>>();
        private readonly Dictionary<string, List<Func<DomInstance>>> domInstances = new Dictionary<string, List<Func<DomInstance>>>(StringComparer.Ordinal);
        private readonly Dictionary<string, List<Func<DomDefinition>>> domDefinitions = new Dictionary<string, List<Func<DomDefinition>>>(StringComparer.Ordinal);
        private readonly Dictionary<string, List<Func<SectionDefinition>>> sectionDefinitions = new Dictionary<string, List<Func<SectionDefinition>>>(StringComparer.Ordinal);
        private readonly Dictionary<string, List<Func<DomBehaviorDefinition>>> behaviorDefinitions = new Dictionary<string, List<Func<DomBehaviorDefinition>>>(StringComparer.Ordinal);

        public DmsBuilder WithProtocol(string pathToProtocolXml)
        {
            protocolPaths.Add(pathToProtocolXml);
            return this;
        }

        public DmsBuilder WithProtocol(string name, Action<IDmsProtocolMockBuilder> configure, string version = IDmsProtocolMock.DefaultVersion)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            protocols.Add(dmsMock =>
            {
                var protocolBuilder = new IDmsProtocolMockBuilder(name, version);
                configure(protocolBuilder);
                protocolBuilder.Build(dmsMock);
            });

            return this;
        }

        public DmsBuilder WithView(int viewId, string name = null)
        {
            views.Add(dmsMock => dmsMock.CreateView(viewId, name ?? $"View {viewId}"));
            return this;
        }

        public DmsBuilder WithDomInstance(string moduleId, Func<DomInstance> createInstance)
        {
            AddDomObject(domInstances, moduleId, createInstance, nameof(createInstance));
            return this;
        }

        public DmsBuilder WithDomDefinition(string moduleId, Func<DomDefinition> createDefinition)
        {
            AddDomObject(domDefinitions, moduleId, createDefinition, nameof(createDefinition));
            return this;
        }

        public DmsBuilder WithSectionDefinition(string moduleId, Func<SectionDefinition> createDefinition)
        {
            AddDomObject(sectionDefinitions, moduleId, createDefinition, nameof(createDefinition));
            return this;
        }

        public DmsBuilder WithDomBehaviorDefinition(string moduleId, Func<DomBehaviorDefinition> createDefinition)
        {
            AddDomObject(behaviorDefinitions, moduleId, createDefinition, nameof(createDefinition));
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

            foreach (var protocol in protocols)
            {
                protocol(dmsMock);
            }

            foreach (var view in views)
            {
                view(dmsMock);
            }

            foreach (var dma in dmas)
            {
                dma(dmsMock);
            }

            foreach (var module in domInstances)
            {
                dmsMock.Dom.SetInstances(module.Key, CreateDomObjects(module.Key, module.Value));
            }

            foreach (var module in domDefinitions)
            {
                dmsMock.Dom.SetDefinitions(module.Key, CreateDomObjects(module.Key, module.Value));
            }

            foreach (var module in sectionDefinitions)
            {
                dmsMock.Dom.SetSectionDefinitions(module.Key, CreateDomObjects(module.Key, module.Value));
            }

            foreach (var module in behaviorDefinitions)
            {
                dmsMock.Dom.SetBehaviorDefinitions(module.Key, CreateDomObjects(module.Key, module.Value));
            }

            return dmsMock;
        }

        private static void AddDomObject<T>(Dictionary<string, List<Func<T>>> objectsByModule, string moduleId, Func<T> createObject, string parameterName)
        {
            if (String.IsNullOrWhiteSpace(moduleId))
            {
                throw new ArgumentException("The DOM module ID cannot be empty or white space.", nameof(moduleId));
            }

            if (createObject == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            if (!objectsByModule.TryGetValue(moduleId, out var objects))
            {
                objects = new List<Func<T>>();
                objectsByModule.Add(moduleId, objects);
            }

            objects.Add(createObject);
        }

        private static IEnumerable<T> CreateDomObjects<T>(string moduleId, IEnumerable<Func<T>> factories)
            where T : class
        {
            foreach (var factory in factories)
            {
                var value = factory();
                if (value == null)
                {
                    throw new InvalidOperationException($"A DOM object factory for module '{moduleId}' returned null.");
                }

                yield return value;
            }
        }
    }
}
