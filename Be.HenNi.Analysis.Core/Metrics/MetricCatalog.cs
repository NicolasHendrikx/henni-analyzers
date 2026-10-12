using System.Collections.Generic;
using Be.HenNi.Analysis.Core.Model;

namespace Be.HenNi.Analysis.Core.Metrics;

public static class MetricCatalog
{
    public static IReadOnlyList<IMetric<TypeModel>> TypeMetrics { get; } = new IMetric<TypeModel>[]
    {
        new Types.NumberOfFields(),
        new Types.NumberOfMethods(),
        new Types.NumberOfPublicAttributes(),
        new Types.NumberOfAccessorMethods(),
        new Types.WeightedMethodCount(),
        new Types.AccessToForeignData(),
        new Types.TightClassCohesion(),
        new Types.WeightOfClass(),
        new Types.LackOfCohesionHendersonSellers(),
        new Types.NonCommentingSourceStatements()
    };

    public static IReadOnlyList<IMetric<MethodModel>> MethodMetrics { get; } = new IMetric<MethodModel>[]
    {
        new Methods.CyclomaticComplexity(),
        new Methods.NonCommentingSourceStatements(),
        new Methods.AccessToForeignData()
    };
}
