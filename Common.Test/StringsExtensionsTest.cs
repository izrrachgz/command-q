using Common.Extensions;

namespace Common.Test.Extensions
{
    public sealed class StringsExtensionsTest
    {
        [Theory]
        [InlineData("https://example.test", true)]
        [InlineData("/relative/path", false)]
        [InlineData(null, false)]
        public void IsAbsoluteUri_Should_Validate_Absolute_Uri_Values(string? value, bool expected)
        {
            Assert.Equal(expected, value.IsAbsoluteUri());
        }

        [Theory]
        [InlineData("First", TestEnum.First)]
        [InlineData("first", TestEnum.First)]
        public void ToEnum_Should_Parse_Only_Declared_Values(string value, TestEnum expected)
        {
            Assert.Equal(expected, value.ToEnum<TestEnum>(ignoreCase: true));
            Assert.Null("99".ToEnum<TestEnum>(ignoreCase: true));
        }

        public enum TestEnum
        {
            First = 1,
            Second = 2
        }
    }
}
