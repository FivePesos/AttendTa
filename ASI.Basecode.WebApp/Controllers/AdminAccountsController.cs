using System.Threading.Tasks;
using ASI.Basecode.Resources.Constants;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ASI.Basecode.WebApp.Controllers
{
    /// <summary>
    /// Administrator-only, read-only list of accounts, roles and access status.
    /// </summary>
    [Authorize(Policy = Policies.Administrator)]
    public class AdminAccountsController : ControllerBase<AdminAccountsController>
    {
        private readonly IAccountAdministrationService _accountAdministrationService;

        public AdminAccountsController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            IAccountAdministrationService accountAdministrationService) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _accountAdministrationService = accountAdministrationService;
        }

        [HttpGet]
        [Route("admin/accounts")]
        public async Task<IActionResult> Index(string search = null, int? page = null)
        {
            ViewBag.Search = search;
            var result = await _accountAdministrationService.GetPageAsync(search, page, HttpContext.RequestAborted);
            return View(result);
        }
    }
}
