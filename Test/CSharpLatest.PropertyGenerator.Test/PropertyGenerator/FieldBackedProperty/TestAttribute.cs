namespace CSharpLatest.PropertyGenerator.Test;

using NUnit.Framework;

[TestFixture]
internal class TestAttribute
{
    [NUnit.Framework.Test]
    public void TestDefaultValues()
    {
        CSharpLatest.FieldBackedPropertyAttribute Attribute = new();

        Assert.That(Attribute.GetterText, Is.Null);
        Assert.That(Attribute.SetterText, Is.Null);
        Assert.That(Attribute.InitializerText, Is.Null);
    }
}
