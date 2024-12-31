namespace CSharpLatest.PropertyGenerator.Test;

extern alias Analyzers;

using NUnit.Framework;
using PropertyGenerator = Analyzers::CSharpLatest.PropertyGenerator;
using GeneratorSettingsEntry = Analyzers::CSharpLatest.GeneratorSettingsEntry;

[TestFixture]
internal class TestSettings
{
    [Test]
    public void TestAsString()
    {
        const string TestValue = "test";
        string Value;

        GeneratorSettingsEntry Entry = new(BuildKey: PropertyGenerator.FieldPrefixKey, DefaultValue: PropertyGenerator.DefaultFieldPrefix);

        Value = Entry.StringValueOrDefault(null, out bool IsNullDefault);
        Assert.That(Value, Is.EqualTo(PropertyGenerator.DefaultFieldPrefix));
        Assert.That(IsNullDefault, Is.True);

        Value = Entry.StringValueOrDefault(string.Empty, out bool IsEmptyDefault);
        Assert.That(Value, Is.EqualTo(PropertyGenerator.DefaultFieldPrefix));
        Assert.That(IsEmptyDefault, Is.True);

        Value = Entry.StringValueOrDefault(TestValue, out bool IsValueDefault);
        Assert.That(Value, Is.EqualTo(TestValue));
        Assert.That(IsValueDefault, Is.False);
    }

    [Test]
    public void TestAsInt()
    {
        const string InvalidIntTestValue = "test";
        const int ValidIntTestValue = 1;
        int Value;

        GeneratorSettingsEntry Entry = new(BuildKey: PropertyGenerator.TabLengthKey, DefaultValue: $"{PropertyGenerator.DefaultTabLength}");

        Value = Entry.IntValueOrDefault(null, out bool IsNullDefault);
        Assert.That(Value, Is.EqualTo(PropertyGenerator.DefaultTabLength));
        Assert.That(IsNullDefault, Is.True);

        Value = Entry.IntValueOrDefault(string.Empty, out bool IsEmptyDefault);
        Assert.That(Value, Is.EqualTo(PropertyGenerator.DefaultTabLength));
        Assert.That(IsEmptyDefault, Is.True);

        Value = Entry.IntValueOrDefault(InvalidIntTestValue, out bool IsInvalidDefault);
        Assert.That(Value, Is.EqualTo(PropertyGenerator.DefaultTabLength));
        Assert.That(IsInvalidDefault, Is.True);

        Value = Entry.IntValueOrDefault($"{ValidIntTestValue}", out bool IsValidDefault);
        Assert.That(Value, Is.EqualTo(ValidIntTestValue));
        Assert.That(IsValidDefault, Is.False);
    }
}
