using System;
using System.Linq;
using Be.HenNi.Analyzers.Constructions;
using Be.HenNi.Analyzers.Core.Metrics;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Be.HenNi.Analyzers.Metrics;

public class CKCohesion : IMetric<double>
{
    private readonly TypeConstruction _type;

    public CKCohesion(TypeDeclarationSyntax type)
    {
        _type = new TypeConstruction(type);
    }

    public double Value
    {
        get
        {
            var fields = _type.Fields.ToList();
            if (fields.Count == 0)
            {
                return Double.NegativeInfinity;
            }
            
            var computed = _type.Computed.ToList();
            if (computed.Count == 0)
            {
                return Double.PositiveInfinity;
            }
            
            var sumOfFieldUsage = (double)_type.Fields
                .Select(field => computed.Count(method => method.DependsOn(field)))
                .Sum();
            
            return sumOfFieldUsage / (_type.Fields.Count() * computed.Count);
        }
    }

    public string ForType
        => _type.SimpleName;
}