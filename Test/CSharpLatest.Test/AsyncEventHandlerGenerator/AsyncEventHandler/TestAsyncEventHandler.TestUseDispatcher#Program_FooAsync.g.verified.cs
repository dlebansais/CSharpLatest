//HintName: Program_FooAsync.g.cs
#nullable enable

namespace CSharpLatest.TestSuite;

using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using CSharpLatest;

partial class Program
{
    [GeneratedCode("CSharpLatest.Analyzers","2.6.3.42")]
    public void Foo(RoutedEventArgs args)
    {
        _ = Dispatcher.BeginInvoke(async () =>
        {
            try
            {
                await FooAsync(args).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"Fatal: exception in FooAsync.\r\n{exception.Message}\r\n{exception.StackTrace}");
                throw;
            }
        });
    }
}