using System.Reflection;
using System.Text;

namespace comi.pears;

public static class App
{
    public static ConsoleRider ConsoleEntry { set; get; }
    public static string ToolName = "Crommitor";
    public static string Date = "2023-8-24";
    public static string Version = "1.1.21";
    public static string Author = "kuyoru-kamikisho";
    public static string ProjectAddress = "https://github.com/kuyoru-kamikisho/crommitor";
    public static string ReportIssue = "https://github.com/kuyoru-kamikisho/crommitor/issues";

    public static void HelpMe()
    {
        var location = Assembly.GetExecutingAssembly().Location;
        var directory = Path.GetDirectoryName(location)!;
        var helpfilePath = "./resources/assist";

        var filePath = Path.Combine(directory, helpfilePath);
        var fullPath = Path.GetFullPath(filePath);
        helpfilePath = fullPath;
        
        var text = File.ReadAllText(helpfilePath, Encoding.UTF8);
        Console.WriteLine(ToolName + " " + Version);
        Console.WriteLine("Author: " + Author);
        Console.WriteLine(text);
        Console.WriteLine("The above information may be outdated.\n" +
                          "Latest update:" + Date + "\n" +
                          "If you can't find the information you need, " +
                          "you can go to the project address to find the information you need: " +
                          ProjectAddress);
    }
}