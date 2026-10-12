using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using Be.HenNi.Analysis.Core.Detection.Smells;
using Be.HenNi.Analysis.Core.Metrics.Types;
using Be.HenNi.Analysis.Core.Model;
using Be.HenNi.Analysis.Core.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Be.HenNi.Analyzers.Analyzers;

/// <summary>
/// Reports the design flaws that can only be judged against the whole code base,
/// using the detection strategies of Be.HenNi.Analysis.Core.
/// Flaws judged on a type alone are reported by <see cref="TypeScopedDesignFlawsAnalyzer"/>.
/// Data Class looks at the clients of a type in the whole compilation, so the code base is modelled
/// once at the end of the compilation, from the semantic models the compiler already computed.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CodeBaseScopedDesignFlawsAnalyzer : DiagnosticAnalyzer
{
    public override void Initialize(AnalysisContext context)
    {
        // Generated code is analyzed, so that partial types are fully modelled, but never reported.
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(start =>
        {
            var semanticModels = new ConcurrentDictionary<SyntaxTree, SemanticModel>();
            start.RegisterSemanticModelAction(analysis => semanticModels[analysis.SemanticModel.SyntaxTree] = analysis.SemanticModel);
            start.RegisterCompilationEndAction(end => Parse(end, semanticModels));
        });
    }

    private static void Parse(CompilationAnalysisContext context, ConcurrentDictionary<SyntaxTree, SemanticModel> semanticModels)
    {
        var symbols = CodeBaseBuilder.TypesOf(context.Compilation).ToList();
        var codeBase = CodeBaseBuilder.Build(context.Compilation, tree => semanticModels[tree]);

        for (var i = 0; i < symbols.Count; i++)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var location = symbols[i].Locations.First(l => l.IsInSource);
            var options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(location.SourceTree!);
            var type = codeBase.Types[i];

            DiagnoseGodClass(context, type, location, options);
            DiagnoseDataClass(context, codeBase, type, location, options);
        }
    }

    private static void DiagnoseGodClass(CompilationAnalysisContext context, TypeModel type, Location location, AnalyzerConfigOptions options)
    {
        var strategy = new GodClass();
        strategy.FewForeignData = GetThreshold(options, GodClassRule, "few_foreign_data", strategy.FewForeignData);
        strategy.VeryHighComplexity = GetThreshold(options, GodClassRule, "very_high_complexity", strategy.VeryHighComplexity);
        strategy.LowCohesion = GetThreshold(options, GodClassRule, "low_cohesion", strategy.LowCohesion);

        var verdict = strategy.Evaluate(type);
        if (verdict.IsSatisfied)
        {
            context.ReportDiagnostic(Diagnostic.Create(GodClassRule, location, type.Name, verdict));
        }
    }

    private static void DiagnoseDataClass(CompilationAnalysisContext context, CodeBase codeBase, TypeModel type, Location location, AnalyzerConfigOptions options)
    {
        var strategy = new DataClass(codeBase);
        strategy.LowFunctionality = GetThreshold(options, DataClassRule, "low_functionality", strategy.LowFunctionality);
        strategy.FewPublicData = GetThreshold(options, DataClassRule, "few_public_data", strategy.FewPublicData);
        strategy.ManyPublicData = GetThreshold(options, DataClassRule, "many_public_data", strategy.ManyPublicData);
        strategy.HighComplexity = GetThreshold(options, DataClassRule, "high_complexity", strategy.HighComplexity);
        strategy.VeryHighComplexity = GetThreshold(options, DataClassRule, "very_high_complexity", strategy.VeryHighComplexity);
        strategy.MinDataClients = GetThreshold(options, DataClassRule, "min_data_clients", strategy.MinDataClients);
        strategy.MinAccessedAttributesByClient = (int)GetThreshold(options, DataClassRule, "min_accessed_attributes_by_client", strategy.MinAccessedAttributesByClient);
        strategy.MinComplexityOfClient = (int)GetThreshold(options, DataClassRule, "min_complexity_of_client", strategy.MinComplexityOfClient);

        var verdict = strategy.Evaluate(type);
        if (!verdict.IsSatisfied)
        {
            return;
        }

        var clients = new DataClients(codeBase)
            {
                MinAccessedAttributes = strategy.MinAccessedAttributesByClient,
                MinComplexity = strategy.MinComplexityOfClient
            }
            .Of(type)
            .Select(client => $"{client.Owner.Name}.{client.Method.Name}");

        context.ReportDiagnostic(Diagnostic.Create(DataClassRule, location, type.Name, string.Join(", ", clients), verdict));
    }

    private static double GetThreshold(AnalyzerConfigOptions options, DiagnosticDescriptor rule, string name, double defaultValue)
        => options.TryGetValue($"dotnet_diagnostic.{rule.Id}.{name}", out var configValue)
           && double.TryParse(configValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var threshold)
            ? threshold
            : defaultValue;

    private static readonly DiagnosticDescriptor GodClassRule = new("HE0010",
        "God Class", "{0} is a God Class: it is complex, not cohesive and uses data of other types ({1})", "Design",
        DiagnosticSeverity.Warning, isEnabledByDefault: true,
        description: "The type centralizes the intelligence of the system. Split it and move behavior next to the data it uses.",
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    private static readonly DiagnosticDescriptor DataClassRule = new("HE0011",
        "Data Class", "{0} is a Data Class: it exposes data whose logic lies in {1} ({2})", "Design",
        DiagnosticSeverity.Warning, isEnabledByDefault: true,
        description: "The type holds data without behavior, while other types contain the logic based on these data. Move that logic into the type.",
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(GodClassRule, DataClassRule);
}
