using Deploy.Properties;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;

namespace Deploy
{
  internal class Program
  {
    static void Main(string[] arguments)
    {
      Action<string> Display = Console.WriteLine;
      DisplayColorLetters(ConsoleColor.White, "Deployment of application with GIT");
      Display(string.Empty);
      DisplayColorLetters(ConsoleColor.White, "cloning a repo");
      Display(string.Empty);
      string gitRepo = Settings.Default.GitRepositoryUrl;
      string gitRepoDirectoryInitial = Settings.Default.LocalBuildDirectory;
      string gitRepoDirectoryFinal = Path.Combine(Settings.Default.LocalBuildDirectory, "matrix");
      if (GitClone(gitRepoDirectoryInitial, gitRepo))
      {
        DisplayColorLetters(ConsoleColor.Green, $"The git repo: {gitRepo} has been cloned in the directory: {gitRepoDirectoryInitial}");
        Display(string.Empty);
      }
      else
      {
        DisplayColorLetters(ConsoleColor.Red, $"The git repo: {gitRepo} has not been cloned in the directory: {gitRepoDirectoryInitial}");
        Display(string.Empty);
        return;
      }

      if (DotNetBuild(gitRepoDirectoryFinal))
      {
        DisplayColorLetters(ConsoleColor.Green, $"The local repo {gitRepoDirectoryFinal} has been built");
        Display(string.Empty);
      }
      else
      {
        DisplayColorLetters(ConsoleColor.Red, $"The local repo {gitRepoDirectoryFinal} has not been built");
        Display(string.Empty);
        return;
      }

      if (DotNetPublish(gitRepoDirectoryFinal))
      {
        DisplayColorLetters(ConsoleColor.Green, $"The local repo {gitRepoDirectoryFinal} has been published");
        Display(string.Empty);
      }
      else
      {
        DisplayColorLetters(ConsoleColor.Red, $"The local repo {gitRepoDirectoryFinal} has not been published");
        Display(string.Empty);
      }

      DisplayColorLetters(ConsoleColor.White, $"Starting Explorer.exe to access to build/publish directory");
      Display(string.Empty);
      StartProcess("explorer.exe", gitRepoDirectoryFinal);
      Display("Press any key to exit:");
      Console.ReadKey();
    }

    private static bool GitInitAndGitPush(string directoryPath, string message = "commit changes")
    {
      bool result = true;
      try
      {
        if (!Directory.Exists(directoryPath))
        {
          Directory.CreateDirectory(directoryPath);
        }
      }
      catch (Exception)
      {
        return false;
      }

      try
      {
        using (PowerShell powershell = PowerShell.Create())
        {
          powershell.AddScript($"cd {directoryPath}");
          powershell.AddScript(@"git init");
          powershell.AddScript(@"git add *");
          powershell.AddScript($@"git commit -m '{message}'");
          powershell.AddScript(@"git push");
          Collection<PSObject> results = powershell.Invoke();
          result = true;
        }
      }
      catch (Exception)
      {
        result = false; 
      }

      return result;
    }

    private static bool GitClone(string directoryPath, string gitPath)
    {
      bool result = true;
      try
      {
        if (!Directory.Exists(directoryPath))
        {
          Directory.CreateDirectory(directoryPath);
        }
      }
      catch (Exception)
      {
        return false;
      }

      try
      {
        using (PowerShell powershell = PowerShell.Create())
        {
          powershell.AddScript($"cd {directoryPath}");
          powershell.AddScript($@"git clone {gitPath}");
          Collection<PSObject> results = powershell.Invoke();
          result=true;
        }
      }
      catch (Exception)
      {
        result = false;
      }

      return result;
    }

    private static bool DotNetBuild(string directoryPath)
    {
      bool result = true;
      try
      {
        if (!Directory.Exists(directoryPath))
        {
          Directory.CreateDirectory(directoryPath);
        }
      }
      catch (Exception)
      {
        return false;
      }

      try
      {
        using (PowerShell powershell = PowerShell.Create())
        {
          powershell.AddScript($"cd {directoryPath}");
          powershell.AddScript("dotnet restore");
          powershell.AddScript("dotnet build");
          Collection<PSObject> results = powershell.Invoke();
          result = true;
        }
      }
      catch (Exception)
      {
        result = false;
      }

      return result;
    }

    private static bool DotNetPublish(string directoryPath)
    {
      bool result = true;
      try
      {
        if (!Directory.Exists(directoryPath))
        {
          Directory.CreateDirectory(directoryPath);
        }
      }
      catch (Exception)
      {
        return false;
      }

      try
      {
        using (PowerShell powershell = PowerShell.Create())
        {
          powershell.AddScript($"cd {directoryPath}");
          powershell.AddScript("dotnet publish");
          Collection<PSObject> results = powershell.Invoke();
          result = true;
        }
      }
      catch (Exception)
      {
        result = false;
      }

      return result;
    }

    public static void StartProcess(string dosScript, string arguments = "", bool useShellExecute = true, bool createNoWindow = false)
    {
      Process task = new Process
      {
        StartInfo =
        {
          UseShellExecute = useShellExecute,
          FileName = dosScript,
          Arguments = arguments,
          CreateNoWindow = createNoWindow
        }
      };

      task.Start();
    }

    private static void DisplayColorLetters(ConsoleColor color, string message)
    {
      Console.ForegroundColor = color;
      Console.Write(message);
    }
  }
}
