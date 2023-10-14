using System.Collections.Immutable;
using Be.HenNi.Analyzers.Constructions;
using Be.HenNi.Analyzers.Metrics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Be.HenNi.Analyzers.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class LackOfCohesionOfMethodAnalyzer : DiagnosticAnalyzer
{
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        
        context.RegisterSyntaxNodeAction(Parse, SyntaxKind.ClassDeclaration);
        context.RegisterSyntaxNodeAction(Parse, SyntaxKind.RecordDeclaration);
        context.RegisterSyntaxNodeAction(Parse, SyntaxKind.StructDeclaration);
    }

    private void Parse(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not TypeDeclarationSyntax type)
        {
            return;
        }

        if (!type.Members.Any())
        {
            return;
        }
        
        var threshold = GetThreshold(context);
        
        var lcom = new LcomHs(new TypeConstruction(type));
        if (lcom.Value < threshold)
        {
            return;
        }
        
        var diagnostic = Diagnostic.Create(Rule,
            type.GetLocation(),
            lcom.ForType,
            threshold,
            lcom.Value
        );
            
        context.ReportDiagnostic(diagnostic);
    }

    private static double GetThreshold(SyntaxNodeAnalysisContext context)
    {
        var config = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree);
        config.TryGetValue("dotnet_diagnostic.HE0003.threshold", out var configValue);

        var threshold = 1.1;
        if (!string.IsNullOrWhiteSpace(configValue))
        {
            double.TryParse(configValue, out threshold);
        }

        return threshold;
    }

    private static readonly DiagnosticDescriptor Rule = new(
        "HE0001", "Lack Of CKCohesion Of Methods", "{0} does not define a cohesive set. CKCohesion should be > {1}. Actual {2}.", "Design",
        DiagnosticSeverity.Warning, isEnabledByDefault: true, description: "Lack of cohesion as defined by Chidamber and Kemerer.");

    // Keep in mind: you have to list your rules here.
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Rule);
}