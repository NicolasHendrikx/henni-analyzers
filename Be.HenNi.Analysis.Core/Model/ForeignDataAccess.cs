namespace Be.HenNi.Analysis.Core.Model;

/// <summary>
/// An access to an attribute of an unrelated type, either directly or through an accessor.
/// </summary>
public sealed class ForeignDataAccess
{
    public ForeignDataAccess(string memberId, string providerTypeId)
    {
        MemberId = memberId;
        ProviderTypeId = providerTypeId;
    }

    public string MemberId { get; }

    public string ProviderTypeId { get; }

    public override bool Equals(object? obj)
        => obj is ForeignDataAccess other && other.MemberId == MemberId;

    public override int GetHashCode()
        => MemberId.GetHashCode();

    public override string ToString()
        => MemberId;
}
