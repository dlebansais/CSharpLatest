namespace CSharpLatest.FieldBackedPropertyGenerator.Test;

extern alias Analyzers;

using NUnit.Framework;
using FieldBackedPropertyGenerator = Analyzers::CSharpLatest.FieldBackedProperty.FieldBackedPropertyGenerator;
using GeneratorSettingsEntry = Analyzers::CSharpLatest.FieldBackedProperty.GeneratorSettingsEntry;

[TestFixture]
internal class TestSettings
{
    [NUnit.Framework.Test]
    public void TestAsString()
    {
        const string TestValue = "test";
        string Value;

        GeneratorSettingsEntry Entry = new(BuildKey: FieldBackedPropertyGenerator.FieldPrefixKey, DefaultValue: FieldBackedPropertyGenerator.DefaultFieldPrefix);

        Value = Entry.StringValueOrDefault(null, out bool IsNullDefault);
        Assert.That(Value, Is.EqualTo(FieldBackedPropertyGenerator.DefaultFieldPrefix));
        Assert.That(IsNullDefault, Is.True);

        Value = Entry.StringValueOrDefault(string.Empty, out bool IsEmptyDefault);
        Assert.That(Value, Is.EqualTo(FieldBackedPropertyGenerator.DefaultFieldPrefix));
        Assert.That(IsEmptyDefault, Is.True);

        Value = Entry.StringValueOrDefault(TestValue, out bool IsValueDefault);
        Assert.That(Value, Is.EqualTo(TestValue));
        Assert.That(IsValueDefault, Is.False);
    }

    [NUnit.Framework.Test]
    public void TestAsInt()
    {
        const string InvalidIntTestValue = "test";
        const int ValidIntTestValue = 1;
        int Value;

        GeneratorSettingsEntry Entry = new(BuildKey: FieldBackedPropertyGenerator.TabLengthKey, DefaultValue: $"{FieldBackedPropertyGenerator.DefaultTabLength}");

        Value = Entry.IntValueOrDefault(null, out bool IsNullDefault);
        Assert.That(Value, Is.EqualTo(FieldBackedPropertyGenerator.DefaultTabLength));
        Assert.That(IsNullDefault, Is.True);

        Value = Entry.IntValueOrDefault(string.Empty, out bool IsEmptyDefault);
        Assert.That(Value, Is.EqualTo(FieldBackedPropertyGenerator.DefaultTabLength));
        Assert.That(IsEmptyDefault, Is.True);

        Value = Entry.IntValueOrDefault(InvalidIntTestValue, out bool IsInvalidDefault);
        Assert.That(Value, Is.EqualTo(FieldBackedPropertyGenerator.DefaultTabLength));
        Assert.That(IsInvalidDefault, Is.True);

        Value = Entry.IntValueOrDefault($"{ValidIntTestValue}", out bool IsValidDefault);
        Assert.That(Value, Is.EqualTo(ValidIntTestValue));
        Assert.That(IsValidDefault, Is.False);
    }
}
