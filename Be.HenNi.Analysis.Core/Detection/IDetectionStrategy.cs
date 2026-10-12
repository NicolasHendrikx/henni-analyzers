namespace Be.HenNi.Analysis.Core.Detection;

/// <summary>
/// Detects a design flaw by combining metrics and thresholds (Marinescu, 2004).
/// </summary>
public interface IDetectionStrategy<in TSubject>
{
    string Name { get; }

    string Description { get; }

    Verdict Evaluate(TSubject subject);
}
