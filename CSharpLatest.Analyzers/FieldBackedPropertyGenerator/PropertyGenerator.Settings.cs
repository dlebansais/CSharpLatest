namespace CSharpLatest;

using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Represents a code generator.
/// </summary>
public partial class PropertyGenerator
{
    /// <summary>
    /// The key in .csproj for the field prefix.
    /// </summary>
    public const string FieldPrefixKey = "FieldPrefix";

    /// <summary>
    /// The default value for the field prefix.
    /// </summary>
    public const string DefaultFieldPrefix = "field";

    /// <summary>
    /// The key in .csproj for the tab length in generated code.
    /// </summary>
    public const string TabLengthKey = "TabLength";

    /// <summary>
    /// The default value for the tab length in generated code.
    /// </summary>
    public const int DefaultTabLength = 4;

    // The settings values.
    private static readonly GeneratorSettingsEntry FieldPrefixSetting = new(BuildKey: FieldPrefixKey, DefaultValue: DefaultFieldPrefix);
    private static readonly GeneratorSettingsEntry TabLengthSetting = new(BuildKey: TabLengthKey, DefaultValue: $"{DefaultTabLength}");
    private static GeneratorSettings Settings = new(FieldPrefix: DefaultFieldPrefix, TabLength: DefaultTabLength);

    /// <summary>
    /// Reads settings.
    /// </summary>
    /// <param name="options">Configuration options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    internal static IEnumerable<GeneratorSettings> ReadSettings(AnalyzerConfigOptionsProvider options, CancellationToken cancellationToken)
    {
        string FieldPrefix = FieldPrefixSetting.ReadAsString(options, out _);
        int TabLength = TabLengthSetting.ReadAsInt(options, out _);

        Settings = Settings with
        {
            FieldPrefix = FieldPrefix,
            TabLength = TabLength,
        };

        return [Settings];
    }
}
