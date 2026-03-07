using Newtonsoft.Json;
using ScriptParser.Parser;
using BuildTool.Converters;

class Program
{
    static void Main(string[] args)
    {
        var scriptsDir = Path.Combine("..", "Scripts");
        //var outputDir = "Build";

        var outputDir = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "GameEngine", "Content", "Events"
        ));

        Directory.CreateDirectory(outputDir);

        foreach (var file in Directory.GetFiles(scriptsDir, "*.txt"))
        {
            var lines = File.ReadAllLines(file);

            var parser = new ScriptParser.Parser.ScriptParser();
            parser.EventInfo.EventID = Path.GetFileNameWithoutExtension(file);
            parser.Parse(lines);

            var json = JsonConvert.SerializeObject(
                new
                {
                    Event = parser.EventInfo,
                    Scenes = parser.Scenes
                    .Select(kv => new
                    {
                        Label = kv.Value.Label,
                        Nodes = kv.Value.Nodes
                            .Select(NodeConverter.ToRuntime)
                            .ToList()
                    })
                    .ToList()


                },
                Formatting.Indented
            );

            var outPath = Path.Combine(
                outputDir,
                Path.GetFileNameWithoutExtension(file) + ".json"
            );

            File.WriteAllText(outPath, json);

            Console.WriteLine("Build OK: " + outPath);
        }
    }
}