using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using Be.HenNi.Analysis.Core.Model;
using Be.HenNi.Analysis.Core.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using MethodMetrics = Be.HenNi.Analysis.Core.Metrics.Methods;
using TypeMetrics = Be.HenNi.Analysis.Core.Metrics.Types;

namespace Be.HenNi.Analyzers.Analyzers;

/// <summary>
/// Reports the design flaws judged on a type alone, measured on the model of Be.HenNi.Analysis.Core:
/// types with too many fields or methods or with non private fields, and methods with too many statements.
/// Flaws judged against the whole code base are reported by <see cref="CodeBaseScopedDesignFlawsAnalyzer"/>.
/// Each type is modelled at the end of its analysis, from the semantic models of its code blocks,
/// so that the diagnostics stay live in the IDE.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TypeScopedDesignFlawsAnalyzer : DiagnosticAnalyzer
{
    public override void Initialize(AnalysisContext context)
    {
        // Generated code is analyzed, so that partial types are fully modelled, but never reported.
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.EnableConcurrentExecution();

        context.RegisterSymbolStartAction(start =>
        {
            if (start.Symbol is not INamedTypeSymbol { TypeKind: TypeKind.Class or TypeKind.Struct, IsImplicitlyDeclared: false } type)
            {
                return;
            }

            var semanticModels = new ConcurrentDictionary<SyntaxTree, SemanticModel>();
            start.RegisterCodeBlockAction(block => semanticModels[block.SemanticModel.SyntaxTree] = block.SemanticModel);
            start.RegisterSymbolEndAction(end => Parse(end, type, semanticModels));
        }, SymbolKind.NamedType);
    }

    private static void Parse(SymbolAnalysisContext context, INamedTypeSymbol symbol, ConcurrentDictionary<SyntaxTree, SemanticModel> semanticModels)
    {
        var type = WrittenPartOf(TypeModelBuilder.Build(symbol, tree => semanticModels[tree]), symbol, context);
        var location = symbol.Locations.First(l => l.IsInSource);
        var options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(location.SourceTree!);

        DiagnoseFields(context, type, location, options);
        DiagnoseMethods(context, type, location, options);
        DiagnoseNonPrivateFields(context, type, location);
        DiagnoseStatements(context, symbol, type);
    }

    /// <summary>
    /// The members written by the developer: those declared only in generated code (Avalonia named controls,
    /// MVVM toolkit properties and commands, designer files...) are not held against the type.
    /// </summary>
    private static TypeModel WrittenPartOf(TypeModel type, INamedTypeSymbol symbol, SymbolAnalysisContext context)
    {
        var generatedTrees = new Dictionary<SyntaxTree, bool>();
        var generatedIds = new HashSet<string>(symbol.GetMembers()
            .Where(member => IsGenerated(member, context, generatedTrees))
            .Select(member => member.GetDocumentationCommentId())
            .OfType<string>());

        return new TypeModel(type.Id, type.Name, type.Kind,
            type.Attributes.Where(attribute => !generatedIds.Contains(attribute.Id)),
            type.Methods.Where(method => !generatedIds.Contains(method.Id)));
    }

    private static bool IsGenerated(ISymbol member, SymbolAnalysisContext context, Dictionary<SyntaxTree, bool> generatedTrees)
        => member.GetAttributes().Any(attribute => attribute.AttributeClass?.ToDisplayString() == "System.CodeDom.Compiler.GeneratedCodeAttribute")
           || member.DeclaringSyntaxReferences.Length > 0
           && member.DeclaringSyntaxReferences.All(reference => IsGenerated(reference.SyntaxTree, context, generatedTrees));

    /// <summary>
    /// Same heuristics as the compiler: generated_code option, file name, or auto-generated header comment.
    /// </summary>
    private static bool IsGenerated(SyntaxTree tree, SymbolAnalysisContext context, Dictionary<SyntaxTree, bool> generatedTrees)
    {
        if (generatedTrees.TryGetValue(tree, out var known))
        {
            return known;
        }

        bool generated;
        if (context.Options.AnalyzerConfigOptionsProvider.GetOptions(tree).TryGetValue("generated_code", out var option))
        {
            generated = string.Equals(option.Trim(), "true", StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            generated = GeneratedFileSuffixes.Any(suffix => tree.FilePath.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                        || tree.GetRoot(context.CancellationToken)
                            .GetLeadingTrivia()
                            .Any(trivia => (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
                                           && (trivia.ToString().IndexOf("<auto-generated", StringComparison.OrdinalIgnoreCase) >= 0
                                               || trivia.ToString().IndexOf("<autogenerated", StringComparison.OrdinalIgnoreCase) >= 0));
        }

        generatedTrees[tree] = generated;
        return generated;
    }

    private static readonly string[] GeneratedFileSuffixes = { ".g.cs", ".g.i.cs", ".designer.cs", ".generated.cs" };

    private static void DiagnoseFields(SymbolAnalysisContext context, TypeModel type, Location location, AnalyzerConfigOptions options)
    {
        var keyword = type.Kind switch
        {
            TypeModelKind.Struct => "struct",
            TypeModelKind.Record => "record",
            TypeModelKind.RecordStruct => "record_struct",
            _ => "class"
        };
        var threshold = GetThreshold(options, TooManyFieldsRule, $"threshold.{keyword}", GetThreshold(options, TooManyFieldsRule, "threshold", 5));

        var numberOfFields = (int)new TypeMetrics.NumberOfFields().Compute(type)!;
        if (numberOfFields > threshold)
        {
            context.ReportDiagnostic(Diagnostic.Create(TooManyFieldsRule, location, type.Name, threshold, numberOfFields));
        }
    }

    private static void DiagnoseMethods(SymbolAnalysisContext context, TypeModel type, Location location, AnalyzerConfigOptions options)
    {
        var threshold = GetThreshold(options, TooManyMethodsRule, "threshold", 20);

        var numberOfMethods = (int)new TypeMetrics.NumberOfMethods().Compute(type)!;
        if (numberOfMethods <= threshold)
        {
            return;
        }

        var numberOfAccessors = type.Methods.Count(method => method.Kind is MethodModelKind.Getter or MethodModelKind.Setter or MethodModelKind.EventAccessor
                                                             && !method.IsAutoAccessor);
        context.ReportDiagnostic(Diagnostic.Create(TooManyMethodsRule, location,
            type.Name, threshold, numberOfMethods, numberOfMethods - numberOfAccessors, numberOfAccessors));
    }

    private static void DiagnoseNonPrivateFields(SymbolAnalysisContext context, TypeModel type, Location location)
    {
        var numberOfPublicAttributes = (int)new TypeMetrics.NumberOfPublicAttributes().Compute(type)!;
        if (numberOfPublicAttributes > 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(NonPrivateFieldsRule, location, type.Name, numberOfPublicAttributes));
        }
    }

    private static void DiagnoseStatements(SymbolAnalysisContext context, INamedTypeSymbol symbol, TypeModel type)
    {
        var symbolsById = symbol.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(method => method.GetDocumentationCommentId() is not null)
            .ToDictionary(method => method.GetDocumentationCommentId()!);
        var ncss = new MethodMetrics.NonCommentingSourceStatements();

        foreach (var method in type.Methods.Where(method => method.HasBody))
        {
            if (!symbolsById.TryGetValue(method.Id, out var methodSymbol)
                || methodSymbol.Locations.FirstOrDefault(l => l.IsInSource) is not { } location)
            {
                continue;
            }

            var threshold = GetThreshold(context.Options.AnalyzerConfigOptionsProvider.GetOptions(location.SourceTree!), TooManyStatementsRule, "threshold", 15);
            var numberOfStatements = (int)ncss.Compute(method)!;
            if (numberOfStatements > threshold)
            {
                context.ReportDiagnostic(Diagnostic.Create(TooManyStatementsRule, location, method.Name, threshold, numberOfStatements));
            }
        }
    }

    private static int GetThreshold(AnalyzerConfigOptions options, DiagnosticDescriptor rule, string name, int defaultValue)
        => options.TryGetValue($"dotnet_diagnostic.{rule.Id}.{name}", out var configValue)
           && int.TryParse(configValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var threshold)
            ? threshold
            : defaultValue;

    private const string Category = "Design";

    private static readonly DiagnosticDescriptor TooManyFieldsRule = new("HE0001",
        "Too Many Fields", "{0} has too many fields. Expected {1}. Actual {2}.", Category,
        DiagnosticSeverity.Warning, isEnabledByDefault: true,
        description: "A type should not contains too many fields for cohesion reasons.");

    private static readonly DiagnosticDescriptor TooManyMethodsRule = new("HE0002",
        "Too Many Methods", "{0} has too many operations. Expected {1}. Actual {2} ({3} methods and {4} accessors).", Category,
        DiagnosticSeverity.Warning, isEnabledByDefault: true,
        description: "A type should not contains too many bodies to avoid god object.");

    private static readonly DiagnosticDescriptor NonPrivateFieldsRule = new("HE0004",
        "Non private instance fields", "Type {0} leaks {1} non private instance fields", Category,
        DiagnosticSeverity.Warning, isEnabledByDefault: true,
        description: "A type should keeps its fields private to avoid breaking encapsulation.");

    private static readonly DiagnosticDescriptor TooManyStatementsRule = new("HE0005",
        "Too Many Statements", "{0} has too many statements. Expected {1}. Actual {2}.", Category,
        DiagnosticSeverity.Warning, isEnabledByDefault: true,
        description: "An operation should have few statement to stay maintainable.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(TooManyFieldsRule, TooManyMethodsRule, NonPrivateFieldsRule, TooManyStatementsRule);
}
