using System;
using System.Linq;
using Be.HenNi.Analyzers.Constructions;
using Be.HenNi.Analyzers.Core.Metrics;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Be.HenNi.Analyzers.Metrics;

public class LcomHs : TypeBasedMetric<double>
{
    public LcomHs(TypeDeclaration type) : base(type)
    { }

    public override double Value
    {
        get
        {
            var fields = Operand.Data.ToList();
            if (fields.Count == 0)
            {
                return Double.NegativeInfinity;
            }
            
            var computed = Operand.Operations.ToList();
            if (computed.Count == 0)
            {
                return Double.NegativeInfinity;
            }
            
            var sumOfFieldUsage = (double)Operand.Data
                .Select(field => computed.Count(method => method.DependsOn(field)))
                .Sum();

            return (sumOfFieldUsage/fields.Count - computed.Count - 1) / (- computed.Count);
        }
    }
}