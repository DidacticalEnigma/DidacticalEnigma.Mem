// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DidacticalEnigma.Mem.Areas.Identity.Pages.Account.Manage
{
    public class PersonalDataModel : PageModel
    {
        public async Task<IActionResult> OnGet()
        {
            return NotFound();
        }
    }
}
