using Dhun.Core.Models.Romanization;

namespace Dhun.Core.Services.Abstractions;

public interface IRomanizationProvider
{
    string EngineId { get; }

    bool Supports(string text);

    Task<string?> RomanizeAsync(string text, InstalledRomanizationPack pack, CancellationToken cancellationToken = default);

    Task<bool> ValidatePackAsync(InstalledRomanizationPack pack, CancellationToken cancellationToken = default);
}
