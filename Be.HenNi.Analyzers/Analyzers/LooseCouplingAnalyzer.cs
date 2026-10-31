using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Be.HenNi.Analyzers.Analyzers;

/// <summary>
/// Reports the accessible members that expose a concrete collection (<c>List&lt;T&gt;</c>, <c>Dictionary&lt;K,V&gt;</c>...)
/// rather than an interface or an <c>ObservableCollection&lt;T&gt;</c>: fields, properties, indexers, methods,
/// operators, constructors and the primary constructor parameters of classes, structs and records.
/// A member is accessible when it and all its containing types are public or protected
/// (internal too, with <c>include_internal</c>).
/// Members whose signature is imposed (overrides, interface implementations) are ignored.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class LooseCouplingAnalyzer : DiagnosticAnalyzer
{
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(start =>
        {
            var enumerable = start.Compilation.GetSpecialType(SpecialType.System_Collections_IEnumerable);
            start.RegisterSyntaxNodeAction(node => new Inspection(node, enumerable).Run(), DeclarationKinds);
        });
    }

    private static readonly SyntaxKind[] DeclarationKinds =
    {
        SyntaxKind.FieldDeclaration, SyntaxKind.PropertyDeclaration, SyntaxKind.IndexerDeclaration,
        SyntaxKind.MethodDeclaration, SyntaxKind.ConstructorDeclaration,
        SyntaxKind.OperatorDeclaration, SyntaxKind.ConversionOperatorDeclaration,
        SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration,
        SyntaxKind.RecordDeclaration, SyntaxKind.RecordStructDeclaration
    };

    /// <summary>
    /// Collections meant to be exposed: they bring more than any interface does (change notifications for binding).
    /// More can be allowed with <c>dotnet_diagnostic.HE0009.allowed_types</c>.
    /// </summary>
    private static readonly string[] DefaultAllowedTypes =
    {
        "System.Collections.ObjectModel.ObservableCollection",
        "System.Collections.ObjectModel.ReadOnlyObservableCollection"
    };

    private sealed class Inspection
    {
        private readonly SyntaxNodeAnalysisContext _context;
        private readonly INamedTypeSymbol _enumerable;
        private readonly bool _includeInternal;
        private readonly HashSet<string> _allowedTypes = new(DefaultAllowedTypes);

        public Inspection(SyntaxNodeAnalysisContext context, INamedTypeSymbol enumerable)
        {
            _context = context;
            _enumerable = enumerable;

            var options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree);
            _includeInternal = RuleOptions.GetBool(options, Rule.Id, "include_internal", false);
            if (options.TryGetValue($"dotnet_diagnostic.{Rule.Id}.allowed_types", out var allowedTypes))
            {
                foreach (var allowedType in allowedTypes.Split(',', ';'))
                {
                    _allowedTypes.Add(allowedType.Trim());
                }
            }
        }

        public void Run()
        {
            switch (_context.Node)
            {
                case FieldDeclarationSyntax field:
                    Check(field.Declaration.Variables[0], field.Declaration.Type);
                    break;
                case PropertyDeclarationSyntax property:
                    Check(property, property.Type);
                    break;
                case IndexerDeclarationSyntax indexer:
                    Check(indexer, indexer.Type);
                    CheckParameters(indexer.ParameterList);
                    break;
                case MethodDeclarationSyntax method:
                    Check(method, method.ReturnType);
                    CheckParameters(method.ParameterList);
                    break;
                case ConstructorDeclarationSyntax constructor:
                    CheckParameters(constructor.ParameterList);
                    break;
                case OperatorDeclarationSyntax operatorDeclaration:
                    Check(operatorDeclaration, operatorDeclaration.ReturnType);
                    CheckParameters(operatorDeclaration.ParameterList);
                    break;
                case ConversionOperatorDeclarationSyntax conversion:
                    Check(conversion, conversion.Type);
                    CheckParameters(conversion.ParameterList);
                    break;
                case TypeDeclarationSyntax type:
                    // Primary constructor: positional parameters of a record, captured parameters of a class or struct.
                    CheckParameters(type.ParameterList);
                    break;
            }
        }

        private void CheckParameters(BaseParameterListSyntax? parameters)
        {
            foreach (var parameter in parameters?.Parameters ?? default)
            {
                Check(parameter, parameter.Type);
            }
        }

        private void Check(SyntaxNode declaration, TypeSyntax? typeSyntax)
        {
            if (typeSyntax is null
                || _context.SemanticModel.GetDeclaredSymbol(declaration, _context.CancellationToken) is not { } symbol
                || !IsAccessible(symbol)
                || ImposedSignature.IsImposed(symbol))
            {
                return;
            }

            ITypeSymbol? type = symbol switch
            {
                IFieldSymbol field => field.Type,
                IPropertySymbol property => property.Type,
                IMethodSymbol method => method.ReturnType,
                IParameterSymbol parameter => parameter.Type,
                _ => null
            };

            foreach (var collection in ConcreteCollections(type).Distinct(SymbolEqualityComparer.Default).OfType<INamedTypeSymbol>())
            {
                var location = LocateIn(typeSyntax, collection) ?? typeSyntax;
                _context.ReportDiagnostic(Diagnostic.Create(Rule, location.GetLocation(), collection.ToDisplayString()));
            }
        }

        /// <summary>
        /// The nested syntax of <paramref name="type"/>, e.g. <c>List&lt;int&gt;</c> in <c>Task&lt;List&lt;int&gt;&gt;</c>.
        /// </summary>
        private TypeSyntax? LocateIn(TypeSyntax syntax, ISymbol? type)
            => syntax.DescendantNodesAndSelf()
                .OfType<TypeSyntax>()
                .FirstOrDefault(candidate => SymbolEqualityComparer.Default.Equals(
                    _context.SemanticModel.GetTypeInfo(candidate, _context.CancellationToken).Type, type));

        private bool IsAccessible(ISymbol symbol)
        {
            // A parameter is as accessible as its method.
            for (var current = symbol is IParameterSymbol parameter ? parameter.ContainingSymbol : symbol;
                 current is not null and not INamespaceSymbol;
                 current = current.ContainingSymbol)
            {
                var accessible = current.DeclaredAccessibility switch
                {
                    Accessibility.Public or Accessibility.Protected or Accessibility.ProtectedOrInternal => true,
                    Accessibility.Internal or Accessibility.ProtectedAndInternal => _includeInternal,
                    _ => false
                };
                if (!accessible)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// The concrete collections found in <paramref name="type"/>: the type itself, or else the ones nested
        /// in its type arguments (<c>Task&lt;List&lt;int&gt;&gt;</c>), tuple elements or array elements.
        /// </summary>
        private IEnumerable<INamedTypeSymbol> ConcreteCollections(ITypeSymbol? type)
        {
            switch (type)
            {
                case IArrayTypeSymbol array:
                    return ConcreteCollections(array.ElementType);
                case INamedTypeSymbol named when IsConcreteCollection(named):
                    return new[] { named };
                case INamedTypeSymbol named:
                    return named.TypeArguments.SelectMany(ConcreteCollections);
                default:
                    return Enumerable.Empty<INamedTypeSymbol>();
            }
        }

        private bool IsConcreteCollection(INamedTypeSymbol type)
            => type is { TypeKind: TypeKind.Class, IsAbstract: false, IsRecord: false, SpecialType: not SpecialType.System_String }
               && type.AllInterfaces.Contains(_enumerable, SymbolEqualityComparer.Default)
               && !IsAllowed(type);

        /// <summary>
        /// Allowed types and the ones derived from them, e.g. <c>class Playlists : ObservableCollection&lt;Playlist&gt;</c>.
        /// </summary>
        private bool IsAllowed(INamedTypeSymbol type)
        {
            for (INamedTypeSymbol? current = type; current is not null; current = current.BaseType)
            {
                var name = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", current.ContainingNamespace.ToDisplayString(), current.Name);
                if (_allowedTypes.Contains(name))
                {
                    return true;
                }
            }

            return false;
        }
    }

    private static readonly DiagnosticDescriptor Rule = new("HE0009",
        "Loose coupling", "Avoid to expose the concrete collection {0}. Expose an interface or an ObservableCollection instead.", "Design",
        DiagnosticSeverity.Warning, isEnabledByDefault: true,
        description: "Exposing a concrete collection couples every client to that implementation: it can no longer be replaced "
                     + "(by a read-only view, another structure...) without breaking them, and the clients can freely alter the state of the type. "
                     + "Program to interface rather than to implementation type: expose IList<T>, IReadOnlyList<T>, IDictionary<K,V>, ISet<T>, IEnumerable<T>... "
                     + "rather than List<T>, Dictionary<K,V>, HashSet<T>... Consider also reducing the access modifier.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Rule);
}
