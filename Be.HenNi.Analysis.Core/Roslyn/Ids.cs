using Be.HenNi.Analysis.Core.Model;
using Microsoft.CodeAnalysis;

namespace Be.HenNi.Analysis.Core.Roslyn;

internal static class Ids
{
    public static string Of(ISymbol symbol)
        => symbol.OriginalDefinition.GetDocumentationCommentId() ?? symbol.OriginalDefinition.ToDisplayString();

    public static Visibility VisibilityOf(Accessibility accessibility)
        => accessibility switch
        {
            Accessibility.Public => Visibility.Public,
            Accessibility.Internal => Visibility.Internal,
            Accessibility.Protected => Visibility.Protected,
            Accessibility.ProtectedOrInternal => Visibility.ProtectedInternal,
            Accessibility.ProtectedAndInternal => Visibility.PrivateProtected,
            _ => Visibility.Private
        };
}
