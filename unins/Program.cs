string currentDirectory = Environment.CurrentDirectory;
string? path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
if (path != null)
{
    // 从 PATH 变量中移除当前目录
    path = path.Replace(";" + currentDirectory, string.Empty);
    path = path.Replace(currentDirectory + ";", string.Empty);
    path = path.Replace(currentDirectory, string.Empty);

    // 更新 PATH 变量
    Environment.SetEnvironmentVariable("PATH", path, EnvironmentVariableTarget.User);
}