namespace Skyline.DataMiner.Utils.UnitTestingFramework.DataMinerSystem.Common
{
    using System;
    using System.Collections.Generic;

    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.Net.Sections;

    internal sealed class DomCacheMock
    {
        internal Dictionary<Guid, DomDefinition> Definitions { get; } = new Dictionary<Guid, DomDefinition>();

        internal Dictionary<Guid, SectionDefinition> SectionDefinitions { get; } = new Dictionary<Guid, SectionDefinition>();

        internal Dictionary<Guid, DomInstance> Instances { get; } = new Dictionary<Guid, DomInstance>();

        internal Dictionary<Guid, DomBehaviorDefinition> BehaviorDefinitions { get; } = new Dictionary<Guid, DomBehaviorDefinition>();

        internal void SetDefinitions(IEnumerable<DomDefinition> definitions)
        {
            Definitions.Clear();

            foreach (var definition in definitions)
            {
                Definitions[definition.ID.Id] = definition;
            }
        }

        internal void SetSectionDefinitions(IEnumerable<SectionDefinition> definitions)
        {
            SectionDefinitions.Clear();

            foreach (var definition in definitions)
            {
                SectionDefinitions[definition.GetID().Id] = definition;
            }
        }

        internal void SetInstances(IEnumerable<DomInstance> instances)
        {
            Instances.Clear();

            foreach (var instance in instances)
            {
                Instances[instance.ID.Id] = instance;
            }
        }

        internal void SetBehaviorDefinitions(IEnumerable<DomBehaviorDefinition> definitions)
        {
            BehaviorDefinitions.Clear();

            foreach (var definition in definitions)
            {
                BehaviorDefinitions[definition.ID.Id] = definition;
            }
        }
    }
}
