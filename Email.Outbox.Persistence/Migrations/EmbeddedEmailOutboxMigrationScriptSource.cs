using System.Text;
using System.Reflection;
using Common.Enumerations;
using System.Security.Cryptography;
using Email.Outbox.Persistence.Migrations.Models;

namespace Email.Outbox.Persistence.Migrations
{
    internal sealed class EmbeddedEmailOutboxMigrationScriptSource
        : IEmailOutboxMigrationScriptSource
    {
        private const string MigrationPrefix = "migration-";
        private const string SqlSuffix = ".sql";
        private readonly Assembly _assembly = typeof(EmailOutboxPersistenceAssembly).Assembly;

        public async Task<IReadOnlyList<EmailOutboxMigrationScript>> GetScriptsAsync(
            DatabaseEngine engine,
            CancellationToken cancellationToken = default)
        {
            string resourcePrefix = $"{_assembly.GetName().Name}.Migrations.{engine}.";
            var scripts = new List<EmailOutboxMigrationScript>();

            foreach (string resourceName in _assembly.GetManifestResourceNames()
                .Where(name => name.StartsWith(resourcePrefix, StringComparison.Ordinal))
                .Order(StringComparer.Ordinal))
            {
                cancellationToken.ThrowIfCancellationRequested();

                string fileName = resourceName[resourcePrefix.Length..];
                long migrationId = ParseMigrationId(resourceName, fileName);
                await using var stream = _assembly.GetManifestResourceStream(resourceName) ??
                    throw new InvalidOperationException($"Embedded email outbox migration '{resourceName}' could not be opened.");
                using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
                string content = await reader.ReadToEndAsync(cancellationToken);
                string checksum = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content)));

                scripts.Add(new EmailOutboxMigrationScript(
                    migrationId,
                    fileName,
                    content,
                    checksum));
            }

            return scripts.OrderBy(script => script.MigrationId).ToArray();
        }

        private static long ParseMigrationId(string resourceName, string fileName)
        {
            if (!fileName.StartsWith(MigrationPrefix, StringComparison.OrdinalIgnoreCase) ||
                !fileName.EndsWith(SqlSuffix, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Embedded email outbox migration '{resourceName}' does not follow migration-X.sql naming.");
            }

            string idText = fileName[MigrationPrefix.Length..^SqlSuffix.Length];
            return long.TryParse(idText, out long migrationId)
                ? migrationId
                : throw new InvalidOperationException($"Embedded email outbox migration '{resourceName}' has an invalid numeric ID.");
        }
    }
}
