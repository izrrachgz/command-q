using System.Xml.Linq;

namespace Common.Test.Architecture;

public sealed class SecurityAndPackagingConventionsTest
{
    private static readonly string[] LibraryProjects = ["Common", "Cqrs", "Localization", "Persistence", "Time"];

    [Fact]
    public void Operational_Log_Templates_Should_Not_Contain_Consumer_Controlled_Payloads()
    {
        string repositoryRoot = FindRepositoryRoot();
        string[] forbiddenPlaceholders = ["{CacheKey}", "{ConnectionString}", "{ParameterName}", "{Query}", "{Token}"];
        string[] violations = LibraryProjects
            .SelectMany(project => Directory.EnumerateFiles(
                Path.Combine(repositoryRoot, project),
                "*Log.cs",
                SearchOption.AllDirectories))
            .SelectMany(path => File.ReadLines(path)
                .Select((line, index) => new SourceLine(path, index + 1, line)))
            .Where(source => forbiddenPlaceholders.Any(placeholder =>
                source.Text.Contains(placeholder, StringComparison.OrdinalIgnoreCase)))
            .Select(source => $"{Path.GetRelativePath(repositoryRoot, source.Path)}:{source.Number}: {source.Text.Trim()}")
            .Order()
            .ToArray();

        Assert.Empty(violations);
    }

    [Fact]
    public void Reusable_Libraries_Should_Not_Write_Directly_To_The_Process_Console()
    {
        string repositoryRoot = FindRepositoryRoot();
        string[] violations = LibraryProjects
            .SelectMany(project => Directory.EnumerateFiles(
                Path.Combine(repositoryRoot, project),
                "*.cs",
                SearchOption.AllDirectories))
            .Where(path => !IsGeneratedOutput(path))
            .SelectMany(path => File.ReadLines(path)
                .Select((line, index) => new SourceLine(path, index + 1, line)))
            .Where(source => source.Text.Contains("Console.", StringComparison.Ordinal))
            .Select(source => $"{Path.GetRelativePath(repositoryRoot, source.Path)}:{source.Number}: {source.Text.Trim()}")
            .Order()
            .ToArray();

        Assert.Empty(violations);
    }

    [Fact]
    public void Package_Build_Should_Produce_Traceable_Unobfuscated_Symbol_Packages()
    {
        string repositoryRoot = FindRepositoryRoot();
        string propsPath = Path.Combine(repositoryRoot, "Directory.Build.props");
        XDocument props = XDocument.Load(propsPath);

        AssertProperty(props, "Deterministic", "true");
        AssertProperty(props, "PublishRepositoryUrl", "true");
        AssertProperty(props, "EmbedUntrackedSources", "true");
        AssertProperty(props, "IncludeSymbols", "true");
        AssertProperty(props, "SymbolPackageFormat", "snupkg");

        string[] buildInputs =
        [
            propsPath,
            Path.Combine(repositoryRoot, "scripts", "build.ps1"),
            .. LibraryProjects.Select(project => Path.Combine(repositoryRoot, project, $"{project}.csproj"))
        ];
        string combinedBuildInputs = string.Join(Environment.NewLine, buildInputs.Select(File.ReadAllText));
        string[] forbiddenObfuscators = ["obfuscar", "dotfuscator", "confuser", "obfuscate", "obfuscation"];

        Assert.All(forbiddenObfuscators, forbidden =>
            Assert.DoesNotContain(forbidden, combinedBuildInputs, StringComparison.OrdinalIgnoreCase));
    }

    private static void AssertProperty(XDocument document, string name, string expected)
    {
        string[] values = document.Descendants(name).Select(element => element.Value.Trim()).ToArray();
        Assert.Contains(expected, values, StringComparer.OrdinalIgnoreCase);
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "command-q.sln")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the command-q repository root.");
    }

    private static bool IsGeneratedOutput(string path) =>
        path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
        || path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal);

    private sealed record SourceLine(string Path, int Number, string Text);
}
