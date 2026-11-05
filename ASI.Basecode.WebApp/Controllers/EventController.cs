using ASI.Basecode.Data.Models.Lookups;
using ASI.Basecode.Resources;
using ASI.Basecode.Resources.Constants;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.WebApp.Formatting;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    /// <summary>
    /// Management draft Event screens. Posted <c>datetime-local</c> values are Asia/Manila and are
    /// converted to UTC here, so Services never convert time zones (AD-11).
    /// </summary>
    [Authorize(Policy = Policies.Management)]
    [AutoValidateAntiforgeryToken]
    public class EventController : ControllerBase<EventController>
    {
        private const string EventIndexRoute = "EventIndex";

        private readonly IEventLifecycleService _eventService;

        public EventController(IHttpContextAccessor httpContextAccessor,
                               ILoggerFactory loggerFactory,
                               IConfiguration configuration,
                               IMapper mapper,
                               IEventLifecycleService eventService) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _eventService = eventService;
        }

        [HttpGet]
        [Route("Event", Name = EventIndexRoute)]
        [Route("Event/Index")]
        public async Task<IActionResult> Index(int? page, CancellationToken cancellationToken) =>
            View(await _eventService.GetPageAsync(page, cancellationToken));

        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken cancellationToken) =>
            View(await WithOptionsAsync(new EventViewModel(), cancellationToken));

        [HttpPost]
        public async Task<IActionResult> Create(EventViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(await WithOptionsAsync(model, cancellationToken));
            }

            ToUtc(model);
            try
            {
                await _eventService.CreateDraftAsync(model, cancellationToken);
                TempData["SuccessMessage"] = "Draft event created.";
                return RedirectToRoute(EventIndexRoute);
            }
            catch (DomainException ex)
            {
                ToManila(model);
                return await HandleAsync(ex, model, cancellationToken);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var model = await _eventService.GetEventAsync(id, cancellationToken);
            if (model == null)
            {
                TempData["ErrorMessage"] = "Event not found.";
                return RedirectToRoute(EventIndexRoute);
            }

            if (model.EventStatusId != EventStatusCode.Draft)
            {
                TempData["ErrorMessage"] = "Only draft events can be edited.";
                return RedirectToRoute(EventIndexRoute);
            }

            ToManila(model);
            return View(await WithOptionsAsync(model, cancellationToken));
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EventViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(await WithOptionsAsync(model, cancellationToken));
            }

            ToUtc(model);
            try
            {
                await _eventService.UpdateDraftAsync(model, cancellationToken);
                TempData["SuccessMessage"] = "Draft event updated.";
                return RedirectToRoute(EventIndexRoute);
            }
            catch (DomainException ex)
            {
                ToManila(model);
                return await HandleAsync(ex, model, cancellationToken);
            }
        }

        private async Task<EventViewModel> WithOptionsAsync(EventViewModel model, CancellationToken cancellationToken)
        {
            model.FormOptions = await _eventService.GetFormOptionsAsync(cancellationToken);
            return model;
        }

        private async Task<IActionResult> HandleAsync(DomainException exception, EventViewModel model, CancellationToken cancellationToken)
        {
            if (exception.Field == null)
            {
                return HandleDomainException(exception, model, nameof(Index));
            }

            model.FormOptions = await _eventService.GetFormOptionsAsync(cancellationToken);
            return HandleDomainException(exception, model);
        }

        // Form controls post Asia/Manila wall-clock values; SQL stores UTC.
        private static void ToUtc(EventViewModel model)
        {
            if (model.StartsAt != default)
            {
                model.StartsAt = ManilaTimeFormatter.ToUtc(model.StartsAt);
            }

            if (model.RegistrationDeadline != default)
            {
                model.RegistrationDeadline = ManilaTimeFormatter.ToUtc(model.RegistrationDeadline);
            }
        }

        private static void ToManila(EventViewModel model)
        {
            if (model.StartsAt != default)
            {
                model.StartsAt = ManilaTimeFormatter.ToLocal(model.StartsAt);
            }

            if (model.RegistrationDeadline != default)
            {
                model.RegistrationDeadline = ManilaTimeFormatter.ToLocal(model.RegistrationDeadline);
            }
        }
    }
}
