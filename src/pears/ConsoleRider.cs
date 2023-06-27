using commitor.src.utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace commitor.src.pears
{
    public class ConsoleRider
    {
        ConfigType config;
        List<Repository> repos;

        public ConsoleRider(ConfigType config,List<Repository> repos)
        {
            this.config=config;
            this.repos = repos;
        }

        public void Hellow()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n Welcome to Crommitor 1.0.1!");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(" If you just want to generate changelog, please use \"-s\" parameter.");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(" Now you can enter a number to select the function you need or enter \"q\" to exit:\n");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(" 1. Add all changed files to the cache and create a commit message");
            Console.WriteLine(" 2. Create a commit message only");
            Console.WriteLine(" 3. Generate a changelog file");
            Console.ResetColor();
            Console.ReadKey();
        }
        public void GenerateChangelog()
        {
            ProcessStartInfo gittagpro = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = $"tag --sort=-creatordate",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            ProcessStartInfo gitrepoaddresspro = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = $"remote -v",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            ProcessStartInfo gitlogpro = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = $"log --decorate {config.branch}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            string header = "";
            string content = "";
            string gitlogstr = "";
            string[] tags = { };
            string gitrepoaddress = "";
            using (Process process = new Process())
            {
                process.StartInfo = gitrepoaddresspro;
                process.Start();
                string pattern = @"http.*git";
                gitrepoaddress = process.StandardOutput.ReadToEnd().Split("\n")[0];

                Match match = Regex.Match(gitrepoaddress, pattern);
                if (match.Success)
                {
                    gitrepoaddress = match.Value.Replace(".git", "");
                }
                process.WaitForExit();
                process.Close();

                process.StartInfo = gittagpro;
                process.Start();
                string gittagstr = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                tags = gittagstr.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                process.Close();

                process.StartInfo = gitlogpro;
                process.Start();
                gitlogstr = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                process.Close();
            }

            header = Methods.BuildHeader(config);
            content = Methods.BuildContent(this.repos, gitlogstr, config, gitrepoaddress);


            using (StreamWriter writer = new StreamWriter(config.outputdir, false, Encoding.UTF8))
            {
                writer.Write(header + content);
            }
        }
    }
}
