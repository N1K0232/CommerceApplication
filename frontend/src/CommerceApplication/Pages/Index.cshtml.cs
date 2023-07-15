using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CommerceApplication.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public Task<IActionResult> OnGetAsync()
    {
        var result = Page();
        return Task.FromResult<IActionResult>(result);
    }

    public Task<IActionResult> OnPostAsync(string email, string password)
    {
        var result = Page();
        return Task.FromResult<IActionResult>(result);
    }
}