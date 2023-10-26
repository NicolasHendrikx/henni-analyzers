using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Be.HenNi.Analyzers.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AvoidEmptyTryCatchFinallyBlockAnalyzer : DiagnosticAnalyzer
{
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Parse, SyntaxKind.TryStatement);
    }

    private void Parse(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not TryStatementSyntax asTry)
        {
            return;
        }

        if (!asTry.Block.Statements.Any())
        {
            context.ReportDiagnostic(MakeDiagnostic(RuleEmptyTry, asTry));
        }

        var emptyCatches = asTry
            .Catches
            .Where(catchBlock => !catchBlock.Block.Statements.Any());
        foreach (var emptyCatch in emptyCatches)
        {
            context.ReportDiagnostic(MakeDiagnostic(RuleEmptyCatch, emptyCatch));
        }

        if (!asTry.Finally?.Block.Statements.Any() ?? false)
        {
            context.ReportDiagnostic(
                MakeDiagnostic(RuleEmptyFinally, asTry.Finally ?? throw new InvalidOperationException("Finally block should be defined")));
        }
    }

    private Diagnostic MakeDiagnostic(DiagnosticDescriptor descriptor, SyntaxNode node)
     => Diagnostic.Create(descriptor, node.GetLocation());
    
    private static readonly DiagnosticDescriptor RuleEmptyTry = new("HE0006", "Empty try statement","Avoid empty try statement", "Security",
        DiagnosticSeverity.Warning, isEnabledByDefault: true, description: "An empty try statement is useless and should be avoided.");

    private static readonly DiagnosticDescriptor RuleEmptyCatch = new("HE0007", "Empty catch statement","Avoid empty catch statement", "Security",
        DiagnosticSeverity.Warning, isEnabledByDefault: true, description: "An empty catch statement is useless and should be avoided.");

    private static readonly DiagnosticDescriptor RuleEmptyFinally = new("HE0008", "Empty finally statement","Avoid empty finally statement", "Security",
        DiagnosticSeverity.Warning, isEnabledByDefault: true, description: "An empty finally statement is useless and should be avoided.");
    
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(RuleEmptyTry, RuleEmptyCatch, RuleEmptyFinally);
}