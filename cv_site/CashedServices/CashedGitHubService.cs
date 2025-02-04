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
        //public async Task<List<RepositoryInfo>> GetPortfolioAsync(string userName)
        //{
        //    if (_memoeyCache.TryGetValue(UserPortfolioKey, out List<RepositoryInfo> portfolio))
        //        return portfolio;

        //    // Create cache options
        //    var cacheOptions = new MemoryCacheEntryOptions()
        //        .SetAbsoluteExpiration(TimeSpan.FromSeconds(30))
        //        .SetSlidingExpiration(TimeSpan.FromSeconds(10));

        //    portfolio = await _gitHubService.GetPortfolioAsync(userName);
        //    _memoeyCache.Set(UserPortfolioKey, portfolio, cacheOptions);
        //    return portfolio;
        //}
        public async Task<List<RepositoryInfo>> GetPortfolioAsync(string userName)
        {
            // בדוק אם יש מידע ב-cache
            if (_memoeyCache.TryGetValue(UserPortfolioKey, out (List<RepositoryInfo> portfolio, DateTimeOffset lastUpdated) cachedData))
            {
                // קבל את המידע על ה-repositories
                var newPortfolio = await _gitHubService.GetPortfolioAsync(userName);

                // בדוק אם התבצעה עדכון על ידי השוואת LastCommit
                if (newPortfolio.Any(repo => repo.LastCommit > cachedData.lastUpdated))
                {
                    // אם התבצעה עדכון, נקה את ה-cache
                    _memoeyCache.Remove(UserPortfolioKey);
                }
                else
                {
                    // אם לא, החזר את המידע מה-cache
                    return cachedData.portfolio;
                }
            }

            // קבל את המידע מחדש אם ה-cache ריק או אם התבצעה עדכון
            var portfolio = await _gitHubService.GetPortfolioAsync(userName);

            // שמור את המידע ב-cache עם הזמן הנוכחי
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(30))
                .SetSlidingExpiration(TimeSpan.FromSeconds(10));

            _memoeyCache.Set(UserPortfolioKey, (portfolio, DateTimeOffset.UtcNow), cacheOptions);
            return portfolio;
        }


        public Task<List<RepositoryInfo>> SearchRepositoriesAsync(string repoName = "", string language = "", string userName = "")
        {
            return _gitHubService.SearchRepositoriesAsync();
        }
    }
}
