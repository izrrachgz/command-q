using Common.Models.Request;

namespace Common.Test;

public sealed class DateRangeCriteriaTest
{
    [Fact]
    public void SearchCriteria_Should_Reuse_The_Date_Range_Contract()
    {
        DateTimeOffset start = new(2026, 9, 1, 0, 0, 0, TimeSpan.Zero);
        DateTimeOffset end = new(2026, 9, 30, 23, 59, 59, TimeSpan.Zero);

        DateRangeCriteria criteria = new SearchCriteria
        {
            StartDate = start,
            EndDate = end
        };

        Assert.Equal(start, criteria.StartDate);
        Assert.Equal(end, criteria.EndDate);
    }
}
