namespace Cli
{
    class Program
    {
        const string USAGE = @"Usage: cli command/query -<Namespace> -<Name> -<Model Response>";

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                ShowHelp();
                return;
            }

            string command = args[0].ToLower();

            switch (command)
            {
                case "command":
                    HandleCreateCommand(args);
                    break;
                case "query":
                    HandleCreateQuery(args);
                    break;
                case "help":
                case "--help":
                case "-h":
                    ShowHelp();
                    break;

                default:
                    Console.WriteLine($"Unknown command: {command}");
                    ShowHelp();
                    break;
            }
        }

        static void HandleCreateCommand(string[] args)
        {
            if (args.Length < 4)
            {
                Console.WriteLine(USAGE);
                return;
            }

            string @namespace = args[1];
            string name = args[2];
            string model = args[3];

            string path = Directory.GetCurrentDirectory();

            string[] fileNames = ["Command", "Validator", "Handler"];

            string pathFiles = Path.Combine(path, @namespace);

            if (!Directory.Exists(pathFiles))
                Directory.CreateDirectory(pathFiles);

            pathFiles = Path.Combine(pathFiles, "Commands");

            if (!Directory.Exists(pathFiles))
                Directory.CreateDirectory(pathFiles);

            foreach (string key in fileNames)
            {
                string content = File.ReadAllText(Path.Combine(path, $"Templates/Command/{key}.txt"));

                string filename = $"{name}";

                if (key != "Command")
                    filename += $"Command{key}";
                else
                    filename += key;

                filename += ".cs";

                File.WriteAllText(Path.Combine(pathFiles, filename), ReplaceContent(content, @namespace, name, model));
            }
        }

        static void HandleCreateQuery(string[] args)
        {
            if (args.Length < 4)
            {
                Console.WriteLine(USAGE);
                return;
            }

            string @namespace = args[1];
            string name = args[2];
            string model = args[3];

            string path = Directory.GetCurrentDirectory();

            string[] fileNames = ["Query", "Validator", "Handler"];

            string pathFiles = Path.Combine(path, @namespace);

            if (!Directory.Exists(pathFiles))
                Directory.CreateDirectory(pathFiles);

            pathFiles = Path.Combine(pathFiles, "Queries");

            if (!Directory.Exists(pathFiles))
                Directory.CreateDirectory(pathFiles);

            foreach (string key in fileNames)
            {
                string content = File.ReadAllText(Path.Combine(path, $"Templates/Query/{key}.txt"));

                string filename = $"{name}";

                if (key != "Query")
                    filename += $"Query{key}";
                else
                    filename += key;

                filename += ".cs";

                File.WriteAllText(Path.Combine(pathFiles, filename), ReplaceContent(content, @namespace, name, model));
            }
        }

        static string ReplaceContent(string content, string @namespace, string name, string model)
        {
            return content.Replace(@"{{Namespace}}", @namespace)
                    .Replace(@"{{Name}}", name)
                    .Replace(@"{{ResponseModel}}", model);
        }

        static void ShowHelp() => Console.WriteLine(USAGE);
    }
}
