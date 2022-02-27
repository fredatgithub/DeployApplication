using Deploy.Properties;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Reflection;

namespace Deploy
{
  internal class Program
  {
    static void Main(string[] arguments)
    {
      if (arguments.Length == 0)
      {
        ApplicationUsage();
        //return; // uncomment for test and prod
      }

      Action<string> Display = Console.WriteLine;
      DisplayColorLettersWithReturn(ConsoleColor.White, $"Deployment of application with GIT {GetVersion()} created by Freddy Juhel copyright MIT {DateTime.Now.Year}");
      Display(string.Empty);
      DisplayColorLettersWithReturn(ConsoleColor.White, "cloning a repo");
      Display(string.Empty);
      string gitRepo = Settings.Default.GitRepositoryUrl;
      string gitRepoDirectoryInitial = Settings.Default.LocalBuildDirectory;
      string gitRepoDirectoryFinal = Path.Combine(Settings.Default.LocalBuildDirectory, "matrix");
      if (GitClone(gitRepoDirectoryInitial, gitRepo))
      {
        DisplayColorLettersWithReturn(ConsoleColor.Green, $"The git repo: {gitRepo} has been cloned in the directory: {gitRepoDirectoryInitial}");
        Display(string.Empty);
      }
      else
      {
        DisplayColorLettersWithReturn(ConsoleColor.Red, $"The git repo: {gitRepo} has not been cloned in the directory: {gitRepoDirectoryInitial}");
        Display(string.Empty);
        return;
      }

      if (DotNetBuild(gitRepoDirectoryFinal))
      {
        DisplayColorLettersWithReturn(ConsoleColor.Green, $"The local repo {gitRepoDirectoryFinal} has been built correctly");
        Display(string.Empty);
      }
      else
      {
        DisplayColorLettersWithReturn(ConsoleColor.Red, $"The local repo {gitRepoDirectoryFinal} has not been built");
        Display(string.Empty);
        return;
      }

      if (DotNetPublish(gitRepoDirectoryFinal))
      {
        DisplayColorLettersWithReturn(ConsoleColor.Green, $"The local repo {gitRepoDirectoryFinal} has been published correctly");
        Display(string.Empty);
      }
      else
      {
        DisplayColorLettersWithReturn(ConsoleColor.Red, $"The local repo {gitRepoDirectoryFinal} has not been published");
        Display(string.Empty);
      }

      DisplayColorLettersWithReturn(ConsoleColor.White, $"Starting Explorer.exe to access to build/publish directory");
      Display(string.Empty);
      StartProcess("explorer.exe", gitRepoDirectoryFinal);
      Display("Press any key to exit:");
      Console.ReadKey();
    }

    private static void ApplicationUsage()
    {
      Action<string> Display = Console.WriteLine;
      Display("How to use this application:");
      Display(string.Empty);
      Display("deploy.exe <environment");
      Display("Environement can be integration, preprod, prod:");
      Display("int");
      Display("pprod");
      Display("prod");
    }

    private static bool GitInitAndGitPush(string directoryPath, string message = "Initial commit")
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

    private static void StartProcess(string dosScript, string arguments = "", bool useShellExecute = true, bool createNoWindow = false)
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

    private static void DisplayColorLettersWithReturn(ConsoleColor color, string message)
    {
      Console.ForegroundColor = color;
      Console.WriteLine(message);
    }

    private static string GetVersion()
    {
      Assembly assembly = Assembly.GetExecutingAssembly();
      FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
      return $"V{fvi.FileMajorPart}.{fvi.FileMinorPart}.{fvi.FileBuildPart}.{fvi.FilePrivatePart}";
    }
  }
}
