using Common.Extensions;

namespace Application.Test.Modules.Common.Extensions
{
    public sealed class EnumerableExtensionsTest
    {
        [Fact]
        public void NotEmpty_Should_Work_For_Any_Item_Type()
        {
            Assert.True(new[] { 1 }.NotEmpty());
            Assert.False(Array.Empty<Guid>().NotEmpty());
        }

        [Fact]
        public void Empty_Should_Treat_Null_And_Empty_Sequences_As_Empty()
        {
            IEnumerable<string>? missing = null;

            Assert.True(missing.Empty());
            Assert.True(Array.Empty<string>().Empty());
            Assert.False(new[] { "value" }.Empty());
        }
    }
}
