#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CSharpLatest.Test;

public static class Prologs
{
    public const string Default = @"
using System;
using System.Threading.Tasks;
using CSharpLatest;

";

    public const string Nullable = @"
#nullable enable

using System;
using System.Threading.Tasks;
using CSharpLatest;

";

    public const string IsExternalInit = @"
#nullable enable

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using CSharpLatest;

namespace System.Runtime.CompilerServices
{
    internal class IsExternalInit { }
}

";

    public const string IsExternalInitNoNullable = @"
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using CSharpLatest;

namespace System.Runtime.CompilerServices
{
    internal class IsExternalInit { }
}

";
}
