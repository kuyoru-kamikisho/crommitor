using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace MyNamespace
{
    // 定义 csogrc.yml 对应的类
    public class CsogrcConfig
    {
        public string projpath { get; set; }
        public string branch { get; set; }
        public string title { get; set; }
        public string[] committypes { get; set; }
        public string[] onlyusers { get; set; }
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
        public static string BuildContent(string commits,CsogrcConfig config,string remoteaddr)
        {
            bool canbewriten = false;
            string[] lines = commits.Split("\n");

            

            foreach (string line in lines)
            {
                string tag_ptn = @"commit\s.*\(";
                string aor_ptn = @"Author:\s.*<";
                string dte_ptn = @"Date:\s";
                Match tag_mtc = Regex.Match(line, tag_ptn);
                Console.WriteLine(line);
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
                var yaml = File.ReadAllText(configpath,Encoding.UTF8);
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
                    Arguments = $"tag",
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

                header= BuildHeader(config);
                content= BuildContent(gitlogstr,config,gitrepoaddress);

                
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
