using ECommerce.Services.IServices;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerce.Front.Admin.Areas.Admin.Pages.Cities;

public class DetailModel(ICityService cityService, IStateService stateService) : PageModel
{
    public string StateName;

    public City City { get; set; }
    public SelectList StateCity { get; set; }

    public async Task<IActionResult> OnGet(int id)
    {
        var result = await cityService.GetById(id);
        var stateCity = (await stateService.GetAll()).ReturnData;

        if (result.Code == 0)
        {
            City = result.ReturnData;
            StateName = stateCity.First(x => x.Id == City.StateId).Name;
            return Page();
        }

        return RedirectToPage("/Cities/Index",
            new { area = "Admin", message = result.Message, code = result.Code.ToString() });
    }
}