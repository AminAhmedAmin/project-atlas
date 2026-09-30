using Atlas.Domain.Common;
using Atlas.Domain.Content;

namespace Atlas.Domain.Tests;

public sealed class ServiceTests
{
    [Fact]
    public void Create_sets_fields_and_timestamps()
    {
        var service = Service.Create(" Web apps ", "We build web apps", null, "code", 1, true, TestTime.Now);

        Assert.Equal("Web apps", service.Title);
        Assert.Equal(TestTime.Now, service.CreatedAtUtc);
        Assert.Equal(TestTime.Now, service.UpdatedAtUtc);
        Assert.True(service.IsPublished);
    }

    [Fact]
    public void Update_keeps_created_time()
    {
        var service = Service.Create("Web apps", "Summary", null, null, 0, false, TestTime.Now);
        var later = TestTime.Now.AddDays(1);

        service.Update("Mobile apps", "Other summary", "Details", "phone", 2, true, later);

        Assert.Equal("Mobile apps", service.Title);
        Assert.Equal(TestTime.Now, service.CreatedAtUtc);
        Assert.Equal(later, service.UpdatedAtUtc);
        Assert.Equal(2, service.DisplayOrder);
    }

    [Theory]
    [InlineData("", "Summary", 0)]
    [InlineData("Title", "", 0)]
    [InlineData("Title", "Summary", -1)]
    public void Invalid_input_is_rejected(string title, string summary, int order)
    {
        Assert.Throws<DomainException>(() =>
            Service.Create(title, summary, null, null, order, true, TestTime.Now));
    }
}
