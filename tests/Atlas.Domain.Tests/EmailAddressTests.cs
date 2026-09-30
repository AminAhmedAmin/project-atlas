using Atlas.Domain.Common;

namespace Atlas.Domain.Tests;

public sealed class EmailAddressTests
{
    [Theory]
    [InlineData("jane@example.com")]
    [InlineData("  first.last+tag@sub.example.org ")]
    public void Valid_addresses_are_accepted(string input)
    {
        Assert.Equal(input.Trim(), EmailAddress.Create(input).Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("jane@localhost")]
    [InlineData("Jane <jane@example.com>")]
    public void Invalid_addresses_are_rejected(string? input)
    {
        Assert.False(EmailAddress.TryCreate(input, out _));
        Assert.Throws<DomainException>(() => EmailAddress.Create(input));
    }

    [Fact]
    public void Overlong_addresses_are_rejected()
    {
        var address = new string('a', 250) + "@example.com";

        Assert.False(EmailAddress.TryCreate(address, out _));
    }
}
