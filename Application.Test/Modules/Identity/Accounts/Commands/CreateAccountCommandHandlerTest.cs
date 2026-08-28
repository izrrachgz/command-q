using SqlKata.Execution;
using Common.Enumerations;
using Persistence.Repository;
using Domain.Modules.Identity;
using Localization.Abstractions;
using Microsoft.Extensions.Logging.Abstractions;
using Application.Modules.Identity.Accounts.Commands;

namespace Application.Test.Modules.Identity.Accounts.Commands
{
    public sealed class CreateAccountCommandHandlerTest
    {
        private const string ExistingEmail = "admin@local.com";
        private const string ExistingUsername = "TurboDecoder";
        private const string IdentityIssuer = "https://localhost:7125/";
        private const string IdentitySubject = "identity-subject";
        private const string LinkedIdentitySubject = "different-subject";
        private const string OtherEmail = "other@local.com";

        [Fact]
        public async Task Handle_Links_Existing_Unmapped_Account()
        {
            using var database = await TestAccountDatabase.CreateAsync();
            var accountId = Guid.NewGuid();
            await database.InsertAccountAsync(accountId, ExistingUsername, ExistingEmail);
            var handler = database.CreateHandler();

            var response = await handler.Handle(CreateCommand(), CancellationToken.None);

            Assert.True(response.Success);
            Assert.True(await database.IdentityExistsAsync(accountId, IdentityIssuer, IdentitySubject));
        }

        [Fact]
        public async Task Handle_Rejects_Account_Linked_To_Another_Identity()
        {
            using var database = await TestAccountDatabase.CreateAsync();
            var accountId = Guid.NewGuid();
            await database.InsertAccountAsync(accountId, ExistingUsername, ExistingEmail);
            await database.InsertIdentityAsync(accountId, IdentityIssuer, LinkedIdentitySubject);
            var handler = database.CreateHandler();

            var response = await handler.Handle(CreateCommand(), CancellationToken.None);

            Assert.False(response.Success);
            Assert.False(await database.IdentityExistsAsync(accountId, IdentityIssuer, IdentitySubject));
        }

        [Fact]
        public async Task Handle_Rejects_When_Only_Username_Matches()
        {
            using var database = await TestAccountDatabase.CreateAsync();
            var accountId = Guid.NewGuid();
            await database.InsertAccountAsync(accountId, ExistingUsername, OtherEmail);
            var handler = database.CreateHandler();

            var response = await handler.Handle(CreateCommand(), CancellationToken.None);

            Assert.False(response.Success);
            Assert.False(await database.IdentityExistsAsync(accountId, IdentityIssuer, IdentitySubject));
        }

        private static CreateAccountCommand CreateCommand()
        {
            return new()
            {
                Email = ExistingEmail,
                Username = ExistingUsername,
                IdentityIssuer = IdentityIssuer,
                IdentitySubject = IdentitySubject
            };
        }

        private sealed class TestAccountDatabase : IDisposable
        {
            private readonly string path;
            private readonly WritableRepository repository;

            private TestAccountDatabase(string path)
            {
                this.path = path;
                repository = new WritableRepository($"Data Source={path};Version=3;", DatabaseEngine.SqLite, NullLogger<WritableRepository>.Instance);
            }

            public static async Task<TestAccountDatabase> CreateAsync()
            {
                var path = Path.Combine(Path.GetTempPath(), $"command-q-account-{Guid.NewGuid():N}.sqlite");
                var database = new TestAccountDatabase(path);
                await database.CreateSchemaAsync();
                return database;
            }

            public CreateAccountCommandHandler CreateHandler()
            {
                return new(
                    TestTextProvider.Instance,
                    repository,
                    TimeProvider.System,
                    NullLogger<CreateAccountCommandHandler>.Instance);
            }

            public async Task InsertAccountAsync(Guid accountId, string username, string email)
            {
                var now = new DateTime(2026, 8, 25, 18, 0, 0, DateTimeKind.Utc);
                await repository.Db.Query(nameof(Account))
                    .InsertAsync(new
                    {
                        Id = accountId,
                        Username = username,
                        Email = email,
                        CreatedUtc = now,
                        ModifiedUtc = now
                    });
            }

            public async Task InsertIdentityAsync(Guid accountId, string issuer, string subject)
            {
                var now = new DateTime(2026, 8, 25, 18, 0, 0, DateTimeKind.Utc);
                await repository.Db.Query(nameof(AccountIdentity))
                    .InsertAsync(new
                    {
                        AccountId = accountId,
                        Issuer = issuer,
                        Subject = subject,
                        CreatedUtc = now,
                        ModifiedUtc = now
                    });
            }

            public async Task<bool> IdentityExistsAsync(Guid accountId, string issuer, string subject)
            {
                return await repository.Db.Query(nameof(AccountIdentity))
                    .WhereNull(nameof(AccountIdentity.DeletedUtc))
                    .Where(nameof(AccountIdentity.AccountId), accountId)
                    .Where(nameof(AccountIdentity.Issuer), issuer)
                    .Where(nameof(AccountIdentity.Subject), subject)
                    .ExistsAsync();
            }

            public void Dispose()
            {
                repository.Dispose();
                File.Delete(path);
            }

            private async Task CreateSchemaAsync()
            {
                repository.Db.Connection.Open();

                try
                {
                    using var command = repository.Db.Connection.CreateCommand();
                    command.CommandText = """
                        CREATE TABLE Account (
                            Id BLOB NOT NULL PRIMARY KEY,
                            Username TEXT NOT NULL,
                            Email TEXT NOT NULL,
                            CreatedUtc TEXT NOT NULL,
                            ModifiedUtc TEXT NOT NULL,
                            DeletedUtc TEXT NULL
                        );

                        CREATE UNIQUE INDEX UX_Account_Username
                        ON Account (Username)
                        WHERE DeletedUtc IS NULL;

                        CREATE UNIQUE INDEX UX_Account_Email
                        ON Account (Email)
                        WHERE DeletedUtc IS NULL;

                        CREATE TABLE AccountIdentity (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            AccountId BLOB NOT NULL,
                            Issuer TEXT NOT NULL,
                            Subject TEXT NOT NULL,
                            CreatedUtc TEXT NOT NULL,
                            ModifiedUtc TEXT NOT NULL,
                            DeletedUtc TEXT NULL
                        );

                        CREATE UNIQUE INDEX UX_AccountIdentity_Issuer_Subject
                        ON AccountIdentity (Issuer, Subject)
                        WHERE DeletedUtc IS NULL;
                        """;
                    await Task.Run(command.ExecuteNonQuery);
                }
                finally
                {
                    repository.Db.Connection.Close();
                }
            }
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
