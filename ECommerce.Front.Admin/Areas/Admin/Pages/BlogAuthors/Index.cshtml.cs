using ECommerce.Services.IServices;

namespace ECommerce.Front.Admin.Areas.Admin.Pages.BlogAuthors;

public class IndexModel(IBlogAuthorService blogAuthorService) : PageModel
{
    public ServiceResult<List<BlogAuthor>> BlogAuthors { get; set; }
    [TempData] public string Message { get; set; }
    [TempData] public string Code { get; set; }

    public async Task<IActionResult> OnGet(string search = "", int pageNumber = 1, int pageSize = 10,
        string message = null, string code = null)
    {
        Message = message;
        Code = code;
        var result = await blogAuthorService.Load(search, pageNumber, pageSize);
        if (result.Code == ServiceCode.Success)
        {
            if (Message != null)
            {
                Message = Message;
                Code = Code;
            }
            else
            {
                Message = result.Message;
                Code = result.Code.ToString();
            }

            BlogAuthors = result;
            return Page();
        }

        return RedirectToPage("/index", new { message = result.Message, code = result.Code.ToString() });
    }
}