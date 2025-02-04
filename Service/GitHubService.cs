using System.Net.Http.Headers;
using System.Net;
using Octokit;
using Microsoft.Extensions.Options;


namespace Service
{
    public class GitHubService : IGitHubService
    {
        private readonly GitHubClient _client;

        public GitHubService(IOptions<GitHub> options )
        {
            _client = new GitHubClient(new Octokit.ProductHeaderValue("CVSite"))
            {
                Credentials = new Credentials(options.Value.Token)
            };
        }

        public async Task<List<RepositoryInfo>> GetPortfolioAsync(string userName)
        {
            var repositories = await _client.Repository.GetAllForUser(userName);

            var portfolio = new List<RepositoryInfo>();
            foreach (var repo in repositories)
            {
                var languages = await _client.Repository.GetAllLanguages(repo.Id);
                portfolio.Add(new RepositoryInfo
                {
                    Name = repo.Name,
                    LastCommit = repo.PushedAt,
                    Stars = repo.StargazersCount,
                    PullRequests = await GetPullRequestCount(repo.Id),
                    Url = repo.HtmlUrl,
                    Languages = languages.Select(language => language.Name)
                });
            }
            return portfolio;
        }

        public async Task<List<RepositoryInfo>> SearchRepositoriesAsync(string repoName = "", string language = "", string userName = "")
        {
            var request = new SearchRepositoriesRequest(repoName);

            if (!string.IsNullOrEmpty(language))
            {
                try
                {
                    var languageEnum = Enum.Parse<Language>(language, true);
                    request.Language = languageEnum;
                }
                catch
                {
                    throw new ArgumentException("Invalid language provided.");
                }
            }

            if (!string.IsNullOrEmpty(userName))
            {
                request.User = userName;
            }

            var result = await _client.Search.SearchRepo(request);

            var repositoriesInfo = new List<RepositoryInfo>();

            foreach (var repo in result.Items)
            {
                var languages = await _client.Repository.GetAllLanguages(repo.Owner.Login, repo.Name); 
                var pullRequests = await _client.PullRequest.GetAllForRepository(repo.Id);

                repositoriesInfo.Add(new RepositoryInfo
                {
                    Name = repo.Name,
                    LastCommit = repo.PushedAt, 
                    Stars = repo.StargazersCount, 
                    PullRequests = pullRequests.Count, 
                    Url = repo.HtmlUrl, 
                    Languages = languages.Select(language => language.Name) 
                });
            }

            return repositoriesInfo;
        }
        private async Task<int> GetPullRequestCount(long repoId)
        {
            var pullRequests = await _client.PullRequest.GetAllForRepository(repoId);
            return pullRequests.Count;
        }
    }


    public class RepositoryInfo
    {
        public string Name { get; set; }
        public DateTimeOffset? LastCommit { get; set; }
        public int Stars { get; set; }
        public int PullRequests { get; set; }
        public string Url { get; set; }
        public IEnumerable<string> Languages { get; set; }
    }



}