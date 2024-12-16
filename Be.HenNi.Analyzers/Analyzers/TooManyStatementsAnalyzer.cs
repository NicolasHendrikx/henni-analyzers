using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
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

        context.RegisterSemanticModelAction(Parse);
    }

    private void Parse(SemanticModelAnalysisContext context)
    {
        var threshold = GetThreshold(context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.SemanticModel.SyntaxTree));
        var members = context
            .SemanticModel
            .SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<BaseMethodDeclarationSyntax>();

        foreach (var member in members)
        {
            var diagnostic = DoParse(member, threshold);
            if(diagnostic is not null) context.ReportDiagnostic(diagnostic);
        }
        
        var accessors = context
            .SemanticModel
            .SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<AccessorDeclarationSyntax>();

        foreach (var accessor in accessors)
        {
            var diagnostic = DoParse(accessor, threshold);
            
            if(diagnostic is not null) context.ReportDiagnostic(diagnostic);
        }

    }

    private Diagnostic? DoParse(AccessorDeclarationSyntax operation, int threshold)
    {
        var measure = operation.Body?.Statements.Count ?? 1;
        
        if (measure <= threshold)
        {
            return null;
        }
        
        return Diagnostic.Create(Rule,
            operation.GetLocation(),
            threshold,
            measure
        );
    }

    private Diagnostic? DoParse(BaseMethodDeclarationSyntax operation, int threshold)
    {
        var measure = operation.Body?.Statements.Count ?? 1;
        
        if (measure <= threshold)
        {
            return null;
        }
        
        return Diagnostic.Create(Rule,
            operation.GetLocation(),
            threshold,
            measure
        );
    }

    private static int GetThreshold(AnalyzerConfigOptions opetions)
    {
        opetions.TryGetValue("dotnet_diagnostic.HE0005.threshold", out var configValue);

        var threshold = 15;
        if (!string.IsNullOrWhiteSpace(configValue))
        {
            int.TryParse(configValue, out threshold);
        }

        return threshold;
    }

    private static readonly DiagnosticDescriptor Rule = new(
        "HE0005", 
        "Too Many Statements", 
        "Too many statements. Expected {0}. Actual {1}.", 
        "Design",
        DiagnosticSeverity.Warning, 
        isEnabledByDefault: true, 
        description: "An operation should have few statement to stay maintainable.");

    // Keep in mind: you have to list your rules here.
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Rule);
}