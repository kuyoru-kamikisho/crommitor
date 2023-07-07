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
            Hellow();
        }

        public void Hellow()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n Welcome to Crommitor 1.0.1!");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(" If you just want to generate changelog silently, please use \"-s\" parameter.");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(" Now you can enter a number to select the function you need or enter \"q\" to exit:\n");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(" 1. Add all changed files to the cache and create a commit message");
            Console.WriteLine(" 2. Create a commit message only");
            Console.WriteLine(" 3. Generate a changelog file");
            Console.ResetColor();

            Console.Write("Input the number:");
            var keyInfo = Console.ReadKey();
            switch (keyInfo.KeyChar)
            {
                case 'q':
                    return;
                case '1':
                    Methods.GitCommitAll(_config.committypes);
                    break;
                case '2':
                    Methods.GitCommitAll(_config.committypes, true);
                    break;
                case '3':
                    GenerateChangelog();
                    break;
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
            }
        }
    }
}