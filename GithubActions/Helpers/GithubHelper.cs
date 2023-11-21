using GithubActions.Entities;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GithubActions.Helpers
{
    public class GithubHelper
    {
        private const string GITHUB_URL = "https://github.com/";
        private CMDHelper _cmd;
        private string localReposPath;
        public GithubHelper()
        {
                _cmd = new CMDHelper(); 
        }
        
        public async Task GetGithubRepository(GithubParams args)
        {
            localReposPath = args.LocalReposPath;

            if (String.IsNullOrEmpty(args.RepoName))
            {
                throw new ArgumentNullException("Repo name can't be null");
            }

            if (String.IsNullOrEmpty(args.GitUserName))
            {
                throw new ArgumentNullException("Repo user name can't be null");
            }            

            var repoDir = Path.Combine(localReposPath, args.RepoName);            
            if (RepoExists(repoDir))
            {                                                
                DeleteRepo(repoDir);
            }
            await CreateRepo(args);
        }

        private async Task CreateRepo(GithubParams args)
        {                        
            await _cmd.Execute($@"{RepoBranch(args.BranchName)} {GITHUB_URL}{args.GitUserName}/{args.RepoName}", localReposPath);                       
        }

        private void DeleteRepo(string repoDir)
        {
            var oldRepoDir = new DirectoryInfo(repoDir);
            oldRepoDir.Delete(true);
        }

        private bool RepoExists(string repoDir) =>
            File.Exists(repoDir);

        private string RepoBranch(string branchName) =>
            !String.IsNullOrEmpty(branchName) ? $"--branch {branchName} " : "";
    }
}

