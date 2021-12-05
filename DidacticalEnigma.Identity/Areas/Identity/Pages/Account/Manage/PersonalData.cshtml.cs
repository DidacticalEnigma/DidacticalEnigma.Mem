// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Threading.Tasks;
using DidacticalEnigma.Identity.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace DidacticalEnigma.Identity.Areas.Identity.Pages.Account.Manage
{
    public class PersonalDataModel : PageModel
    {
        public async Task<IActionResult> OnGet()
        {
            return NotFound();
        }
    }
}
