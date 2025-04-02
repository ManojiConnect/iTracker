using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Common;

public class DisplayMessageModel : PageModel
{
    public string Message { get; set; }

    public void OnGet(string message)
    {
        Message = message;
    }
}
