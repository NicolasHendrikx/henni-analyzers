using System.Globalization;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Be.HenNi.Analyzers.Analyzers;

/// <summary>
/// Reads the options of a rule from an .editorconfig: <c>dotnet_diagnostic.HE0000.name = value</c>.
/// </summary>
internal static class RuleOptions
{
    public static int GetInt(AnalyzerConfigOptions options, string ruleId, string name, int defaultValue)
        => options.TryGetValue($"dotnet_diagnostic.{ruleId}.{name}", out var value)
           && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number)
            ? number
            : defaultValue;

    public static double GetDouble(AnalyzerConfigOptions options, string ruleId, string name, double defaultValue)
        => options.TryGetValue($"dotnet_diagnostic.{ruleId}.{name}", out var value)
           && double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number)
            ? number
            : defaultValue;

    public static bool GetBool(AnalyzerConfigOptions options, string ruleId, string name, bool defaultValue)
        => options.TryGetValue($"dotnet_diagnostic.{ruleId}.{name}", out var value)
           && bool.TryParse(value.Trim(), out var flag)
            ? flag
            : defaultValue;
}
