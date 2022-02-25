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
      string gitRepo = "https://github.com/fredatgithub/GitAutoUpdate";
      string gitRepoDirectory = @"c:\temp";
      if (GitClone(gitRepoDirectory, gitRepo))
      {
        Display($"The git repo: {gitRepo} has been cloned in the directory: {gitRepoDirectory}");
      }
      else
      {
        Display($"The git repo: {gitRepo} has not been cloned in the directory: {gitRepoDirectory}");
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
  }
}
