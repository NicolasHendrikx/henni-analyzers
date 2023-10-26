# HenNi Analyzers

Defines a set of OO Design rules based on metrics (see table below). 
Those rules are used during the C# programming project at HELMo College. 

Rule ID | Category | Severity | Notes 
--------|----------|----------|-------
HE0001  | Design   | Warning   | Your type has too many fields                 
HE0002  | Design   | Warning   | Your type has too many methods                
HE0003  | Design   | Warning   | Your type lacks of cohesion                   
HE0004  | Design   | Warning   | Your type exposes non private instance fields 
HE0005  | Design   | Warning   | This method counts too many statements   
HE0006  | Security | Warning   | An empty try statement is useless and should be avoided
HE0007  | Security | Warning   | An empty catch statement is useless and should be avoided
HE0008  | Security | Warning   | An empty finally statement is useless and should be avoided
HE0009  | Design   | Warning   | Avoid to expose dependencies on concrete class

## Configuration

You can define thresholds for rules HE0001-5 in an .editorconfig file.

```properties

```