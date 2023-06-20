﻿using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace MyNamespace
{
    public class TypedCommits
    {
        public string Type { get; set; }
        public string TypeDescription { get; set; }
        public List<string> Values { get; set; }
    }
    public class GroupStrs
    {
        public string TagName { get; set; }
        public List<TypedCommits> GroupedCommits { get; set; }
    }
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
            int tag_idx = -1;
            List<GroupStrs> tag_groups = new List<GroupStrs>();
            string[] lines = commits.Split("\n");
            string com_ptn = @"^commit\s.*";
            string tag_ptn = @"tag:\s.*\)";
            string tag_get = @"tag:\s([^,\n\)]+)";
            string aor_ptn = @"Author:\s+(\w+\.\w+)";
            string dte_ptn = @"Date:\s([^\n]+)";
            string isu_ptn = @"#\d+";
            string msg_ptn = @"\s{4}^(.*)";
            string cmt_ref = "";
            string cmt_fnk = "";
            string aor_ref = "";
            string dte_ref = "";

            foreach (string line in lines)
            {

                // 匹配commit信息（tag）
                Match com_mtc = Regex.Match(line, com_ptn);
                if (com_mtc.Success)
                {
                    cmt_ref = com_mtc.Value.Substring(7, 7);
                    cmt_fnk = com_mtc.Value.Split(" ")[1];
                    Match tag_mtc = Regex.Match(com_mtc.Value, tag_ptn);
                    if (tag_mtc.Success)
                    {
                        Match tag_mtg = Regex.Match(tag_mtc.Value, tag_get);
                        if (tag_mtg.Success)
                        {
                            tag_groups.Add(new GroupStrs
                            {
                                TagName = tag_mtg.Groups[1].Value,
                                GroupedCommits = new List<TypedCommits>()
                            });
                            tag_idx += 1;
                            continue;
                        }
                    }
                }

                // 匹配作者
                Match aot_mtc = Regex.Match(line, aor_ptn);
                if (aot_mtc.Success)
                {
                    aor_ref = aot_mtc.Groups[1].Value;

                    if (config.onlyusers.Any(o => o.value == aor_ref))
                    {
                        canbewriten = true;
                    }
                    else
                    {
                        canbewriten = false;
                    }
                    continue;
                }

                // 匹配日期
                Match dte_mtc = Regex.Match(line, dte_ptn);
                if (dte_mtc.Success)
                {
                    dte_ref = DateFormater(dte_mtc.Value.Substring(8));
                    continue;
                }

                // 匹配提交信息
                if (tag_idx > -1)
                {
                    foreach (CommitType cmt in config.committypes)
                    {
                        if (line.Trim().StartsWith(cmt.value))
                        {
                            if (tag_groups.Count > tag_idx)
                            {
                                List<TypedCommits> now_types = tag_groups[tag_idx].GroupedCommits;
                                int i = now_types.FindIndex(o => o.Type == cmt.value);
                                if (i == -1)
                                {
                                    now_types.Add(new TypedCommits
                                    {
                                        Type = cmt.value,
                                        TypeDescription = cmt.description,
                                        Values = new List<string> { line }
                                    });
                                }
                                else
                                {
                                    now_types[i].Values.Add(line);
                                }
                            }
                            break;
                        }
                    }
                }
            }

            // 构建文本内容
            string content = "";
            for (int i = 0; i < tag_groups.Count; i++)
            {
                content = content 
                    + tag_groups[i].TagName
                    + $" ([{dte_ref}]())";
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
