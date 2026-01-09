namespace CSharpLatest.AsyncEventHandlerGenerator.Test;

using NUnit.Framework;

[TestFixture]
internal class TestAttribute
{
    [NUnit.Framework.Test]
    public void TestDefaultValues()
    {
        AsyncEventHandlerAttribute Attribute = new();

        Assert.That(Attribute.WaitUntilCompletion, Is.False);
        Assert.That(Attribute.UseDispatcher, Is.False);
    }
}
