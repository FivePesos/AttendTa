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

namespace ASI.Basecode.WebApp.Controllers;

[Authorize(Policy = Policies.Management)]
public sealed class AccountRequestController : ControllerBase<AccountRequestController>
{
    private readonly IAccountRequestQueueService accountRequestQueueService;

    public AccountRequestController(
        IHttpContextAccessor httpContextAccessor,
        ILoggerFactory loggerFactory,
        IConfiguration configuration,
        IMapper mapper,
        IAccountRequestQueueService accountRequestQueueService)
        : base(httpContextAccessor, loggerFactory, configuration, mapper)
    {
        this.accountRequestQueueService = accountRequestQueueService;
    }

    [HttpGet]
    [Route("AccountRequest")]
    [Route("AccountRequest/Index")]
    public async Task<IActionResult> Index(string status = "pending", int? page = null)
    {
        var normalizedStatus = status?.Trim().ToLowerInvariant() switch
        {
            "approved" => "approved",
            "rejected" => "rejected",
            _ => "pending"
        };

        ViewBag.Status = normalizedStatus;
        var result = await accountRequestQueueService.GetPageAsync(normalizedStatus, page, HttpContext.RequestAborted);
        return View(result);
    }
}
