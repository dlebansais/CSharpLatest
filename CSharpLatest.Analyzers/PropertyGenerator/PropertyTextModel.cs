namespace CSharpLatest;

/// <summary>
/// Represents the model of a property text contract.
/// </summary>
/// <param name="GetterText">The getter text.</param>
/// <param name="SetterText">The setter text.</param>
/// <param name="InitializerText">The initializer text.</param>
internal record PropertyTextModel(string GetterText,
                                  string SetterText,
                                  string InitializerText);
