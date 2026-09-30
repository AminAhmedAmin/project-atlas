using Atlas.Domain.Common;

namespace Atlas.Domain.Tests;

public sealed class HexColorTests
{
    [Theory]
    [InlineData("#1E88E5", "#1e88e5")]
    [InlineData("  #abcdef ", "#abcdef")]
    [InlineData("#abc", "#aabbcc")]
    public void Valid_colors_are_normalized(string input, string expected)
    {
        Assert.Equal(expected, HexColor.Create(input).Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1e88e5")]
    [InlineData("#12345")]
    [InlineData("#gggggg")]
    [InlineData("red")]
    public void Invalid_colors_are_rejected(string? input)
    {
        Assert.False(HexColor.TryCreate(input, out _));
        Assert.Throws<DomainException>(() => HexColor.Create(input));
    }

    [Fact]
    public void Colors_with_same_value_are_equal()
    {
        Assert.Equal(HexColor.Create("#ABCDEF"), HexColor.Create("#abcdef"));
    }
}
