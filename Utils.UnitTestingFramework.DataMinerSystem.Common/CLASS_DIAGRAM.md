# DataMiner System Mock Class Diagram

This document shows the main classes of the DataMiner System mock and how the DMS, protocol, parameter, table, DOM, and connection parts are connected.

`+` means public, `~` means internal, `*--` means ownership, and `..>` means that one class uses or creates another class.

## DMS construction and ownership

```mermaid
classDiagram
    direction LR

    class DmsBuilder {
        +WithProtocol(...)
        +WithDma(...)
        +WithView(...)
        +WithDomDefinition(...)
        +WithDomInstance(...)
        +WithSectionDefinition(...)
        +WithDomBehaviorDefinition(...)
        +Build() IDmsMock
    }

    class DmaBuilder {
        +WithElement(...)
        ~Build(IDmsMock)
    }

    class DmsElementBuilder {
        +UnderView(...)
        +WithTable(...)
        +WithParameter~T~(...)
        ~Build(IDmsMock, IDmaMock)
    }

    class IDmsProtocolMockBuilder {
        +AddParameterDefinition(...)
        +AddTableDefinition(...)
        +Build() IDmsProtocolMock
    }

    class IDmsMock {
        +Connection IConnectionMock
        +Dom DomSystemMock
        +AddProtocol(xmlPath) IDmsProtocolMock
        +CreateAgent(...) IDmaMock
        +CreateView(...) IDmsViewMock
        +GetElementMock(...) IDmsElementMock
        +GetViewMock(...) IDmsViewMock
    }

    class Cache {
        <<internal>>
        ~Agents
        ~Elements
        ~Protocols
        ~Services
        ~Views
    }

    class IDmaMock {
        +Id int
        +GetElement(...)
        +GetService(...)
    }

    class IDmsElementMock
    class IDmsServiceMock
    class IDmsViewMock
    class IDmsProtocolMock
    class IConnectionMock
    class DomSystemMock

    DmsBuilder ..> IDmsMock : builds
    DmsBuilder ..> DmaBuilder : configures
    DmsBuilder ..> IDmsProtocolMockBuilder : configures
    DmaBuilder ..> DmsElementBuilder : configures
    DmsElementBuilder ..> IDmsElementMock : builds

    IDmsMock *-- Cache : shared state
    IDmsMock *-- IConnectionMock : Connection
    IDmsMock *-- DomSystemMock : Dom
    Cache o-- IDmaMock : agents
    Cache o-- IDmsElementMock : elements
    Cache o-- IDmsProtocolMock : protocols
    Cache o-- IDmsServiceMock : services
    Cache o-- IDmsViewMock : views
    IDmaMock o-- IDmsElementMock : creates
    IDmaMock o-- IDmsServiceMock : creates
    IDmsElementMock --> IDmsProtocolMock : uses protocol
```

## Protocol, parameters, and tables

```mermaid
classDiagram
    direction LR

    class IDmsProtocolMockBuilder {
        +AddParameterDefinition(...)
        +AddTableDefinition(...)
        +Build() IDmsProtocolMock
    }

    class IDmsProtocolMock {
        +Name string
        +ReferencedVersion string
        +Definitions ParameterAndTableDefinitions
        +AddParameterDefinition(...)
        +AddTableDefinition(...)
    }

    class ParameterAndTableDefinitions {
        +AddParameterDefinition(...)
        +AddTableDefinition(...)
        +GetParameterDefinition(...)
        +GetTableDefinition(...)
        +Populate(...)
    }

    class ParameterDefinition
    class TableSchema

    class IDmsElementMock {
        +GetStandaloneParameterMock~T~(...)
        +GetDmsTableMock(...) DmsTableMock
    }

    class ParametersAndTables {
        +ParametersAndTables(definitions)
        +GetParameter(...)
        +GetTable(...)
    }

    class IParameterModel {
        <<interface>>
        +Changed
    }

    class ITableModel {
        <<interface>>
        +RowChanged
    }

    class ParameterModel {
        +Update(...)
    }

    class TableModel {
        <<internal>>
    }

    class DmsStandaloneParameterMock~T~ {
        +Value object
        +UpdateValue(...)
    }

    class DmsTableMock {
        +GetCell(...)
        +SetCell(...)
        +GetRow(...)
        +SetRow(...)
        +SetRows(...)
    }

    class DmsColumnMock~T~ {
        <<internal>>
    }

    IDmsProtocolMockBuilder ..> IDmsProtocolMock : builds
    IDmsProtocolMock *-- ParameterAndTableDefinitions : Definitions
    ParameterAndTableDefinitions o-- ParameterDefinition : parameter definitions
    ParameterAndTableDefinitions o-- TableSchema : table definitions

    IDmsElementMock *-- ParametersAndTables : runtime values
    IDmsElementMock --> IDmsProtocolMock : obtains definitions
    ParametersAndTables *-- ParameterModel : creates
    ParametersAndTables *-- TableModel : creates
    ParameterModel ..|> IParameterModel
    TableModel ..|> ITableModel
    DmsStandaloneParameterMock~T~ --> IParameterModel : wraps
    DmsTableMock --> ITableModel : wraps
    DmsTableMock ..> DmsColumnMock~T~ : creates columns
    IDmsElementMock ..> DmsStandaloneParameterMock~T~ : exposes
    IDmsElementMock ..> DmsTableMock : exposes
```

