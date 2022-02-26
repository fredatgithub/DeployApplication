using Deploy.Properties;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;

namespace Deploy
{
  internal class Program
  {
    static void Main(string[] arguments)
    {
      Action<string> Display = Console.WriteLine;
      Display("Deployment of application with GIT");
      Display("cloning a repo");
      string gitRepo = Settings.Default.GitRepositoryUrl;
      string gitRepoDirectoryInitial = Settings.Default.LocalBuildDirectory;
      string gitRepoDirectoryFinal = Path.Combine(Settings.Default.LocalBuildDirectory, "matrix");
      if (GitClone(gitRepoDirectoryInitial, gitRepo))
      {
        Display($"The git repo: {gitRepo} has been cloned in the directory: {gitRepoDirectoryInitial}");
      }
      else
      {
        Display($"The git repo: {gitRepo} has not been cloned in the directory: {gitRepoDirectoryInitial}");
        return;
      }

      if (DotNetBuild(gitRepoDirectoryFinal))
      {
        Display($"The local repo {gitRepoDirectoryFinal} has been built");
      }
      else
      {
        Display($"The local repo {gitRepoDirectoryFinal} has not been built");
        return;
      }

      if (DotNetPublish(gitRepoDirectoryFinal))
      {
        Display($"The local repo {gitRepoDirectoryFinal} has been published");
      }

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
  }
}
