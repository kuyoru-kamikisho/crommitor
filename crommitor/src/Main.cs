using System.Reflection;
using System.Text;
using comi.pears;
using comi.src.pears;
using YamlDotNet.Serialization;

namespace comi
{
    class Program
    {
        private static void Main(string[] args)
        {
            var location = Assembly.GetExecutingAssembly().Location;
            var directory = Path.GetDirectoryName(location);
            
            Console.OutputEncoding = Encoding.UTF8;

            string? configpath = null;
            var silent = false;

            for (var i = 0; i < args.Length; i++)
            {
                if (args[i] == "-c")
                {
                    if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                    {
                        configpath = args[i + 1];
                        i++;
                    }
                    else
                    {
                        Console.WriteLine("Argument '-c' with no path specified");
                    }
                }

                if (args[i] == "-s")
                {
                    silent = true;
                }

                if (args[i] == "-h" || args[i] == "--help")
                {
                    App.HelpMe();
                    return;
                }

                if (args[i] == "-v")
                {
                    Console.WriteLine(App.Version);
                    return;
                }
            }

            if (configpath == null)
            {
                Console.WriteLine("Usage: csogrc.exe -c [config_path]");
                configpath = "./csogrc.yml";
                if (directory != null)
                {
                    var filePath = Path.Combine(directory, configpath);
                    var fullPath = Path.GetFullPath(filePath);
                    configpath = fullPath;
                }
            }

            if (!File.Exists(configpath))
            {
                Console.WriteLine($"Error: configFile '{configpath}' not found.");
                return;
            }

            try
            {
                var filePath = Path.Combine(directory, "./resources/repository.yml");
                var fullPath = Path.GetFullPath(filePath);
                var repf = File.ReadAllText(fullPath, Encoding.UTF8);
                var cfgf = File.ReadAllText(configpath, Encoding.UTF8);
                var deserializer = new DeserializerBuilder().Build();
                var config = deserializer.Deserialize<ConfigType>(cfgf);
                var repos = deserializer.Deserialize<Repositories>(repf);

                Directory.SetCurrentDirectory(config.projpath);

                App.ConsoleEntry = new ConsoleRider(config, repos.platform);
                App.ConsoleEntry.Hellow();

                if (silent)
                {
                    App.ConsoleEntry.GenerateChangelog();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Unexpected Error: " + ex.Message);
                Console.WriteLine("You can report an issue in here: " + App.ReportIssue);
                Console.ResetColor();
            }
        }
    }
}