The protocol contains definitions only. Each element uses those definitions to create its own `ParametersAndTables`, so two elements can use the same protocol while keeping separate runtime values.

## DOM and message flow

```mermaid
classDiagram
    direction LR

    class IDmsMock {
        +Connection IConnectionMock
        +Dom DomSystemMock
    }

    class IConnectionMock {
        +NotifySubscriptions(DMSMessage)
        +HandleMessages(DMSMessage[])
        +RegisterMessageHandler(...)
        +UnregisterMessageHandler(...)
        ~SetMessageHandler(...)
        ~NotifySubscriptions(parameterMessage)
        ~NotifySubscriptions(tableMessage)
    }

    class SubscriptionSet {
        <<internal>>
        ~Id string
        ~Filters
    }

    class DomSystemMock {
        ~DomSystemMock(notifySubscriptions)
        +SetInstances(...)
        +SetDefinitions(...)
        +SetSectionDefinitions(...)
        +SetBehaviorDefinitions(...)
        ~HandleMessages(...)
    }

    class DomCacheMock {
        <<internal>>
        ~Instances
        ~Definitions
        ~SectionDefinitions
        ~BehaviorDefinitions
    }

    class IDmsElementMock {
        +AgentId int
        +ElementId int
    }

    class DomInstance
    class DomDefinition
    class SectionDefinition
    class DomBehaviorDefinition
    class DomInstancesChangedEventMessage
    class ParameterChangeEventMessage
    class ParameterTableUpdateEventMessage

    class DomInstanceBuilder {
        <<external DOM package>>
    }

    class DomDefinitionBuilder {
        <<external DOM package>>
    }

    class SectionDefinitionBuilder {
        <<external DOM package>>
    }

    class DomBehaviorDefinitionBuilder {
        <<external DOM package>>
    }

    IDmsMock *-- IConnectionMock : Connection
    IDmsMock *-- DomSystemMock : Dom
    IConnectionMock *-- SubscriptionSet : stored subscriptions
    IConnectionMock ..> DomSystemMock : routes DOM SLNet requests
    DomSystemMock *-- DomCacheMock : one cache per module

    DomCacheMock o-- DomInstance
    DomCacheMock o-- DomDefinition
    DomCacheMock o-- SectionDefinition
    DomCacheMock o-- DomBehaviorDefinition

    DomInstanceBuilder ..> DomInstance : builds
    DomDefinitionBuilder ..> DomDefinition : builds
    SectionDefinitionBuilder ..> SectionDefinition : builds
    DomBehaviorDefinitionBuilder ..> DomBehaviorDefinition : builds

    DomSystemMock ..> DomInstancesChangedEventMessage : creates after CRUD
    DomSystemMock ..> IConnectionMock : publishes DOM events
    IDmsElementMock ..> ParameterChangeEventMessage : creates on value change
    IDmsElementMock ..> ParameterTableUpdateEventMessage : creates on table change
    IDmsElementMock ..> IConnectionMock : publishes parameter/table events
    IConnectionMock ..> SubscriptionSet : checks filters
```

The message flow is:

1. A parameter, table, or DOM value changes.
2. The responsible mock creates the matching DataMiner event message.
3. `IConnectionMock` checks active subscription filters.
4. When a subscription matches, `IConnection.OnNewMessage` is raised.

DOM request messages follow the opposite direction: `IConnectionMock` forwards them to `DomSystemMock.HandleMessages`, and `DomSystemMock` reads or changes the `DomCacheMock` belonging to the requested module.
