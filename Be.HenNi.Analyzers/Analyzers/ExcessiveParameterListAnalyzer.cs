using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Be.HenNi.Analyzers.Analyzers;

/// <summary>
/// Reports the parameter lists with too many parameters (PMD ExcessiveParameterList): methods, constructors,
/// operators, indexers, delegates and the primary constructors of classes, structs and records.
/// Overrides and interface implementations are ignored: their list is imposed, the interface is the one to blame.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ExcessiveParameterListAnalyzer : DiagnosticAnalyzer
{
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Parse, DeclarationKinds);
    }

    private static readonly SyntaxKind[] DeclarationKinds =
    {
        SyntaxKind.MethodDeclaration, SyntaxKind.ConstructorDeclaration,
        SyntaxKind.OperatorDeclaration, SyntaxKind.ConversionOperatorDeclaration,
        SyntaxKind.IndexerDeclaration, SyntaxKind.DelegateDeclaration,
        SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration,
        SyntaxKind.RecordDeclaration, SyntaxKind.RecordStructDeclaration
    };

    private static void Parse(SyntaxNodeAnalysisContext context)
    {
        var parameters = ParameterListOf(context.Node);
        var threshold = RuleOptions.GetInt(
            context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree), Rule.Id, "threshold", 5);
        if (parameters is null || parameters.Parameters.Count <= threshold)
        {
            return;
        }

        var symbol = context.SemanticModel.GetDeclaredSymbol(context.Node, context.CancellationToken);
        if (symbol is null || IsImposed(context.Node, symbol))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, parameters.GetLocation(),
            NameOf(symbol), threshold, parameters.Parameters.Count));
    }

    private static string NameOf(ISymbol symbol)
        => symbol is IMethodSymbol { MethodKind: MethodKind.Constructor }
            ? symbol.ContainingType.Name
            : symbol.ToDisplayString(NameFormat);

    private static BaseParameterListSyntax? ParameterListOf(SyntaxNode node)
        => node switch
        {
            BaseMethodDeclarationSyntax method => method.ParameterList,
            IndexerDeclarationSyntax indexer => indexer.ParameterList,
            DelegateDeclarationSyntax @delegate => @delegate.ParameterList,
            TypeDeclarationSyntax type => type.ParameterList,
            _ => null
        };

    private static bool IsImposed(SyntaxNode node, ISymbol symbol)
        => node is BaseMethodDeclarationSyntax or IndexerDeclarationSyntax && ImposedSignature.IsImposed(symbol);

    private static readonly SymbolDisplayFormat NameFormat = new(
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        memberOptions: SymbolDisplayMemberOptions.IncludeContainingType);

    private static readonly DiagnosticDescriptor Rule = new("HE0012",
        "Excessive Parameter List", "{0} has too many parameters. Expected {1}. Actual {2}.", "Design",
        DiagnosticSeverity.Warning, isEnabledByDefault: true,
        description: "A long parameter list makes calls hard to read (which argument is which?) and easy to get wrong, and every caller depends on all of them. "
                     + "It often reveals either parameters that belong together, to group in an object of their own, "
                     + "or an operation that does several things, to split.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Rule);
}
