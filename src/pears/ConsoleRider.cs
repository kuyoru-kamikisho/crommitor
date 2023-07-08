using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using commitor.src.pears;
using commitor.utils;

namespace commitor.pears
{
    public class ConsoleRider
    {
        ConfigType _config;
        List<RepositoryInfo> _repos;

        public ConsoleRider(ConfigType config, List<RepositoryInfo> repos)
        {
            _config = config;
            _repos = repos;
        }

        public void Hellow()
        {
            string[] options =
            {
                "Add all changed files to the cache and create a commit message",
                "Create a commit message only",
                "Generate a changelog file",
                "Exit"
            };
            var chooseN = 0;
            while (true)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\nWelcome to Crommitor {App.Version}!");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("If you just want to generate changelog silently, please use \"-s\" parameter.");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Now you can use the arrow keys on the keyboard" +
                                  " to select the following functions:\n");
                Console.ResetColor();
                for (int i = 0; i < options.Length; i++)
                {
                    if (i == chooseN)
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine($"{i + 1}. {options[i]} ←");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"{i + 1}. {options[i]}");
                    }
                }

                var keyInfo = Console.ReadKey(true);
                switch (keyInfo.Key)
                {
                    case ConsoleKey.UpArrow:
                        chooseN = (chooseN - 1 + options.Length) % options.Length;
                        break;
                    case ConsoleKey.DownArrow:
                        chooseN = (chooseN + 1) % options.Length;
                        break;
                    case ConsoleKey.Enter:
                        Console.Clear();
                        switch (chooseN)
                        {
                            case 0:
                                Methods.GitCommitAll(_config.committypes);
                                break;
                            case 1:
                                Methods.GitCommitAll(_config.committypes, true);
                                break;
                            case 2:
                                GenerateChangelog();
                                break;
                        }

                        return;
                }
            }
        }

        public void GenerateChangelog()
        {
            var gitrepoaddresspro = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "remote -v",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var gitlogpro = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = $"log --decorate {_config.branch}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            string header;
            string content;
            string gitlogstr;
            string gitrepoaddress;
            using (var process = new Process())
            {
                process.StartInfo = gitrepoaddresspro;
                process.Start();
                var pattern = @"http.*git";
                gitrepoaddress = process.StandardOutput.ReadToEnd().Split("\n")[0];

                var match = Regex.Match(gitrepoaddress, pattern);
                if (match.Success)
                {
                    gitrepoaddress = match.Value.Replace(".git", "");
                }

                process.WaitForExit();
                process.Close();

                process.StartInfo = gitlogpro;
                process.Start();
                gitlogstr = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                process.Close();
            }

            header = Methods.BuildHeader(_config);
            content = Methods.BuildContent(this._repos, gitlogstr, _config, gitrepoaddress);


            using (StreamWriter writer = new StreamWriter(_config.outputdir, false, Encoding.UTF8))
            {
                writer.Write(header + content);
                Console.WriteLine("Finished.");
            }
        }
    }
}