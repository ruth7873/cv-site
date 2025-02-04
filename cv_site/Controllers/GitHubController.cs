using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Service;

[ApiController]
[Route("api/[controller]")]
public class GitHubController : ControllerBase
{
    private readonly IGitHubService _gitHubService;

    public GitHubController(IGitHubService gitHubService)
    {
        _gitHubService = gitHubService;
    }

    [HttpGet("portfolio")]
    public async Task<IActionResult> GetPortfolio(string userName)
    {
        var result = await _gitHubService.GetPortfolioAsync(userName);
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchRepositories(string repoName = "", string language = "", string userName = "")
    {
        var result = await _gitHubService.SearchRepositoriesAsync(repoName, language, userName);
        return Ok(result);
    }
}