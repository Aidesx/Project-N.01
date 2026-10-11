using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Project_385.Repository.Components
{
    public class CategoriesViewComponent : ViewComponent
    {
        private readonly DataContext _datacontext;
        public CategoriesViewComponent(DataContext context)
        {
            _datacontext = context;
        }
        public async Task<IViewComponentResult> InvokeAsync() =>View(await _datacontext.Categories.Where(x => x.Status == 1).ToListAsync());

    }
}