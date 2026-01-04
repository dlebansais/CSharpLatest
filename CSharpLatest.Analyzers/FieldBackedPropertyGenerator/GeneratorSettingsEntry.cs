namespace CSharpLatest.FieldBackedProperty;

using System.Globalization;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Represents a setting in the .csproj project file.
/// <para>
/// <PropertyGroup>
///     <PropertySourceGenerator_FieldPrefix>demoField</PropertySourceGenerator_FieldPrefix>
/// </PropertyGroup>
/// <ItemGroup>
///     <CompilerVisibleProperty Include="PropertySourceGenerator_FieldPrefix" />
/// </ItemGroup>
/// </para>
/// </summary>
/// <param name="BuildKey">The key in the project file.</param>
/// <param name="DefaultValue">The default value.</param>
internal record GeneratorSettingsEntry(string BuildKey, string DefaultValue)
{
    private const string BuilProperty = "build_property";
    private const string PropertySourceGenerator = "PropertySourceGenerator";

    // Stryker disable once String: this line is untestable.
    private static string ProjectKey(string key) => $"{BuilProperty}.{PropertySourceGenerator}_{key}";

    /// <summary>
    /// Reads the setting as a string, always returning a valid value, using the default value if necessary.
    /// </summary>
    /// <param name="options">The settings to read.</param>
    /// <param name="isDefault"><see langword="true"/> if the default value is returned; otherwise, <see langword="false"/>.</param>
    /// <returns>The current setting value as a string if valid, the default value otherwise.</returns>
    public string ReadAsString(AnalyzerConfigOptionsProvider options, out bool isDefault)
    {
        _ = options.GlobalOptions.TryGetValue(ProjectKey(BuildKey), out string? Value);
        return StringValueOrDefault(Value, out isDefault);
    }

    /// <summary>
    /// Returns <paramref name="value"/> as a string if valid, the default value otherwise.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="isDefault"><see langword="true"/> if the default value is returned; otherwise, <see langword="false"/>.</param>
    /// <returns><paramref name="value"/> as a string if valid, the default value otherwise.</returns>
    public string StringValueOrDefault(string? value, out bool isDefault)
    {
        if (value is not null && value != string.Empty)
        {
            isDefault = false;
            return value;
        }

        isDefault = true;
        return DefaultValue;
    }

    /// <summary>
    /// Reads the setting as an int, always returning a valid value, using the default value if necessary.
    /// </summary>
    /// <param name="options">The settings to read.</param>
    /// <param name="isDefault"><see langword="true"/> if the default value is returned; otherwise, <see langword="false"/>.</param>
    /// <returns>The current setting value as an int if valid, the default value otherwise.</returns>
    public int ReadAsInt(AnalyzerConfigOptionsProvider options, out bool isDefault)
    {
        _ = options.GlobalOptions.TryGetValue(ProjectKey(BuildKey), out string? Value);
        return IntValueOrDefault(Value, out isDefault);
    }

    /// <summary>
    /// Returns <paramref name="value"/> as an int if valid, the default value otherwise.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="isDefault"><see langword="true"/> if the default value is returned; otherwise, <see langword="false"/>.</param>
    /// <returns><paramref name="value"/> as an int if valid, the default value otherwise.</returns>
    public int IntValueOrDefault(string? value, out bool isDefault)
    {
        if (value is not null && int.TryParse(value, out int IntValue))
        {
            isDefault = false;
            return IntValue;
        }

        isDefault = true;
        return int.Parse(DefaultValue, CultureInfo.InvariantCulture);
    }
}
