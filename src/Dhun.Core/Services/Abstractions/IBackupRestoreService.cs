using Dhun.Core.Services.Data;

namespace Dhun.Core.Services.Abstractions;

public interface IBackupRestoreService
{
    Task<BackupResult> CreateBackupAsync(string destinationFolderPath);
    Task<RestoreResult> RestoreFromBackupAsync(string backupFilePath);
    Task<bool> ValidateBackupAsync(string backupFilePath);
}
