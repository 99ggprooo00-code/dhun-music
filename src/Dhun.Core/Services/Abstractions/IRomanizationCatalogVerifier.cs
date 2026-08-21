using Dhun.Core.Models.Romanization;

namespace Dhun.Core.Services.Abstractions;

public interface IRomanizationCatalogVerifier
{
    bool Verify(RomanizationCatalogEnvelope envelope);
}
