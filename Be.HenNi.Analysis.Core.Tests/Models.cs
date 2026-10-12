using System;
using System.IO;
using System.Linq;
using Be.HenNi.Analysis.Core.Model;
using Be.HenNi.Analysis.Core.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Be.HenNi.Analysis.Core.Tests;

/// <summary>
/// Builds the model of a type from a compilable source snippet.
/// </summary>
internal static class Models
{
    private static readonly MetadataReference[] References =
        ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
        .Split(Path.PathSeparator)
        .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path))
        .ToArray();

    public static TypeModel Of(string source, string typeMetadataName)
    {
        var compilation = Compile(source);
        var type = compilation.GetTypeByMetadataName(typeMetadataName)
                   ?? throw new ArgumentException($"{typeMetadataName} not found", nameof(typeMetadataName));
        return TypeModelBuilder.Build(type, compilation);
    }

    public static CodeBase CodeBaseOf(string source)
        => CodeBaseBuilder.Build(Compile(source));

    public static TypeModel Type(this CodeBase codeBase, string name)
        => codeBase.Types.Single(type => type.Name == name);

    private static Compilation Compile(string source)
    {
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest));
        var compilation = CSharpCompilation.Create(
            "Snippet",
            new[] { tree },
            References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

        Assert.Empty(compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
        return compilation;
    }

    public static MethodModel Method(this TypeModel type, string name)
        => type.Methods.Single(method => method.Name == name);
}
