namespace CSharpLatest;

using System;

/// <summary>
/// Represents an attribute to automatically generate the text of a field backed property.
/// The 'field' keyword is converted to a field declaration for frameworks that don't support the keyword.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class FieldBackedPropertyAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the text to generate for the getter.
    /// </summary>
    public string? GetterText { get; set; }

    /// <summary>
    /// Gets or sets the text to generate for the setter.
    /// </summary>
    public string? SetterText { get; set; }

    /// <summary>
    /// Gets or sets the text to generate for the initializer.
    /// </summary>
    public string? InitializerText { get; set; }
}
