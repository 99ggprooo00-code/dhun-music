using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Dhun.WinUI.ViewModels;

/// <summary>
///     Minimal surface required by <see cref="Dhun.WinUI.Controls.PaginationControl"/>.
///     Implement on any list view model that wants to reuse the shared pager.
/// </summary>
public interface IPagedListViewModel : INotifyPropertyChanged
{
    int CurrentPage { get; }
    int TotalPages { get; }
    int SongsPerPage { get; set; }
    int TotalItemCount { get; }
    bool HasNextPage { get; }
    bool HasPreviousPage { get; }
    IAsyncRelayCommand NextPageCommand { get; }
    IAsyncRelayCommand PreviousPageCommand { get; }
}
