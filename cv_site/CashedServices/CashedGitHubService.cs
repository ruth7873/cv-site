using Microsoft.Extensions.Caching.Memory;
using Service;

namespace cv_site.CashedServices
{
    public class CashedGitHubService : IGitHubService
    {
        private readonly IGitHubService _gitHubService;
        private readonly IMemoryCache _memoeyCache;
        private const string UserPortfolioKey = "UserPortfolioKey";
        public CashedGitHubService(IGitHubService gitHubService, IMemoryCache memoryCache)
        {
            _gitHubService = gitHubService;
            _memoeyCache = memoryCache;
        }
        public async Task<List<RepositoryInfo>> GetPortfolioAsync(string userName)
        {
            if (_memoeyCache.TryGetValue(UserPortfolioKey, out List<RepositoryInfo> portfolio))
                return portfolio;

            // Create cache options
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(30))
                .SetSlidingExpiration(TimeSpan.FromSeconds(10));

            portfolio = await _gitHubService.GetPortfolioAsync(userName);
            _memoeyCache.Set(UserPortfolioKey, portfolio, cacheOptions);
            return portfolio;
        }


        public Task<List<RepositoryInfo>> SearchRepositoriesAsync(string repoName = "", string language = "", string userName = "")
        {
            return _gitHubService.SearchRepositoriesAsync();
        }
    }
}
