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
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    [Authorize(Policy = Policies.Management)]
    [AutoValidateAntiforgeryToken]
    public class VenueController : ControllerBase<VenueController>
    {
        private readonly IVenueService _venueService;
        private readonly IPrivateFileStore _privateFileStore;

        public VenueController(IHttpContextAccessor httpContextAccessor,
                               ILoggerFactory loggerFactory,
                                IConfiguration configuration,
                                IMapper mapper,
                                IVenueService venueService,
                                IPrivateFileStore privateFileStore) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _venueService = venueService;
            _privateFileStore = privateFileStore;
        }

        public IActionResult Index()
        {
            var venues = _venueService.GetVenues();
            return View(venues);
        }

        [HttpGet]
        public async Task<IActionResult> Photo(int id, CancellationToken cancellationToken)
        {
            var venue = _venueService.GetVenue(id);
            if (venue == null)
            {
                return NotFound();
            }

            var extension = Path.GetExtension(venue.ImagePath ?? string.Empty).ToLowerInvariant();
            var contentType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => null
            };
            if (contentType is null)
            {
                return NotFound();
            }

            try
            {
                var stream = await _privateFileStore.OpenReadAsync(venue.ImagePath, cancellationToken);
                return File(stream, contentType);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException)
            {
                return NotFound();
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new VenueViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(VenueViewModel model, IFormFile photo)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string savedImagePath = null;
            try
            {
                var actorUserId = GetActorUserId();
                if (actorUserId <= 0)
                {
                    throw new DomainException(OutcomeCode.Forbidden, "A valid authenticated user is required to create a venue.");
                }

                await using var content = photo?.OpenReadStream();
                if (content == null)
                {
                    throw new InvalidDataException("A venue photo is required.");
                }

                savedImagePath = await _privateFileStore.SaveAsync(
                    PrivateFileArea.VenueImages,
                    content,
                    Path.GetExtension(photo.FileName),
                    HttpContext.RequestAborted);
                model.ImagePath = savedImagePath;
                await _venueService.AddVenueAsync(model, actorUserId, HttpContext.RequestAborted);
                TempData["SuccessMessage"] = "Venue added successfully.";
                return RedirectToAction("Index");
            }
            catch (InvalidDataException ex)
            {
                if (!string.IsNullOrWhiteSpace(savedImagePath))
                {
                    TryDeleteVenueImage(savedImagePath);
                }
                ModelState.AddModelError("photo", ex.Message);
                return View(model);
            }
            catch (DomainException ex)
            {
                if (!string.IsNullOrWhiteSpace(savedImagePath))
                {
                    TryDeleteVenueImage(savedImagePath);
                }
                TempData["ErrorMessage"] = ex.Message;
                return View(model);
            }
            catch
            {
                if (!string.IsNullOrWhiteSpace(savedImagePath))
                {
                    TryDeleteVenueImage(savedImagePath);
                }
                throw;
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var model = _venueService.GetVenue(id);
            if (model == null)
            {
                return RedirectToAction("Index");
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(VenueViewModel model, IFormFile photo)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string newImagePath = null;
            var databaseUpdateSucceeded = false;
            var existing = _venueService.GetVenue(model.VenueId);
            if (existing == null)
            {
                return RedirectToAction("Index");
            }

            try
            {
                var actorUserId = GetActorUserId();
                if (actorUserId <= 0)
                {
                    throw new DomainException(OutcomeCode.Forbidden, "A valid authenticated user is required to update a venue.");
                }

                model.ImagePath = existing.ImagePath;
                if (photo != null && photo.Length > 0)
                {
                    await using var content = photo.OpenReadStream();
                    newImagePath = await _privateFileStore.SaveAsync(
                        PrivateFileArea.VenueImages,
                        content,
                        Path.GetExtension(photo.FileName),
                        HttpContext.RequestAborted);
                    model.ImagePath = newImagePath;
                }

                await _venueService.UpdateVenueAsync(model, actorUserId, HttpContext.RequestAborted);
                databaseUpdateSucceeded = true;
                if (!string.IsNullOrWhiteSpace(newImagePath)
                    && !string.Equals(existing.ImagePath, newImagePath, System.StringComparison.OrdinalIgnoreCase))
                {
                    TryDeleteVenueImage(existing.ImagePath);
                }
                TempData["SuccessMessage"] = "Venue updated successfully.";
                return RedirectToAction("Index");
            }
            catch (InvalidDataException ex)
            {
                if (!databaseUpdateSucceeded && !string.IsNullOrWhiteSpace(newImagePath))
                {
                    TryDeleteVenueImage(newImagePath);
                }
                ModelState.AddModelError("photo", ex.Message);
                return View(model);
            }
            catch (DomainException ex)
            {
                if (!databaseUpdateSucceeded && !string.IsNullOrWhiteSpace(newImagePath))
                {
                    TryDeleteVenueImage(newImagePath);
                }
                TempData["ErrorMessage"] = ex.Message;
                return View(model);
            }
            catch
            {
                if (!databaseUpdateSucceeded && !string.IsNullOrWhiteSpace(newImagePath))
                {
                    TryDeleteVenueImage(newImagePath);
                }
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            try
            {
                await _venueService.ToggleActiveAsync(id, GetActorUserId(), HttpContext.RequestAborted);
                TempData["SuccessMessage"] = "Venue status updated.";
            }
            catch (DomainException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        private void TryDeleteVenueImage(string relativePath)
        {
            try
            {
                _privateFileStore.Delete(relativePath);
            }
            catch (Exception ex) when (ex is IOException
                                       || ex is UnauthorizedAccessException
                                       || ex is InvalidDataException)
            {
                _logger.LogWarning("Could not clean up a venue photo after its database update.");
            }
        }
    }
}
