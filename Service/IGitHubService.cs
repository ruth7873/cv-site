using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octokit;

namespace Service
{
    public interface IGitHubService
    {
        Task<List<RepositoryInfo>> GetPortfolioAsync(string userName);
        Task<List<RepositoryInfo>> SearchRepositoriesAsync(string repoName = "", string language = "", string userName = "");
      

    }
}