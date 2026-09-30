using Atlas.Domain.Common;

namespace Atlas.Application.Common;

internal static class DomainErrors
{
    /// <summary>Maps a domain rule violation that slipped past validation into a result error.</summary>
    public static Error From(DomainException exception) => new("Domain", exception.Message);
}
