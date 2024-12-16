using System.Linq;
using Be.HenNi.Analyzers.Constructions;

namespace Be.HenNi.Analyzers.Metrics;

public class NonCommentingSourceStatement : MethodBasedMetric<int>
{

    public NonCommentingSourceStatement(Operation operation) : base(operation)
    { }

    public override int Value
    {
        get
        {
            var lines = Operand.Body.Split('\n');
            return lines
                .Select(line => line.TrimStart())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Count(line => IsStatement(line));
        }
        
    }

    private bool IsStatement(string line)
        => char.IsLetter(line[0]) || (line[0] != '{' && line[0] != '}');
}