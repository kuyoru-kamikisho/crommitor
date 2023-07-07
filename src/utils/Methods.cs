using System.Diagnostics;
using System.Text.RegularExpressions;
using commitor.pears;
using commitor.src.pears;

namespace commitor.utils
{
    public static class Methods
    {
        public static bool GitCommitAll(CommitType[] types, bool onlyCommit = false)
        {
            try
            {
                string msg;
                var selectedIndex = 0;
                var choosing = true;
                while (choosing)
                {
                    Console.Clear();
                    Console.WriteLine("Select a commit type below:");
                    for (var i = 0; i < types.Length; i++)
                    {
                        var t = types[i];
                        if (i == selectedIndex)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"\t{i + 1}. {t.value + ":",-12}{t.description}\u0332");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.WriteLine($"\t{i + 1}. {t.value + ":",-12}{t.description}");
                        }
                    }

                    // 监听用户按键
                    var keyInfo = Console.ReadKey(true);

                    // 根据按键进行相应操作
                    switch (keyInfo.Key)
                    {
                        case ConsoleKey.UpArrow:
                            selectedIndex = (selectedIndex - 1 + types.Length) % types.Length;
                            break;
                        case ConsoleKey.DownArrow:
                            selectedIndex = (selectedIndex + 1) % types.Length;
                            break;
                        case ConsoleKey.Enter:
                            Console.Clear();
                            choosing = false;
                            break;
                    }
                }

                var initStr = types[selectedIndex].value + ": ";
                var msgInput = string.Empty;
                while (string.IsNullOrWhiteSpace(msgInput))
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("Write a short commit message:");
                    Console.ResetColor();
                    Console.Write(initStr);
                    msgInput = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(msgInput)) continue;
                    Console.Clear();
                    Console.WriteLine("At least one piece of information is required, please re-enter.");
                }

                msg = initStr + msgInput;
                var ags = "-am";
                if (onlyCommit)
                {
                    ags = "-m";
                }

                var cmall = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"commit {ags} \"{msg}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                var process = new Process();
                process.StartInfo = cmall;
                process.Start();
                process.WaitForExit();
                process.Close();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public static string LinkBuilder(string url, string path)
        {
            var uri = new Uri(url);
            var newPath = "";
            if (uri.AbsolutePath.LastIndexOf("/", StringComparison.Ordinal) > 0)
            {
                newPath = string.Concat(
                    uri.AbsolutePath.AsSpan(0, uri.AbsolutePath.LastIndexOf("/", StringComparison.Ordinal)), "/");
            }

            newPath += path;
            return uri.Scheme + "://" + uri.Authority + newPath;
        }

        public static string ParseIssueAddress(string input, string repoaddr, string issuePath)
        {
            var regex = new Regex(@"\b(\w+\/)?\w+#\d+\b|\#\d+\b");
            var matches = regex.Matches(input);
            for (int i = 0; i < matches.Count; i++)
            {
                string m = matches[i].Value;
                string[] issueGp = m.Split("#");
                int issueNumber = int.Parse(issueGp[1]);

                input = input.Replace(m, $"[{m}]({LinkBuilder(repoaddr, issueGp[0]) + issuePath + issueNumber})");
            }

            return input;
        }

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

        public static string BuildHeader(ConfigType config)
        {
            string header1 = $"# {config.title}\n\n{config.description}\n";
            return header1;
        }

        public static string BuildContent(List<RepositoryInfo> repos, string commits, ConfigType config,
            string remoteaddr)
        {
            var canbewriten = false;
            var tagIdx = -1;
            var tagGroups = new List<CategorizedCommits>();
            var lines = commits.Split("\n");
            var comPtn = @"^commit\s.*";
            var tagPtn = @"tag:\s.*\)";
            var tagGet = @"tag:\s([^,\n\)]+)";
            var aorPtn = @"Author:\s+(\w+\.\w+)";
            var dtePtn = @"Date:\s([^\n]+)";
            var dteRef = "";
            string aorRef;

            foreach (string line in lines)
            {
                // 匹配commit信息（tag）
                Match comMtc = Regex.Match(line, comPtn);
                if (comMtc.Success)
                {
                    var tagMtc = Regex.Match(comMtc.Value, tagPtn);
                    if (tagMtc.Success)
                    {
                        var tagMtg = Regex.Match(tagMtc.Value, tagGet);
                        if (tagMtg.Success)
                        {
                            tagGroups.Add(new CategorizedCommits
                            {
                                TagName = tagMtg.Groups[1].Value,
                                GroupedCommits = new List<TypedCommits>()
                            });
                            tagIdx += 1;
                            continue;
                        }
                    }
                }

                // 匹配作者
                Match aotMtc = Regex.Match(line, aorPtn);
                if (aotMtc.Success)
                {
                    aorRef = aotMtc.Groups[1].Value;

                    if (config.onlyusers.Any(o => o.value == aorRef))
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
                Match dteMtc = Regex.Match(line, dtePtn);
                if (dteMtc.Success)
                {
                    dteRef = DateFormater(dteMtc.Value.Substring(8));
                    continue;
                }

                // 匹配提交信息
                if (canbewriten && tagIdx > -1)
                {
                    foreach (CommitType cmt in config.committypes)
                    {
                        if (line.Trim().StartsWith(cmt.value))
                        {
                            if (tagGroups.Count > tagIdx)
                            {
                                List<TypedCommits> nowTypes = tagGroups[tagIdx].GroupedCommits;
                                int i = nowTypes.FindIndex(o => o.Type == cmt.value);
                                if (i == -1)
                                {
                                    nowTypes.Add(new TypedCommits
                                    {
                                        Type = cmt.value,
                                        TypeDescription = cmt.description,
                                        Values = new List<string> { line }
                                    });
                                }
                                else
                                {
                                    nowTypes[i].Values.Add(line);
                                }
                            }

                            break;
                        }
                    }
                }
            }

            // 构建文本内容
            string content = "";
            var myPlatform = repos.FirstOrDefault(p => p.name == config.useplatform) ?? null;
            for (int i = 0; i < tagGroups.Count; i++)
            {
                int t = (i + 1) % tagGroups.Count;

                // 构建标签号
                content = content
                          + "\n## "
                          + tagGroups[i].TagName
                          + $" ([{dteRef}]({remoteaddr}{myPlatform?.tagbri}{tagGroups[i].TagName}...{tagGroups[t].TagName}))\n";

                // 构建提交类型以及内容
                foreach (TypedCommits tc in tagGroups[i].GroupedCommits)
                {
                    content = content
                              + $"\n### {tc.Type}\n\n";

                    foreach (string cm in tc.Values)
                    {
                        content = content
                                  + $"- {ParseIssueAddress(cm.Trim(), remoteaddr, myPlatform!.issuebri)}\n";
                    }
                }
            }

            return content;
        }
    }
}