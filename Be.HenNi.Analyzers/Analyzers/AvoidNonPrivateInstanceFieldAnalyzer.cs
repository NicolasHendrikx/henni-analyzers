using System.Collections.Immutable;
using Be.HenNi.Analyzers.Constructions;
using Be.HenNi.Analyzers.Metrics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Be.HenNi.Analyzers.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AvoidNonPrivateInstanceFieldAnalyzer : DiagnosticAnalyzer
{
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        
        context.EnableConcurrentExecution();
        
        context.RegisterSyntaxNodeAction(Parse, 
            SyntaxKind.ClassDeclaration, SyntaxKind.RecordDeclaration, SyntaxKind.StructDeclaration);
    }

    private void Parse(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not TypeDeclarationSyntax type)
        {
            return;
        }
        
        var nonPrivateFields = new NumberOfNonPrivateInstanceFields(new TypeConstruction(type));
        if (nonPrivateFields.Value <= 0)
        {
            return;
        }
        
        var diagnostic = Diagnostic.Create(Rule,
            type.GetLocation(),
            nonPrivateFields.ForType,
            nonPrivateFields.Value
        );
            
        context.ReportDiagnostic(diagnostic);
    }

    private static readonly DiagnosticDescriptor Rule = new("HE0004", "Non private instance fields","Type {0} leaks {1} non private instance fields", "Design",
        DiagnosticSeverity.Warning, isEnabledByDefault: true, description: "A type should keeps its fields private to avoid breaking encapsulation.");

    // Keep in mind: you have to list your rules here.
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Rule);
}