using commitor.src.pears;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace commitor.src.utils
{
    public static class Methods
    {
        public static string LinkBuilder(string url, string path)
        {
            Uri uri = new Uri(url);
            string newPath = "";
            if (uri.AbsolutePath.LastIndexOf("/") > 0)
            {
                newPath = uri.AbsolutePath.Substring(0, uri.AbsolutePath.LastIndexOf("/")) + "/";
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
        public static string BuildContent(List<Repository> repos, string commits, ConfigType config, string remoteaddr)
        {
            bool canbewriten = false;
            int tag_idx = -1;
            var tag_groups = new List<CategorizedCommits>();
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
                            tag_groups.Add(new CategorizedCommits
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
                if (canbewriten && tag_idx > -1)
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
            Repository myPlatform = repos.FirstOrDefault(p => p.name == config.useplatform);
            for (int i = 0; i < tag_groups.Count; i++)
            {
                int t = (i + 1) % tag_groups.Count;

                // 构建标签号
                content = content
                    + "\n## "
                    + tag_groups[i].TagName
                    + $" ([{dte_ref}]({remoteaddr}{myPlatform.tagbri}{tag_groups[i].TagName}...{tag_groups[t].TagName}))\n";

                // 构建提交类型以及内容
                foreach (TypedCommits tc in tag_groups[i].GroupedCommits)
                {
                    content = content
                        + $"\n### {tc.Type}\n\n";

                    foreach (string cm in tc.Values)
                    {
                        content = content
                            + $"- {ParseIssueAddress(cm.Trim(), remoteaddr, myPlatform.issuebri)}\n";
                    }

                }
            }

            return content;
        }
    }
}
