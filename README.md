# HenNi Analyzers

Defines a set of OO Design rules based on metrics (see table below).
Those rules are used during the C# programming project at HELMo College.

Rule ID | Category | Severity | Notes 
--------|----------|----------|-------
HE0001  | Design   | Warning   | Your type has too many fields                 
HE0002  | Design   | Warning   | Your type has too many methods                
HE0004  | Design   | Warning   | Your type exposes non private instance fields 
HE0005  | Design   | Warning   | This method counts too many statements   
HE0006  | Security | Warning   | An empty try statement is useless and should be avoided
HE0007  | Security | Warning   | An empty catch statement is useless and should be avoided
HE0008  | Security | Warning   | An empty finally statement is useless and should be avoided
HE0009  | Design   | Warning   | Avoid to expose dependencies on concrete class
HE0010  | Design   | Warning   | Your type is a God Class: complex, not cohesive and using data of other types
HE0011  | Design   | Warning   | Your type is a Data Class whose data are exploited by other types

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