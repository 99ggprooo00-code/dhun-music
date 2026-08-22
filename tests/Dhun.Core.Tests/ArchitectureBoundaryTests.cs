using System.Reflection;
using Dhun.Core.Sources.Local;
using FluentAssertions;

namespace Dhun.Core.Tests;

public sealed class ArchitectureBoundaryTests
{
    [Fact]
    public void Core_assembly_does_not_reference_WinUI_or_windows_app_sdk()
    {
        var referencedAssemblies = typeof(LocalMusicSource).Assembly
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name)
            .Where(name => name is not null)
            .ToArray();

        referencedAssemblies.Should().NotContain(name =>
            name!.Equals("Microsoft.UI.Xaml", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("Microsoft.WindowsAppSDK", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("Microsoft.WindowsAppRuntime", StringComparison.OrdinalIgnoreCase));
    }
}
