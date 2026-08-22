namespace Dhun.Core.Sources;

/// <summary>Provides search within a music source.</summary>
public interface ISearchSource
{
    Task<SourceSearchPage> SearchAsync(
        SourceSearchQuery query,
        CancellationToken cancellationToken = default);
}
