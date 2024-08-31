namespace CSharpLatest.Test;

public static class Prologs
{
    public const string Default = @"
using System;

";

    public const string Nullable = @"
#nullable enable

using System;

";

    public const string IsExternalInit = @"
#nullable enable

using System;
using System.ComponentModel;

namespace System.Runtime.CompilerServices
{
    internal class IsExternalInit { }
}

";
}
