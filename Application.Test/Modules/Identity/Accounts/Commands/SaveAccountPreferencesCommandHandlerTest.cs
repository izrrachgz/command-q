using System.Data;
using SqlKata.Execution;
using Common.Enumerations;
using Persistence.Repository;
using Domain.Modules.Identity;
using Localization.Abstractions;
using Microsoft.Extensions.Logging.Abstractions;
using Application.Modules.Identity.AccountActivity.Commands;
using Application.Modules.Identity.AccountActivity.Services;
using Application.Modules.Identity.Accounts.Preferences.Commands;

namespace Application.Test.Modules.Identity.Accounts.Commands
{
    public sealed class SaveAccountPreferencesCommandHandlerTest
    {
        private static readonly DateTimeOffset Now = new(2026, 8, 20, 18, 30, 0, TimeSpan.Zero);

        [Fact]
        public async Task Handle_Should_Save_Preferences_And_Activity_In_One_Transaction()
        {
            using var database = await TestDatabase.Create();
            var handler = database.CreateHandler(database.CreateRecorder());
            var accountId = Guid.NewGuid();

            var response = await handler.Handle(CreateCommand(accountId), TestContext.Current.CancellationToken);

            Assert.True(response.Success);
            Assert.Equal(1, await database.Count(nameof(AccountPreference)));
            Assert.Equal(1, await database.Count(nameof(AccountActivity)));
        }

        [Fact]
        public async Task Handle_Should_Roll_Back_Preferences_When_Activity_Fails()
        {
            using var database = await TestDatabase.Create();
            var handler = database.CreateHandler(new FailingActivityRecorder());

            var response = await handler.Handle(CreateCommand(Guid.NewGuid()), TestContext.Current.CancellationToken);

            Assert.False(response.Success);
            Assert.Equal(0, await database.Count(nameof(AccountPreference)));
        }

        [Fact]
        public async Task Handle_Should_Update_The_Existing_Active_Row()
        {
            using var database = await TestDatabase.Create();
            var handler = database.CreateHandler(database.CreateRecorder());
            var accountId = Guid.NewGuid();
            await handler.Handle(CreateCommand(accountId), TestContext.Current.CancellationToken);

            var response = await handler.Handle(
                CreateCommand(accountId) with { Theme = AccountTheme.Light },
                TestContext.Current.CancellationToken);

            Assert.True(response.Success);
            Assert.Equal(1, await database.Count(nameof(AccountPreference)));
            Assert.Equal(2, await database.Count(nameof(AccountActivity)));
        }

        private static SaveAccountPreferencesCommand CreateCommand(Guid accountId)
        {
            return new()
            {
                AccountId = accountId,
                Culture = "en-US",
                Theme = AccountTheme.Dark,
                TimeZone = "America/Hermosillo"
            };
        }

        private sealed class TestDatabase : IDisposable
        {
            private readonly string path;
            private readonly WritableRepository repository;
            private readonly TimeProvider timeProvider = new FixedTimeProvider(Now);

            private TestDatabase(string path)
            {
                this.path = path;
                repository = new WritableRepository(
                    $"Data Source={path};Version=3;",
                    DatabaseEngine.SqLite,
                    NullLogger<WritableRepository>.Instance);
            }

            public static async Task<TestDatabase> Create()
            {
                var database = new TestDatabase(Path.Combine(Path.GetTempPath(), $"commandq-preferences-{Guid.NewGuid():N}.sqlite"));
                await database.CreateSchema();
                return database;
            }

            public SaveAccountPreferencesCommandHandler CreateHandler(IAccountActivityRecorder recorder)
            {
                return new(repository, recorder, timeProvider, TestTextProvider.Instance);
            }

            public IAccountActivityRecorder CreateRecorder()
            {
                return new AccountActivityRecorder(repository, timeProvider, NullLogger<AccountActivityRecorder>.Instance);
            }

            public async Task<long> Count(string table)
            {
                return await repository.Db.Query(table).CountAsync<long>();
            }

            public void Dispose()
            {
                repository.Dispose();
                File.Delete(path);
            }

            private async Task CreateSchema()
            {
                repository.Db.Connection.Open();

                try
                {
                    using var command = repository.Db.Connection.CreateCommand();
                    command.CommandText = """
                        CREATE TABLE AccountPreference (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            AccountId BLOB NOT NULL,
                            Culture TEXT NOT NULL,
                            Theme INTEGER NOT NULL,
                            CompactNavigation INTEGER NOT NULL,
                            EnhancedFocus INTEGER NOT NULL,
                            ReduceMotion INTEGER NOT NULL,
                            ShowDenseTables INTEGER NOT NULL,
                            TimeZone TEXT NOT NULL,
                            CreatedUtc TEXT NOT NULL,
                            ModifiedUtc TEXT NOT NULL,
                            DeletedUtc TEXT NULL
                        );
                        CREATE UNIQUE INDEX UX_AccountPreference_AccountId
                        ON AccountPreference (AccountId)
                        WHERE DeletedUtc IS NULL;

                        CREATE TABLE AccountActivity (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            AccountId BLOB NOT NULL,
                            Module TEXT NOT NULL,
                            Action TEXT NOT NULL,
                            EntityType TEXT NULL,
                            EntityId TEXT NULL,
                            Message TEXT NOT NULL,
                            MetadataJson TEXT NULL,
                            IpAddress TEXT NULL,
                            UserAgent TEXT NULL,
                            CreatedUtc TEXT NOT NULL,
                            ModifiedUtc TEXT NOT NULL,
                            DeletedUtc TEXT NULL
                        );
                        """;
                    await Task.Run(command.ExecuteNonQuery);
                }
                finally
                {
                    repository.Db.Connection.Close();
                }
            }
        }

        private sealed class FailingActivityRecorder : IAccountActivityRecorder
        {
            public Task<bool> RecordAsync(
                RecordAccountActivityCommand activity,
                IDbTransaction? transaction = null,
                CancellationToken cancellationToken = default)
            {
                return Task.FromResult(false);
            }
        }

        private sealed class FixedTimeProvider(DateTimeOffset value) : TimeProvider
        {
            public override DateTimeOffset GetUtcNow() => value;
        }

        private sealed class TestTextProvider : ITextProvider
        {
            public static TestTextProvider Instance { get; } = new();

            public string Get(string key, params object[] args)
            {
                return args.Length == 0 ? key : string.Format(key, args);
            }
        }
    }
}
