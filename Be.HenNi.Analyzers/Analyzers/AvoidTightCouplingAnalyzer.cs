﻿using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Be.HenNi.Analyzers.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AvoidTightCouplingAnalyzer : DiagnosticAnalyzer
{
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSemanticModelAction(Parse);
    }
    
    private void Parse(SemanticModelAnalysisContext context)
    {
        var members =   context.SemanticModel
            .SyntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<MemberDeclarationSyntax>()
            .ToImmutableArray();
        
        foreach (var field in members.OfType<BaseFieldDeclarationSyntax>().Where(f => f.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword))))
        {
            Diagnose(context, field, field.Declaration.Type);
        }
        
        foreach (var property in members.OfType<BasePropertyDeclarationSyntax>().Where(f => f.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword))))
        {
            Diagnose(context, property, property.Type);
        }
        
        foreach (var method in members.OfType<MethodDeclarationSyntax>().Where(f => f.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword))))
        {
            Diagnose(context, method, method.ReturnType);
            foreach (var parameter in method.ParameterList.Parameters)
            {
                if (parameter.Type != null) Diagnose(context, parameter, parameter.Type);
            }
        }
        
        foreach (var operatorDeclaration in members.OfType<OperatorDeclarationSyntax>().Where(f => f.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword))))
        {
           Diagnose(context, operatorDeclaration, operatorDeclaration.ReturnType);
           foreach (var parameter in operatorDeclaration.ParameterList.Parameters)
           {
               if (parameter.Type != null) Diagnose(context, parameter, parameter.Type);
           }
        }
        
    }

    private void Diagnose(SemanticModelAnalysisContext context, SyntaxNode node, TypeSyntax type)
    {
        var typeInfo = context
            .SemanticModel
            .GetTypeInfo(type)
            .Type;
        
        if(typeInfo is not INamedTypeSymbol namedTypeSymbol) return;
    
        var typeIsConcrete = namedTypeSymbol is
        {
            DelegateInvokeMethod: null,
            IsAbstract: false,
            IsValueType: false,
            IsRecord: false,
            SpecialType: SpecialType.None
        };
        if (typeIsConcrete)
        {
            context.ReportDiagnostic(MakeDiagnostic(node, typeInfo.ToDisplayString()));
        }
    }

    private Diagnostic MakeDiagnostic(SyntaxNode node, string fieldDeclaration)
        => Diagnostic.Create(TightCouplingRule, node.GetLocation(), fieldDeclaration);

    private static readonly DiagnosticDescriptor TightCouplingRule = new("HE0009", 
        "Tight Coupling Declaration","Avoid to expose concrete type. Consider reducing access modifier or to expose abstract or value types.", "Design",
        DiagnosticSeverity.Warning, isEnabledByDefault: true, description: 
        "Program to interface rather than to implementation type. Implementation found {0}.");
    
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(TightCouplingRule);
}