using System.Text;

namespace commitor.pears;

public static class App
{
    public static string ToolName = "Crommitor";
    public static string Date = "2023-7-7";
    public static string Version = "1.0.0";
    public static string Author = "kuyoru-kamikisho";
    public static string ProjectAddress = "https://github.com/kuyoru-kamikisho/crommitor";
    public static string ReportIssue = "https://github.com/kuyoru-kamikisho/crommitor/issues";

    public static void HelpMe()
    {
        var text = File.ReadAllText("./resources/assist", Encoding.UTF8);
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