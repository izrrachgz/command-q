using SqlKata;
using SqlKata.Compilers;
using Persistence.Repository.Extensions;

namespace Application.Test.Modules.Persistence.Extensions
{
    public sealed class QuerySearchAndSortExtensionsTest
    {
        private static readonly SqlServerCompiler Compiler = new();

        [Fact]
        public void ApplySearchTerm_Should_Group_Like_Conditions_And_Trim_The_Term()
        {
            var query = new Query("Record")
                .Where("AccountId", 42)
                .ApplySearchTerm("  sample  ", ["Name", "Description"]);

            var result = Compiler.Compile(query);

            Assert.Contains("WHERE [AccountId] = @p0 AND (", result.Sql, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("[Name]", result.Sql, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(" OR ", result.Sql, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("[Description]", result.Sql, StringComparison.OrdinalIgnoreCase);
            Assert.Equal([42, "%sample%", "%sample%"], result.Bindings);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ApplySearchTerm_Should_Not_Add_A_Predicate_For_An_Empty_Term(string? searchTerm)
        {
            var query = new Query("Record").ApplySearchTerm(searchTerm, ["Name"]);

            var result = Compiler.Compile(query);

            Assert.DoesNotContain("WHERE", result.Sql, StringComparison.OrdinalIgnoreCase);
            Assert.Empty(result.Bindings);
        }

        [Fact]
        public void ApplySorting_Should_Use_The_Mapped_Column_And_Requested_Direction()
        {
            IReadOnlyDictionary<string, string> columns = new Dictionary<string, string>
            {
                ["name"] = "DisplayName"
            };
            var query = new Query("Record")
                .ApplySorting("name", ascendingSort: false, columns, defaultColumn: "CreatedUtc");

            var result = Compiler.Compile(query);

            Assert.Contains("ORDER BY [DisplayName] DESC", result.Sql, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ApplySorting_Should_Use_The_Configured_Default_When_Sort_Is_Missing()
        {
            IReadOnlyDictionary<string, string> columns = new Dictionary<string, string>();
            var query = new Query("Record")
                .ApplySorting(
                    sortBy: null,
                    ascendingSort: true,
                    columns,
                    defaultColumn: "CreatedUtc",
                    defaultAscending: false);

            var result = Compiler.Compile(query);

            Assert.Contains("ORDER BY [CreatedUtc] DESC", result.Sql, StringComparison.OrdinalIgnoreCase);
        }
    }
}
