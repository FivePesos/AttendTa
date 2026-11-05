using ASI.Basecode.Resources;
using ASI.Basecode.Resources.Constants;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    [Authorize(Policy = Policies.Management)]
    [AutoValidateAntiforgeryToken]
    public class MemberController : ControllerBase<MemberController>
    {
        private readonly IMemberService _memberService;

        public MemberController(IHttpContextAccessor httpContextAccessor,
                                ILoggerFactory loggerFactory,
                                IConfiguration configuration,
                                IMapper mapper,
                                IMemberService memberService) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _memberService = memberService;
        }

        [HttpGet]
        [Route("Member")]
        [Route("Member/Index")]
        public IActionResult Index(string search)
        {
            var members = _memberService.GetMembers(search);
            ViewBag.Search = search;
            return View(members);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var model = _memberService.GetMember(id);
            if (model == null)
            {
                return RedirectToAction("Index");
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(MemberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _memberService.UpdateMemberAsync(model, GetActorUserId(), HttpContext.RequestAborted);
                TempData["SuccessMessage"] = "Member updated successfully.";
                return RedirectToAction("Index");
            }
            catch (DomainException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                await _memberService.ToggleStatusAsync(id, GetActorUserId(), HttpContext.RequestAborted);
                TempData["SuccessMessage"] = "Member status updated.";
            }
            catch (DomainException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction("Index");
        }
    }
}
