string currentDirectory = Environment.CurrentDirectory;
string? path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
if (path != null)
{
    // 将当前目录添加到 PATH 变量中
    path += ";" + currentDirectory;

    // 更新 PATH 变量
    Environment.SetEnvironmentVariable("PATH", path, EnvironmentVariableTarget.User);
}