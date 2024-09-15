namespace CSharpLatest;

/// <summary>
/// Represents information about the base of a class.
/// </summary>
/// <param name="IsObject"><see langword="True"/> if the base is ultimately <see cref="object"/>.</param>
/// <param name="Depth">The base depth.</param>
internal record BaseInfo(bool IsObject, int Depth);
