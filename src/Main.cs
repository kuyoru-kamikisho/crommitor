using System.Text;
using commitor.pears;
using YamlDotNet.Serialization;
using commitor.src.pears;

namespace commitor
{
    class Program
    {
        private static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            string? configpath = null;
            var silent = false;

            for (var i = 0; i < args.Length; i++)
            {
                if (args[i] == "-c" && i + 1 < args.Length)
                {
                    configpath = args[i + 1];
                    break;
                }

                if (args[i] == "-s")
                {
                    silent = true;
                }
            }

            if (configpath == null)
            {
                Console.WriteLine("Usage: csogrc.exe -c [config_path]");
                configpath = "./csogrc.yml";
            }

            if (!File.Exists(configpath))
            {
                Console.WriteLine($"Error: configFile '{configpath}' not found.");
                Console.ReadKey();
                return;
            }

            try
            {
                var repf = File.ReadAllText("./resources/repository.yml", Encoding.UTF8);
                var cfgf = File.ReadAllText(configpath, Encoding.UTF8);
                var deserializer = new DeserializerBuilder().Build();
                var config = deserializer.Deserialize<ConfigType>(cfgf);
                var repos = deserializer.Deserialize<Repositories>(repf);

                Directory.SetCurrentDirectory(config.projpath);

                var cri = new ConsoleRider(config, repos.platform);

                if (silent)
                {
                    cri.GenerateChangelog();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }
}