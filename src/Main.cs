using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using commitor.src.pears;
using commitor.src.utils;

namespace commitor
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            string configpath = null;
            var silent = false;

            for (int i = 0; i < args.Length; i++)
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
                return;
            }

            if (!File.Exists(configpath))
            {
                Console.WriteLine($"Error: configFile '{configpath}' not found.");
                Console.ReadKey();
                return;
            }

            try
            {
                var regexYaml = new Regex(@"\.(yml|yaml)$", RegexOptions.IgnoreCase);

                var repf = File.ReadAllText("./resources/repository.yml", Encoding.UTF8);
                var cfgf = File.ReadAllText(configpath, Encoding.UTF8);
                var deserializer = new DeserializerBuilder().Build();
                var config = deserializer.Deserialize<ConfigType>(cfgf);
                var repos = deserializer.Deserialize<List<Repository>>(repf);

                Directory.SetCurrentDirectory(config.projpath);

                var cri = new ConsoleRider(config, repos);

                if(silent)
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
