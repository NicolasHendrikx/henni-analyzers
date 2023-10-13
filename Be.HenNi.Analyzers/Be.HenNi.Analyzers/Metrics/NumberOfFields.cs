using System.Linq;
using Be.HenNi.Analyzers.Constructions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Be.HenNi.Analyzers.Metrics;

/// <summary>
/// Compute the number of instance field for a given type
/// </summary>
public class NumberOfFields : TypeBasedMetric<int>
{
    public NumberOfFields(TypeConstruction operand) : base(operand)
    {
    }
    
    public override int Value 
        => Operand.ParametersMembers.Count() + 
           Operand.ExplicitInstanceFields.Count() +
           Operand.BakedInstanceFields.Count();
}