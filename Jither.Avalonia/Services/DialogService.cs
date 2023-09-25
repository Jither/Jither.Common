using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jither.Avalonia.Services;

public interface IDialogService
{
    Task<string?> OpenFile(string title, string initialPath, IReadOnlyList<string> filter);
    Task<string?> SaveFile(string title, string initialPath, IReadOnlyList<string> filter);
    Task<string?> BrowseForFolder(string title, string initialPath);
}

public class DialogService : IDialogService
{
    private Window? Owner
    {
        get
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                return desktop.MainWindow;
            }
            return null;
        }
    }

    private IStorageProvider StorageProvider => Owner?.StorageProvider ?? throw new InvalidOperationException($"No StorageProvider instance found.");

    public DialogService()
    {
    }

    public async Task<string?> OpenFile(string title, string initialPath, IReadOnlyList<string> filter)
    {
        var options = new FilePickerOpenOptions
        {
            Title = title,
            SuggestedStartLocation = await StorageProvider.TryGetFolderFromPathAsync(initialPath),
            FileTypeFilter = CreateFilter(filter)
        };
        var files = await StorageProvider.OpenFilePickerAsync(options);

        return files.Count > 0 ? files[0]?.TryGetLocalPath() : null;
    }

    public async Task<string?> SaveFile(string title, string initialPath, IReadOnlyList<string> filter)
    {
        var options = new FilePickerSaveOptions
        {
            Title = title,
            ShowOverwritePrompt = true,
            FileTypeChoices = CreateFilter(filter),
            SuggestedStartLocation = await StorageProvider.TryGetFolderFromPathAsync(initialPath),
        };
        var file = await StorageProvider.SaveFilePickerAsync(options);
        return file?.TryGetLocalPath();
    }

    public async Task<string?> BrowseForFolder(string title, string initialPath)
    {
        var options = new FolderPickerOpenOptions
        {
            Title = title,
            SuggestedStartLocation = await StorageProvider.TryGetFolderFromPathAsync(initialPath)
        };
        var folders = await StorageProvider.OpenFolderPickerAsync(options);
        return folders.Count > 0 ? folders[0]?.TryGetLocalPath() : null;
    }

    private IReadOnlyList<FilePickerFileType> CreateFilter(IReadOnlyList<string> filter)
    {
        var types = new List<FilePickerFileType>();
        foreach (var f in filter)
        {
            var type = f.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            string name = type[0];
            var patterns = type[1].Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            types.Add(new FilePickerFileType(name) { Patterns = patterns });
        }
        return types;
    }
}
