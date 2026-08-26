using Microsoft.Extensions.DependencyInjection;
using Time.Extensions;

namespace Time.Test.Extensions
{
    public sealed class TimeServiceCollectionExtensionsTest
    {
        [Fact]
        public void AddTimeServices_Should_Register_System_Provider_By_Default()
        {
            var services = new ServiceCollection();

            using var provider = services.AddTimeServices().BuildServiceProvider();

            Assert.Same(TimeProvider.System, provider.GetRequiredService<TimeProvider>());
        }

        [Fact]
        public void AddTimeServices_Should_Preserve_Existing_Provider()
        {
            var expected = new FixedTimeProvider(new DateTimeOffset(2026, 8, 25, 12, 0, 0, TimeSpan.Zero));
            var services = new ServiceCollection();
            services.AddSingleton<TimeProvider>(expected);

            using var provider = services.AddTimeServices().BuildServiceProvider();

            Assert.Same(expected, provider.GetRequiredService<TimeProvider>());
        }

        [Fact]
        public void GetCurrentInstant_Should_Use_The_Injected_Provider()
        {
            var value = new DateTimeOffset(2026, 8, 25, 12, 0, 0, TimeSpan.Zero).AddTicks(7);
            var expected = value.AddTicks(-7);
            TimeProvider provider = new FixedTimeProvider(value);

            var instant = provider.GetCurrentInstant();
            DateTime utcDateTime = provider.GetCurrentUtcDateTime();

            Assert.Equal(expected, instant.ToDateTimeOffset());
            Assert.Equal(expected.UtcDateTime, utcDateTime);
        }

        private sealed class FixedTimeProvider(DateTimeOffset value) : TimeProvider
        {
            public override DateTimeOffset GetUtcNow()
            {
                return value;
            }
        }
    }
}
