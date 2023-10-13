﻿using System.Collections.Immutable;
using Be.HenNi.Analyzers.Constructions;
using Be.HenNi.Analyzers.Metrics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Be.HenNi.Analyzers.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TooManyMethodsAnalyzer : DiagnosticAnalyzer
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

        var threshold = GetThreshold(context);
        
        var numberOfMethods = new NumberOfMethods(new TypeConstruction(type));
        if (numberOfMethods.Value <= threshold)
        {
            return;
        }
        
        var diagnostic = Diagnostic.Create(Rule,
            type.GetLocation(),
            numberOfMethods.ForType,
            threshold,
            numberOfMethods.Value
        );
            
        context.ReportDiagnostic(diagnostic);
    }
    
    private static int GetThreshold(SyntaxNodeAnalysisContext context)
    {
        var config = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree);
        config.TryGetValue("dotnet_diagnostic.HE0002.threshold", out var configValue);

        var threshold = 2;
        if (!string.IsNullOrWhiteSpace(configValue))
        {
            int.TryParse(configValue, out threshold);
        }

        return threshold;
    }

    private static readonly DiagnosticDescriptor Rule = new("HE0002", "Too Many Methods", "{0} has too many bodies. Expected {1}. Actual {2}.", "Design",
        DiagnosticSeverity.Warning, isEnabledByDefault: true, description: "A type should not contains too many bodies to avoid god object.");

    // Keep in mind: you have to list your rules here.
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Rule);
}