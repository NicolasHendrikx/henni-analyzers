# HenNi Analyzers

Defines a set of OO Design rules based on metrics (see table below).
Those rules are used during the C# programming project at HELMo College.

Rule ID | Category | Severity | Notes 
--------|----------|----------|-------
HE0001  | Design   | Warning   | Your type has too many fields                 
HE0002  | Design   | Warning   | Your type has too many methods                
HE0003  | Design   | Warning   | Your type has too many accessible members
HE0004  | Design   | Warning   | Your type exposes non private instance fields 
HE0005  | Design   | Warning   | This method counts too many statements   
HE0006  | Security | Warning   | An empty try statement is useless and should be avoided
HE0007  | Security | Warning   | An empty catch statement is useless and should be avoided
HE0008  | Security | Warning   | An empty finally statement is useless and should be avoided
HE0009  | Design   | Warning   | Avoid to expose a concrete collection (List, Dictionary...) rather than an interface or an ObservableCollection
HE0010  | Design   | Warning   | Your type is a God Class: complex, not cohesive and using data of other types
HE0011  | Design   | Warning   | Your type is a Data Class whose data are exploited by other types
HE0012  | Design   | Warning   | This parameter list has too many parameters
HE0013  | Security | Warning   | A catch clause that only rethrows the exception is useless
HE0014  | Security | Warning   | A catch clause throws a new instance of the type it has just caught

## Configuration

You can define thresholds for rules HE0001, HE0002 and HE0005 in an .editorconfig file (default values shown).

```properties
# HE0001: instance fields, auto-properties and captured primary constructor parameters
dotnet_diagnostic.HE0001.threshold=5
# optional, by kind of type: class, struct, record, record_struct
dotnet_diagnostic.HE0001.threshold.record=8
# HE0002: methods, operators and written accessors, constructors excluded
dotnet_diagnostic.HE0002.threshold=20
# HE0005: statements of a method, nested ones included, blocks excluded
dotnet_diagnostic.HE0005.threshold=15
```

Rule HE0009 reports the accessible (public or protected) fields, properties, indexers, methods, operators, constructors
and primary constructor parameters (classes, structs and records) whose type is, or contains, a concrete collection
(a class implementing `IEnumerable`). Overrides and interface implementations are ignored.
`ObservableCollection<T>`, `ReadOnlyObservableCollection<T>` and the types derived from them are allowed.

```properties
# HE0009: also report the internal members (default false)
dotnet_diagnostic.HE0009.include_internal=true
# HE0009: more allowed types, comma separated, without type arguments
dotnet_diagnostic.HE0009.allowed_types=System.Collections.ObjectModel.Collection,System.Collections.ObjectModel.ReadOnlyCollection
```

Rule HE0012 (PMD ExcessiveParameterList) counts the parameters of methods, constructors, operators, indexers, delegates
and primary constructors (classes, structs and records), whatever their visibility. Overrides and interface
implementations are ignored: the interface is the one to blame.
Rule HE0003 (PMD ExcessivePublicCount) counts the fields, properties, indexers, events, methods and operators
that are public or internal (constructors and nested types excluded; members declared only in generated code are ignored).
Rule HE0013 (PMD AvoidRethrowingException) reports a catch clause whose only statement is `throw;` or `throw e;`
(`e` being the caught exception). It is not reported when it has a `when` filter, or when a following catch clause
of the same try would otherwise catch the exception.
Rule HE0014 (PMD AvoidThrowingNewInstanceOfSameException) reports a `throw new T(...)` inside the catch clause of
exactly the type `T`, even when the caught exception is passed as inner exception.

What these alerts mean for the design:

Rule ID | Likely consequence
--------|-------------------
HE0002  | The type holds a lot of behaviour, private methods included: it is hard to change and probably has several responsibilities. Extract a cohesive group of methods.
HE0009  | Clients are coupled to the implementation of a collection: it cannot be replaced without breaking them, and they can alter the state of the type.
HE0012  | Calls are hard to read and easy to get wrong. Group the parameters that belong together in an object, or split the operation.
HE0003  | The type exposes a large surface that clients must understand, and you must test and keep stable: it probably does too much. Split it or reduce its visibility.
HE0013  | The catch clause has the same effect as no catch at all: it adds noise, and `throw e;` even resets the stack trace. Remove it, or handle, log or wrap the exception to add context.
HE0014  | The callers that handle this type learn nothing more, and the origin of the failure is harder to find. Use `throw;`, or throw a type that carries the meaning of the failure at this level, keeping the original as inner exception.

HE0002 measures the behaviour held inside a type, HE0003 what it exposes to the outside: a type can trigger one without the other.

```properties
# HE0012: parameters of a list
dotnet_diagnostic.HE0012.threshold=5
# HE0003: accessible members of a type
dotnet_diagnostic.HE0003.threshold=20
# HE0003: internal members are counted (default true), protected ones are not (default false)
dotnet_diagnostic.HE0003.include_internal=true
dotnet_diagnostic.HE0003.include_protected=false
```

Rules HE0010-11 are detection strategies (Lanza & Marinescu) combining several metrics.
Default thresholds are those of PMD; each one can be overridden (use a dot as decimal separator).

```properties
# HE0010 God Class: ATFD > few_foreign_data && WMC >= very_high_complexity && TCC < low_cohesion
dotnet_diagnostic.HE0010.few_foreign_data=5
dotnet_diagnostic.HE0010.very_high_complexity=47
dotnet_diagnostic.HE0010.low_cohesion=0.333

# HE0011 Data Class: WOC < low_functionality
#   && (NOPA+NOAM > few_public_data && WMC < high_complexity || NOPA+NOAM > many_public_data && WMC < very_high_complexity)
#   && NDCM >= min_data_clients
# NDCM counts the methods of other types that access at least min_accessed_attributes_by_client data of the type
# and have a cyclomatic complexity of at least min_complexity_of_client.
dotnet_diagnostic.HE0011.low_functionality=0.333
dotnet_diagnostic.HE0011.few_public_data=3
dotnet_diagnostic.HE0011.many_public_data=5
dotnet_diagnostic.HE0011.high_complexity=31
dotnet_diagnostic.HE0011.very_high_complexity=47
dotnet_diagnostic.HE0011.min_data_clients=1
dotnet_diagnostic.HE0011.min_accessed_attributes_by_client=2
dotnet_diagnostic.HE0011.min_complexity_of_client=2
```

HE0010-11 are computed once the whole compilation is known: they are reported on build,
and in the IDE only when full solution analysis is enabled.