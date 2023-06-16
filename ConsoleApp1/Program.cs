using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace MyNamespace
{
    // 定义 csogrc.yml 对应的类
    public class CommitType
    {
        public string value { get; set; }
        public string description { get; set; }
    }
    public class Platform
    {
        public string name { get; set; }
        public string tagbri { get; set; }
        public string commitbri { get; set; }
        public string issuebri { get; set; }
    }
    public class CsogrcConfig
    {
        public string useplatform { get; set; }
        public Platform[] platform { get; set; }
        public string projpath { get; set; }
        public string branch { get; set; }
        public string title { get; set; }
        public CommitType[] committypes { get; set; }
        public CommitType[] onlyusers { get; set; }
        public string outputdir { get; set; }
        public string description { get; set; }

    }

    class Program
    {
        public static string DateFormater(string dateString)
        {
            DateTime date;
            if (DateTime.TryParseExact(dateString, "ddd MMM d HH:mm:ss yyyy zzz",
                                       System.Globalization.CultureInfo.InvariantCulture,
                                       System.Globalization.DateTimeStyles.None, out date))
            {
                return date.ToString("yyyy-MM-dd");
            }
            else
            {
                throw new ArgumentException("Invalid date string");
            }
        }

        public static string BuildHeader(CsogrcConfig config)
        {
            string header1 = $"# {config.title}\n\n{config.description}\n\n";
            return header1;
        }
        public static string BuildContent(string commits, CsogrcConfig config, string remoteaddr)
        {
            bool canbewriten = false;

            Dictionary<string, List<object>> stacker = new Dictionary<string, List<object>>();
            foreach (CommitType key in config.committypes)
            {
                stacker.Add(key.value, new List<object>());
            }

            string[] lines = commits.Split("\n");
            string com_ptn = @"commit\s.*";
            string tag_ptn = @"tag:\s.*\)";
            string aor_ptn = @"Author:\s.*<";
            string dte_ptn = @"Date:\s";
            string isu_ptn = @"\d+";
            foreach (string line in lines)
            {
                Match com_mtc = Regex.Match(line, com_ptn);
                if (com_mtc.Success)
                {
                    Match tag_mtc = Regex.Match(com_mtc.Value, tag_ptn);
                    if (tag_mtc.Success)
                    {

                    }
                }
                foreach (var item in config.committypes)
                {
                    if (line.StartsWith(item + ": "))
                    {
                        stacker[item.value].Add($"- { line } ([]())");
                        break;
                    }
                }
            }
            return "";
        }
        static void Main(string[] args)
        {

            Console.OutputEncoding = System.Text.Encoding.UTF8;

            string configpath = null;
            for (int i = 0; i < args.Length; i++)
            {
                Console.WriteLine(args[i]);
            }
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-c" && i + 1 < args.Length)
                {
                    configpath = args[i + 1];
                    break;
                }
            }

            if (configpath == null)
            {
                Console.WriteLine("Usage: csogrc.exe -c [config_path]");
                return;
            }

            if (!File.Exists(configpath))
            {
                Console.WriteLine($"Error: configFile '{configpath}' not found.");
                return;
            }

            try
            {
                Regex regexJson = new Regex(@"\.json$", RegexOptions.IgnoreCase);
                Regex regexYaml = new Regex(@"\.(yml|yaml)$", RegexOptions.IgnoreCase);

                var yaml = File.ReadAllText(configpath, Encoding.UTF8);
                var deserializer = new DeserializerBuilder().Build();
                var config = deserializer.Deserialize<CsogrcConfig>(yaml);
                Console.WriteLine(yaml);
                Console.WriteLine($"Name: {config.projpath}");
                // 访问其他属性
                Console.WriteLine($"进入目录：{config.projpath}");
                Directory.SetCurrentDirectory(config.projpath);
                Console.WriteLine($"Current directory: {Directory.GetCurrentDirectory()}");

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
                    Arguments = $"log {config.branch}",
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
                    Console.WriteLine($"{gitrepoaddress}");
                    Match match = Regex.Match(gitrepoaddress, pattern);
                    if (match.Success)
                    {
                        Console.WriteLine("匹配成功");
                        gitrepoaddress = match.Value.Replace(".git", "");
                    }
                    process.WaitForExit();
                    Console.WriteLine($"远程仓库：{gitrepoaddress}");

                    process.Close();

                    process.StartInfo = gittagpro;
                    process.Start();
                    string gittagstr = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    Console.WriteLine(gittagstr);
                    tags = gittagstr.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    Console.WriteLine("-------------------------------");
                    Console.WriteLine(tags[5]);

                    process.Close();

                    process.StartInfo = gitlogpro;
                    process.Start();
                    gitlogstr = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    process.Close();
                }

                header = BuildHeader(config);
                content = BuildContent(gitlogstr, config, gitrepoaddress);


                using (StreamWriter writer = new StreamWriter(config.outputdir, false, Encoding.UTF8))
                {
                    writer.Write(header);
                }

                Console.WriteLine("字符串已输出到文件：" + config.outputdir);
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

    }
}
