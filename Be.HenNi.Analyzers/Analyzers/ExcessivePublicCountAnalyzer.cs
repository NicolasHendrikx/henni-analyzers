using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Be.HenNi.Analyzers.Analyzers;

/// <summary>
/// Reports the types with too many accessible members (PMD ExcessivePublicCount): fields, properties, indexers,
/// events, methods, operators and conversions, constructors and nested types excluded.
/// Public and internal members are counted, protected ones too with <c>include_protected</c>
/// (internal ones can be left out with <c>include_internal</c>).
/// The members declared only in generated code are not held against the type.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ExcessivePublicCountAnalyzer : DiagnosticAnalyzer
{
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(Parse, SymbolKind.NamedType);
    }

    private static void Parse(SymbolAnalysisContext context)
    {
        if (context.Symbol is not INamedTypeSymbol { TypeKind: TypeKind.Class or TypeKind.Struct or TypeKind.Interface, IsImplicitlyDeclared: false } type)
        {
            return;
        }

        var generatedTrees = new Dictionary<SyntaxTree, bool>();
        var tree = type.Locations
            .Select(candidate => candidate.SourceTree)
            .OfType<SyntaxTree>()
            .FirstOrDefault(candidate => !GeneratedCode.IsGenerated(candidate, context.Options, context.CancellationToken, generatedTrees));
        if (tree is null)
        {
            return;
        }

        var location = type.Locations.First(candidate => candidate.SourceTree == tree);
        var options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(tree);
        var threshold = RuleOptions.GetInt(options, Rule.Id, "threshold", 20);
        var includeInternal = RuleOptions.GetBool(options, Rule.Id, "include_internal", true);
        var includeProtected = RuleOptions.GetBool(options, Rule.Id, "include_protected", false);

        var count = type.GetMembers().Count(member =>
            IsCountedKind(member)
            && IsCountedAccessibility(member.DeclaredAccessibility, includeInternal, includeProtected)
            && !GeneratedCode.IsGenerated(member, context.Options, context.CancellationToken, generatedTrees));
        if (count > threshold)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, location, type.Name, threshold, count));
        }
    }

    private static bool IsCountedKind(ISymbol member)
        => member switch
        {
            // Backing fields and the equality contract of a record are implicit, its positional properties are declared by its parameters.
            IFieldSymbol field => !field.IsImplicitlyDeclared,
            IPropertySymbol property => !property.IsImplicitlyDeclared || property.DeclaringSyntaxReferences.Length > 0,
            IEventSymbol => true,
            IMethodSymbol method => method.MethodKind is MethodKind.Ordinary or MethodKind.UserDefinedOperator or MethodKind.Conversion
                                    && !method.IsImplicitlyDeclared,
            _ => false
        };

    private static bool IsCountedAccessibility(Accessibility accessibility, bool includeInternal, bool includeProtected)
        => accessibility switch
        {
            Accessibility.Public => true,
            Accessibility.Internal => includeInternal,
            Accessibility.ProtectedOrInternal => includeInternal || includeProtected,
            Accessibility.Protected or Accessibility.ProtectedAndInternal => includeProtected,
            _ => false
        };

    private static readonly DiagnosticDescriptor Rule = new("HE0003",
        "Excessive Public Count", "{0} has too many accessible members. Expected {1}. Actual {2}.", "Design",
        DiagnosticSeverity.Warning, isEnabledByDefault: true,
        description: "Every accessible member is a promise made to the clients of the type: it has to be understood, tested and kept stable. "
                     + "A large surface suggests that the type has several responsibilities and probably does too much. "
                     + "Consider splitting it into smaller types, or reducing the visibility of what the clients do not need. "
                     + "This rule looks at what is exposed; HE0002 looks at the behaviour held inside the type, private methods included.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Rule);
}
