using System.Collections.Immutable;
using Be.HenNi.Analyzers.Constructions;
using Be.HenNi.Analyzers.Metrics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Be.HenNi.Analyzers.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TooManyFieldsAnalyzer : DiagnosticAnalyzer
{
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        
        context.EnableConcurrentExecution();
        
        context.RegisterSyntaxNodeAction(Parse, SyntaxKind.ClassDeclaration);
        context.RegisterSyntaxNodeAction(Parse, SyntaxKind.RecordDeclaration);
        context.RegisterSyntaxNodeAction(Parse, SyntaxKind.RecordStructDeclaration);
        context.RegisterSyntaxNodeAction(Parse, SyntaxKind.StructDeclaration);
    }

    private void Parse(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not TypeDeclarationSyntax type)
        {
            return;
        }

        var threshold = GetThreshold(context, type.Kind());
        
        var numberOfFields = new NumberOfFields(new TypeDeclaration(type));
        if (numberOfFields.Value <= threshold)
        {
            return;
        }
        
        var diagnostic = Diagnostic.Create(Rule,
            type.Identifier.GetLocation(),
            numberOfFields.ForType,
            threshold,
            numberOfFields.Value
        );
            
        context.ReportDiagnostic(diagnostic);
    }

    private static int GetThreshold(SyntaxNodeAnalysisContext context, SyntaxKind type = SyntaxKind.ClassDeclaration)
    {
        var config = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree);
        config.TryGetValue("dotnet_diagnostic.HE0001.threshold", out var configValue);

        var threshold = 5;
        if (!string.IsNullOrWhiteSpace(configValue))
        {
            int.TryParse(configValue, out threshold);
        }

        var typeKeyWord = type switch
        {
            SyntaxKind.StructDeclaration => "struct",
            SyntaxKind.RecordDeclaration => "record",
            SyntaxKind.RecordStructDeclaration => "record_struct",
            SyntaxKind.ClassDeclaration => "record_struct",
            _ => "class"
        };
        
        config.TryGetValue($"dotnet_diagnostic.HE0001.threshold.{typeKeyWord}", out var typeSpecificConfigValue);
        if (!string.IsNullOrWhiteSpace(typeSpecificConfigValue))
        {
            int.TryParse(typeSpecificConfigValue, out threshold);
        }
        
        return threshold;
    }

    private static readonly DiagnosticDescriptor Rule = new("HE0003", "Too Many Fields", "{0} has too many fields. Expected {1}. Actual {2}.", "Design",
        DiagnosticSeverity.Warning, isEnabledByDefault: true, description: "A type should not contains too many fields for cohesion reasons.");

    // Keep in mind: you have to list your rules here.
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Rule);
}