using ASI.Basecode.Data;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Repositories;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Security;
using ASI.Basecode.Services.Services;
using ASI.Basecode.WebApp.Authentication;
using ASI.Basecode.WebApp.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace ASI.Basecode.WebApp
{
    // Other services configuration
    internal partial class StartupConfigurer
    {
        /// <summary>
        /// Configures the other services.
        /// </summary>
        private void ConfigureOtherServices()
        {
            // Framework
            this._services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Common
            this._services.AddScoped<IUnitOfWork, UnitOfWork>();
            this._services.TryAddSingleton(TimeProvider.System);
            this._services.AddScoped<IActivityLogWriter, ActivityLogWriter>();

            // Services
            this._services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();
            this._services.AddScoped<IAccountService, AccountService>();
            this._services.AddScoped<IMemberService, MemberService>();
            this._services.AddScoped<IEventTypeService, EventTypeService>();
            this._services.AddScoped<IVenueService, VenueService>();

            // Repositories
            this._services.AddScoped<IUserRepository, UserRepository>();
            this._services.AddScoped<IAccountRequestRepository, AccountRequestRepository>();
            this._services.AddScoped<IMemberRepository, MemberRepository>();
            this._services.AddScoped<IEventTypeRepository, EventTypeRepository>();
            this._services.AddScoped<IVenueRepository, VenueRepository>();

            // Local, private venue-photo storage.
            this._services.AddScoped<IVenueImageStorage, VenueImageStorage>();

            // Current request actor for service-layer authorization and audit.
            this._services.AddScoped<ICurrentActor, HttpCurrentActor>();

            this._services.AddHttpClient();
        }
    }
}
