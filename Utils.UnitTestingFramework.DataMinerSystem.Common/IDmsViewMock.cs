using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyline.DataMiner.Utils.UnitTestingFramework.DataMinerSystem.Common
{
    using Moq;

    using Skyline.DataMiner.Core.DataMinerSystem.Common;
    using Skyline.DataMiner.Core.DataMinerSystem.Common.Properties;

    /// <summary>
    /// A pre-arranged mock of <see cref="IDmsView"/>.
    /// </summary>
    public class IDmsViewMock : Mock<IDmsView>
    {
        private readonly Cache cache;
        private readonly int id;
        private AlarmLevel alarmLevel = AlarmLevel.Undefined;
        private string name;
        private IDmsView parent;

        /// <summary>
        /// Gets or sets the display string returned by the mock.
        /// </summary>
        public string Display { get; set; } = String.Empty;

        /// <summary>
        /// Gets or sets the properties returned by the mock.
        /// </summary>
        public IPropertyCollection<IDmsViewProperty, IDmsViewPropertyDefinition> Properties { get; set; } = CreateEmptyPropertyCollection();

        /// <summary>
        /// Gets or sets the alarm level returned by the mock.
        /// </summary>
        public AlarmLevel AlarmLevel
        {
            get
            {
                return alarmLevel;
            }

            set
            {
                if (!Enum.IsDefined(typeof(AlarmLevel), value))
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                alarmLevel = value;
            }
        }

        /// <summary>
        /// Gets or sets the parent view returned by the mock.
        /// </summary>
        public IDmsView Parent
        {
            get
            {
                return parent;
            }

            set
            {
                ValidateParent(value);
                parent = value;
            }
        }

        internal IDmsViewMock(Cache cache, int id, string name)
        {
            this.cache = cache;
            this.id = id;

            ValidateName(name);
            this.name = name;

            Setup(view => view.Id).Returns(id);
            Setup(view => view.Display).Returns(() => Display);
            Setup(view => view.Elements).Returns(() => this.cache.GetElements().Where(elementMock => elementMock.Views.Contains(Object)).Select(elementMock => elementMock.Object).ToList().AsReadOnly());
            Setup(view => view.Services).Returns(() => this.cache.GetServices().Where(serviceMock => serviceMock.Views.Contains(Object)).Select(serviceMock => serviceMock.Object).ToList().AsReadOnly());
            Setup(view => view.Properties).Returns(() => Properties);
            Setup(view => view.Parent).Returns(() => Parent);
            SetupSet(view => view.Parent = It.IsAny<IDmsView>()).Callback((IDmsView value) => Parent = value);
            Setup(view => view.ChildViews).Returns(() => this.cache.GetViews().Where(viewMock => ReferenceEquals(viewMock.Parent, Object)).Select(viewMock => viewMock.Object).ToList().AsReadOnly());
            Setup(view => view.Name).Returns(() => this.name);
            SetupSet(view => view.Name = It.IsAny<string>()).Callback((string value) =>
            {
                ValidateName(value);
                this.name = value;
            });
            Setup(view => view.Delete()).Callback(() => this.cache.RemoveView(id));
            Setup(view => view.GetAlarmLevel()).Returns(() => AlarmLevel);
        }

        private void ValidateParent(IDmsView value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (id == -1)
            {
                throw new NotSupportedException("The root view cannot be assigned a parent view.");
            }

            if (value.Id == id)
            {
                throw new NotSupportedException("A view cannot be its own parent.");
            }
        }

        private void ValidateName(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (String.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("The view name cannot be empty or white space.", nameof(name));
            }

            if (name.Length > 200)
            {
                throw new ArgumentException("The view name cannot exceed 200 characters.", nameof(name));
            }

            if (name[0] == '.' || name[name.Length - 1] == '.' || name[0] == ' ' || name[name.Length - 1] == ' ')
            {
                throw new ArgumentException("The view name cannot start or end with a dot or space.", nameof(name));
            }

            if (name.IndexOf('|') >= 0)
            {
                throw new ArgumentException("The view name contains a forbidden character.", nameof(name));
            }

            if (name.IndexOf('%') != name.LastIndexOf('%'))
            {
                throw new ArgumentException("The view name cannot contain more than one percentage character.", nameof(name));
            }

            var existingViewMock = cache.GetView(name);

            if (existingViewMock != null && !ReferenceEquals(existingViewMock, this))
            {
                throw new ArgumentException("A view with the specified name already exists.", nameof(name));
            }
        }

        private static IPropertyCollection<IDmsViewProperty, IDmsViewPropertyDefinition> CreateEmptyPropertyCollection()
        {
            var propertiesMock = new Mock<IPropertyCollection<IDmsViewProperty, IDmsViewPropertyDefinition>>();

            propertiesMock.Setup(properties => properties.Count).Returns(0);
            propertiesMock.Setup(properties => properties.GetEnumerator()).Returns(() => new List<IDmsViewProperty>().GetEnumerator());

            return propertiesMock.Object;
        }
    }
}
