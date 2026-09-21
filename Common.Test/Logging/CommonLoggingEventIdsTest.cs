using System.Reflection;
using Common.Abstractions;

namespace Common.Test.Logging;

public sealed class CommonLoggingEventIdsTest
{
    [Fact]
    public void EventIds_Should_Be_Unique_And_In_The_Common_Package_Range()
    {
        Assembly assembly = typeof(ITaskRunner).Assembly;
        int[] ranges = GetConstants(assembly, "EventIdRanges").ToArray();
        int[] eventIds = GetConstants(assembly, "EventIds").ToArray();

        Assert.Equal([300000], ranges);
        Assert.NotEmpty(eventIds);
        Assert.Equal(eventIds.Length, eventIds.Distinct().Count());
        Assert.All(eventIds, eventId => Assert.InRange(eventId, 300000, 300999));
    }

    private static IEnumerable<int> GetConstants(Assembly assembly, string typeSuffix) => assembly
        .GetTypes()
        .Where(type => type.Name.EndsWith(typeSuffix, StringComparison.Ordinal))
        .SelectMany(type => type
            .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
            .Where(field => field.IsLiteral && field.FieldType == typeof(int))
            .Select(field => (int)field.GetRawConstantValue()!));
}
