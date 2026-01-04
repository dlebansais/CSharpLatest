namespace CSharpLatest.FieldBackedProperty;

/// <summary>
/// Represents the model of a field backed property attribute.
/// </summary>
/// <param name="GetterText">The getter text.</param>
/// <param name="SetterText">The setter text.</param>
/// <param name="InitializerText">The initializer text.</param>
internal record PropertyAttributeModel(string GetterText,
                                       string SetterText,
                                       string InitializerText);
