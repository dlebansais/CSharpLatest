namespace CSharpLatest;

/// <summary>
/// Represents the settings of the code generator.
/// </summary>
/// <param name="FieldPrefix">The prefix for generated fields.</param>
/// <param name="TabLength">The tab length in generated code.</param>
internal record GeneratorSettings(string FieldPrefix, int TabLength);
