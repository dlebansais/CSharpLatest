namespace CSharpLatest;

using System;

/// <summary>
/// Represents an attribute to automatically generate an event that can run handlers using async/await.
/// </summary>
[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class AsyncEventAttribute : Attribute
{
}
