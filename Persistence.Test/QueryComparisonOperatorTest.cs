using Persistence.Constants;

namespace Persistence.Test.Constants
{
    public sealed class QueryComparisonOperatorTest
    {
        [Theory]
        [InlineData(QueryComparisonOperator.Equal, "=")]
        [InlineData(QueryComparisonOperator.NotEqual, "!=")]
        [InlineData(QueryComparisonOperator.GreaterThan, ">")]
        [InlineData(QueryComparisonOperator.GreaterThanOrEqual, ">=")]
        [InlineData(QueryComparisonOperator.LessThan, "<")]
        [InlineData(QueryComparisonOperator.LessThanOrEqual, "<=")]
        public void Operator_Should_Have_Expected_Sql_Value(string actual, string expected)
        {
            Assert.Equal(expected, actual);
        }
    }
}
