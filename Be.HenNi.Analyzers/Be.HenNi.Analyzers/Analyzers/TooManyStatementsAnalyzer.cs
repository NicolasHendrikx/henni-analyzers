using System.Collections.Immutable;
using Be.HenNi.Analyzers.Constructions;
using Be.HenNi.Analyzers.Metrics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Be.HenNi.Analyzers.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TooManyStatementsAnalyzer : DiagnosticAnalyzer
{
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        
        context.EnableConcurrentExecution();
        
        context.RegisterSyntaxNodeAction(Parse, SyntaxKind.MethodDeclaration);
        context.RegisterSyntaxNodeAction(Parse, SyntaxKind.PropertyDeclaration);
        context.RegisterSyntaxNodeAction(Parse, SyntaxKind.IndexerDeclaration);
        context.RegisterSyntaxNodeAction(Parse, SyntaxKind.EventDeclaration);
    }

    private void Parse(SyntaxNodeAnalysisContext context)
    {
        int threshold = GetThreshold(context);

        switch (context.Node)
        {
            case MethodDeclarationSyntax method:
                var diagnostic = DoParse(method, threshold);
                if (diagnostic != null)
                {
                    context.ReportDiagnostic(diagnostic);
                }
                break;
            case BasePropertyDeclarationSyntax baseProp when baseProp.AccessorList != null:
                foreach (var accessor in baseProp.AccessorList.Accessors)
                {
                    DoParse(accessor, threshold);
                }
                break;
            case BasePropertyDeclarationSyntax baseProp:
                DoParse(baseProp, threshold);
                break;
        }
    }

    private Diagnostic? DoParse(CSharpSyntaxNode method, int threshold)
    {
        var metric = new NonCommentingSourceStatement(MethodDeclarationSimpleFactory.FromNode(method));
        if (metric.Value <= threshold)
        {
            return null;
        }
        
        return Diagnostic.Create(Rule,
            method.GetLocation(),
            metric.ForType,
            threshold,
            metric.Value
        );
    }

    private static int GetThreshold(SyntaxNodeAnalysisContext context)
    {
        var config = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree);
        config.TryGetValue("dotnet_diagnostic.HE0005.threshold", out var configValue);

        var threshold = 15;
        if (!string.IsNullOrWhiteSpace(configValue))
        {
            int.TryParse(configValue, out threshold);
        }

        return threshold;
    }

    private static readonly DiagnosticDescriptor Rule = new("HE0005", "Too Many Statements", "{0} has too many statements. Expected {1}. Actual {2}.", "Design",
        DiagnosticSeverity.Warning, isEnabledByDefault: true, description: "A methode should have few statement to stay maintainable.");

    // Keep in mind: you have to list your rules here.
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Rule);
}