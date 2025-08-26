using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Menulo.Domain.Entities;
using Menulo.Infrastructure.Persistence;

namespace Menulo.Pages.Categories
{
    public class IndexModel : PageModel
    {
        private readonly Menulo.Infrastructure.Persistence.AppDbContext _context;

        public IndexModel(Menulo.Infrastructure.Persistence.AppDbContext context)
        {
            _context = context;
        }

        public IList<Category> Category { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Category = await _context.Categories
                .Include(c => c.Restaurant).ToListAsync();
        }
    }
}
