using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace ArshaHamrah.Areas.Admin.Pages;

[Authorize(Roles = "Admin,SuperAdmin")]
public class BlogCategoryModel : PageModel
{
    public void OnGet()
    {
    }
}