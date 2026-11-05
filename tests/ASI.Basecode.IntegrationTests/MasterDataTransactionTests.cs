using ASI.Basecode.Data;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Models.Lookups;
using ASI.Basecode.Data.Repositories;
using ASI.Basecode.Resources;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.Services.Services;
using ASI.Basecode.WebApp.Controllers;
using ASI.Basecode.WebApp.Storage;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Claims;
using Xunit;

namespace ASI.Basecode.IntegrationTests;

public class MasterDataTransactionTests(SqlServerDatabaseFixture fixture) : IClassFixture<SqlServerDatabaseFixture>
{
    private static readonly IMapper Mapper = new MapperConfiguration(config =>
    {
        config.CreateMap<EventTypeViewModel, EventType>();
        config.CreateMap<EventType, EventTypeViewModel>();
        config.CreateMap<VenueViewModel, Venue>();
        config.CreateMap<Venue, VenueViewModel>();
    }).CreateMapper();

    private AsiBasecodeDBContext Context() => new(new DbContextOptionsBuilder<AsiBasecodeDBContext>().UseSqlServer(fixture.ConnectionString).Options);

    private static string UniqueName(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

    private static User NewUser() => new()
    {
        Name = "Master Data Actor",
        UserId = $"{Guid.NewGuid():N}@example.invalid",
        Password = "test",
        IsActive = true,
        RoleId = RoleCode.Admin
    };

    private static EventTypeViewModel EventTypeModel(string name) => new() { TypeName = name, Description = "Description" };

    private static VenueViewModel VenueModel(string roomName, string imagePath = "venues/test.jpg") => new()
    {
        RoomName = roomName,
        Capacity = 20,
        Location = "Main Building",
        ImagePath = imagePath
    };

    private static MemberViewModel MemberModel(string studentNumber) => new()
    {
        StudentNumber = studentNumber,
        Course = "Computing",
        YearLevel = 1,
        Section = "A",
        MobileNumber = "555-0100",
        MembershipType = "Regular",
        Status = "active"
    };

    private async Task<int> SeedUserAsync(CancellationToken cancellationToken)
    {
        await using var context = Context();
        var user = NewUser();
        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);
        return user.Id;
    }

    private async Task<(int ActorId, int LinkedUserId)> SeedUsersAsync(CancellationToken cancellationToken)
    {
        await using var context = Context();
        var actor = NewUser();
        var linked = NewUser();
        context.Users.AddRange(actor, linked);
        await context.SaveChangesAsync(cancellationToken);
        return (actor.Id, linked.Id);
    }

    [Fact]
    public async Task EventTypeWriteRollsBackWhenTheRepositorySavesAndThenFails()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var writer = Context();
        await using var observer = Context();
        var unitOfWork = new UnitOfWork(writer);
        var actorId = await SeedUserAsync(ct);
        var service = new EventTypeService(new EventTypeRepositoryThatSavesThenFails(new EventTypeRepository(unitOfWork), unitOfWork), unitOfWork, Mapper);
        var name = UniqueName("Type");

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddEventTypeAsync(EventTypeModel(name), actorId, ct));

        Assert.Empty(writer.ChangeTracker.Entries());
        Assert.False(await observer.EventTypes.AnyAsync(t => t.TypeName == name, ct));
    }

    [Fact]
    public async Task EventTypeWriteCommitsWithoutAnOuterTransaction()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var writer = Context();
        await using var observer = Context();
        var unitOfWork = new UnitOfWork(writer);
        var actorId = await SeedUserAsync(ct);
        var service = new EventTypeService(new EventTypeRepository(unitOfWork), unitOfWork, Mapper);
        var name = UniqueName("Type");

        await service.AddEventTypeAsync(EventTypeModel(name), actorId, ct);

        Assert.True(await observer.EventTypes.AnyAsync(t => t.TypeName == name, ct));
    }

    [Fact]
    public async Task EventTypeWriteJoinsAnOuterTransactionThatRollsBack()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var writer = Context();
        await using var observer = Context();
        var unitOfWork = new UnitOfWork(writer);
        var actorId = await SeedUserAsync(ct);
        var service = new EventTypeService(new EventTypeRepository(unitOfWork), unitOfWork, Mapper);
        var name = UniqueName("Type");

        await Assert.ThrowsAsync<InvalidOperationException>(() => unitOfWork.ExecuteInTransactionAsync(async inner =>
        {
            await service.AddEventTypeAsync(EventTypeModel(name), actorId, inner);
            throw new InvalidOperationException("The outer work failed.");
        }, ct));

        Assert.False(await observer.EventTypes.AnyAsync(t => t.TypeName == name, ct));
    }

    [Fact]
    public async Task VenueWriteRollsBackWhenTheRepositorySavesAndThenFails()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var writer = Context();
        await using var observer = Context();
        var unitOfWork = new UnitOfWork(writer);
        var actorId = await SeedUserAsync(ct);
        var service = new VenueService(new VenueRepositoryThatSavesThenFails(new VenueRepository(unitOfWork), unitOfWork), unitOfWork, Mapper);
        var roomName = UniqueName("Room");

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddVenueAsync(VenueModel(roomName), actorId, ct));

        Assert.Empty(writer.ChangeTracker.Entries());
        Assert.False(await observer.Venues.AnyAsync(v => v.RoomName == roomName, ct));
    }

    [Fact]
    public async Task VenueWriteCommitsWithoutAnOuterTransaction()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var writer = Context();
        await using var observer = Context();
        var unitOfWork = new UnitOfWork(writer);
        var actorId = await SeedUserAsync(ct);
        var service = new VenueService(new VenueRepository(unitOfWork), unitOfWork, Mapper);
        var roomName = UniqueName("Room");

        await service.AddVenueAsync(VenueModel(roomName), actorId, ct);

        Assert.True(await observer.Venues.AnyAsync(v => v.RoomName == roomName, ct));
    }

    [Fact]
    public async Task VenueWriteJoinsAnOuterTransactionThatRollsBack()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var writer = Context();
        await using var observer = Context();
        var unitOfWork = new UnitOfWork(writer);
        var actorId = await SeedUserAsync(ct);
        var service = new VenueService(new VenueRepository(unitOfWork), unitOfWork, Mapper);
        var roomName = UniqueName("Room");

        await Assert.ThrowsAsync<InvalidOperationException>(() => unitOfWork.ExecuteInTransactionAsync(async inner =>
        {
            await service.AddVenueAsync(VenueModel(roomName), actorId, inner);
            throw new InvalidOperationException("The outer work failed.");
        }, ct));

        Assert.False(await observer.Venues.AnyAsync(v => v.RoomName == roomName, ct));
    }

    [Fact]
    public async Task MemberWriteRollsBackWhenTheRepositorySavesAndThenFails()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var writer = Context();
        await using var observer = Context();
        var unitOfWork = new UnitOfWork(writer);
        var (actorId, linkedUserId) = await SeedUsersAsync(ct);
        var service = new MemberService(new MemberRepositoryThatSavesThenFails(new MemberRepository(unitOfWork), unitOfWork), unitOfWork, new TestCurrentActor(actorId));
        var studentNumber = UniqueName("SN");

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateMemberFromApprovedAccountRequestAsync(linkedUserId, MemberModel(studentNumber), actorId, ct));

        Assert.Empty(writer.ChangeTracker.Entries());
        Assert.False(await observer.Members.AnyAsync(m => m.StudentNumber == studentNumber, ct));
    }

    [Fact]
    public async Task MemberWriteCommitsWithoutAnOuterTransaction()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var writer = Context();
        await using var observer = Context();
        var unitOfWork = new UnitOfWork(writer);
        var (actorId, linkedUserId) = await SeedUsersAsync(ct);
        var service = new MemberService(new MemberRepository(unitOfWork), unitOfWork, new TestCurrentActor(actorId));
        var studentNumber = UniqueName("SN");

        await service.CreateMemberFromApprovedAccountRequestAsync(linkedUserId, MemberModel(studentNumber), actorId, ct);

        Assert.True(await observer.Members.AnyAsync(m => m.StudentNumber == studentNumber, ct));
    }

    [Fact]
    public async Task MemberWriteJoinsAnOuterTransactionThatRollsBack()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var writer = Context();
        await using var observer = Context();
        var unitOfWork = new UnitOfWork(writer);
        var (actorId, linkedUserId) = await SeedUsersAsync(ct);
        var service = new MemberService(new MemberRepository(unitOfWork), unitOfWork, new TestCurrentActor(actorId));
        var studentNumber = UniqueName("SN");

        await Assert.ThrowsAsync<InvalidOperationException>(() => unitOfWork.ExecuteInTransactionAsync(async inner =>
        {
            await service.CreateMemberFromApprovedAccountRequestAsync(linkedUserId, MemberModel(studentNumber), actorId, inner);
            throw new InvalidOperationException("The outer work failed.");
        }, ct));

        Assert.False(await observer.Members.AnyAsync(m => m.StudentNumber == studentNumber, ct));
    }

    [Fact]
    public async Task EventTypeWritesUseTheSharedOutcomeContract()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var writer = Context();
        await using var observer = Context();
        var unitOfWork = new UnitOfWork(writer);
        var actorId = await SeedUserAsync(ct);
        var service = new EventTypeService(new EventTypeRepository(unitOfWork), unitOfWork, Mapper);
        var name = UniqueName("Type");

        Assert.Equal(OutcomeCode.Forbidden, (await Assert.ThrowsAsync<DomainException>(() => service.AddEventTypeAsync(EventTypeModel(name), 0, ct))).Outcome);
        Assert.Equal(OutcomeCode.Invalid, (await Assert.ThrowsAsync<DomainException>(() => service.AddEventTypeAsync(EventTypeModel("   "), actorId, ct))).Outcome);
        Assert.Equal(OutcomeCode.Invalid, (await Assert.ThrowsAsync<DomainException>(() => service.AddEventTypeAsync(new EventTypeViewModel { TypeName = name, Description = "  " }, actorId, ct))).Outcome);
        Assert.Equal(OutcomeCode.NotFound, (await Assert.ThrowsAsync<DomainException>(() => service.UpdateEventTypeAsync(new EventTypeViewModel { TypeId = int.MaxValue, TypeName = name, Description = "Description" }, actorId, ct))).Outcome);
        Assert.Equal(OutcomeCode.NotFound, (await Assert.ThrowsAsync<DomainException>(() => service.ToggleActiveAsync(int.MaxValue, actorId, ct))).Outcome);
        Assert.False(await observer.EventTypes.AnyAsync(t => t.TypeName == name, ct));

        await service.AddEventTypeAsync(EventTypeModel(name), actorId, ct);
        var duplicate = await Assert.ThrowsAsync<DomainException>(() => service.AddEventTypeAsync(EventTypeModel(name), actorId, ct));
        Assert.Equal(OutcomeCode.Duplicate, duplicate.Outcome);
        Assert.Equal(1, await observer.EventTypes.CountAsync(t => t.TypeName == name, ct));
    }

    [Fact]
    public async Task VenueWritesUseTheSharedOutcomeContract()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var writer = Context();
        await using var observer = Context();
        var unitOfWork = new UnitOfWork(writer);
        var actorId = await SeedUserAsync(ct);
        var service = new VenueService(new VenueRepository(unitOfWork), unitOfWork, Mapper);
        var roomName = UniqueName("Room");

        Assert.Equal(OutcomeCode.Forbidden, (await Assert.ThrowsAsync<DomainException>(() => service.AddVenueAsync(VenueModel(roomName), 0, ct))).Outcome);
        var missingPhoto = await Assert.ThrowsAsync<DomainException>(() => service.AddVenueAsync(VenueModel(roomName, "  "), actorId, ct));
        Assert.Equal(OutcomeCode.Invalid, missingPhoto.Outcome);
        Assert.Equal(nameof(VenueViewModel.ImagePath), missingPhoto.Field);
        Assert.Equal(OutcomeCode.NotFound, (await Assert.ThrowsAsync<DomainException>(() => service.UpdateVenueAsync(new VenueViewModel { VenueId = int.MaxValue, RoomName = roomName, Capacity = 20, ImagePath = "venues/test.jpg" }, actorId, ct))).Outcome);
        Assert.Equal(OutcomeCode.NotFound, (await Assert.ThrowsAsync<DomainException>(() => service.ToggleActiveAsync(int.MaxValue, actorId, ct))).Outcome);
        Assert.False(await observer.Venues.AnyAsync(v => v.RoomName == roomName, ct));
    }

    [Fact]
    public async Task MemberWritesUseTheSharedOutcomeContract()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var writer = Context();
        await using var observer = Context();
        var unitOfWork = new UnitOfWork(writer);
        var (actorId, linkedUserId) = await SeedUsersAsync(ct);
        var service = new MemberService(new MemberRepository(unitOfWork), unitOfWork, new TestCurrentActor(actorId));
        var studentNumber = UniqueName("SN");

        Assert.Equal(OutcomeCode.Forbidden, (await Assert.ThrowsAsync<DomainException>(() => service.CreateMemberFromApprovedAccountRequestAsync(linkedUserId, MemberModel(studentNumber), 0, ct))).Outcome);
        Assert.Equal(OutcomeCode.Invalid, (await Assert.ThrowsAsync<DomainException>(() => service.CreateMemberFromApprovedAccountRequestAsync(0, MemberModel(studentNumber), actorId, ct))).Outcome);
        Assert.Equal(OutcomeCode.NotFound, (await Assert.ThrowsAsync<DomainException>(() => service.UpdateMemberAsync(new MemberViewModel { MemberId = int.MaxValue, StudentNumber = studentNumber, Course = "Computing", YearLevel = 1, Section = "A", Status = "active" }, actorId, ct))).Outcome);
        Assert.Equal(OutcomeCode.NotFound, (await Assert.ThrowsAsync<DomainException>(() => service.ToggleStatusAsync(int.MaxValue, actorId, ct))).Outcome);
        Assert.False(await observer.Members.AnyAsync(m => m.StudentNumber == studentNumber, ct));

        await service.CreateMemberFromApprovedAccountRequestAsync(linkedUserId, MemberModel(studentNumber), actorId, ct);
        var duplicateProfile = await Assert.ThrowsAsync<DomainException>(() => service.CreateMemberFromApprovedAccountRequestAsync(linkedUserId, MemberModel(UniqueName("SN")), actorId, ct));
        Assert.Equal(OutcomeCode.Duplicate, duplicateProfile.Outcome);

        var otherUserId = await SeedUserAsync(ct);
        var duplicateStudentNumber = await Assert.ThrowsAsync<DomainException>(() => service.CreateMemberFromApprovedAccountRequestAsync(otherUserId, MemberModel(studentNumber), actorId, ct));
        Assert.Equal(OutcomeCode.Duplicate, duplicateStudentNumber.Outcome);
        Assert.Equal(1, await observer.Members.CountAsync(m => m.StudentNumber == studentNumber, ct));
    }

    [Fact]
    public async Task VenueImageStorageRefusesBadPhotosWithFileFailure()
    {
        var root = Path.Combine(Path.GetTempPath(), "attendta-master-data", Guid.NewGuid().ToString("N"));
        var webRoot = Path.Combine(root, "wwwroot");
        Directory.CreateDirectory(root);
        Directory.CreateDirectory(webRoot);
        try
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["Common:SetupRoot"] = root })
                .Build();
            var storage = new VenueImageStorage(configuration, new TestWebHostEnvironment(webRoot));

            Assert.Equal(OutcomeCode.FileFailure, (await Assert.ThrowsAsync<DomainException>(() => storage.SaveAsync(null!))).Outcome);
            Assert.Equal(OutcomeCode.FileFailure, (await Assert.ThrowsAsync<DomainException>(() => storage.SaveAsync(Photo("photo.gif", [0x47, 0x49, 0x46, 0x38])))).Outcome);
            Assert.Equal(OutcomeCode.FileFailure, (await Assert.ThrowsAsync<DomainException>(() => storage.SaveAsync(Photo("photo.png", [0xFF, 0xD8, 0xFF])))).Outcome);
            Assert.False(Directory.Exists(Path.Combine(root, "uploads")));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task VenueCreateCleansUpTheSavedImageWhenTheServiceRefuses()
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "7") }, "test"))
        };
        var storage = new RecordingVenueImageStorage("venues/saved.jpg");
        var controller = new VenueController(
            new HttpContextAccessor { HttpContext = httpContext },
            NullLoggerFactory.Instance,
            new ConfigurationBuilder().Build(),
            Mapper,
            new RefusingVenueService(),
            storage)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext },
            TempData = new TempDataDictionary(httpContext, new NullTempDataProvider())
        };

        var result = await controller.Create(VenueModel("Room"), Photo("photo.png", [0x89, 0x50, 0x4E, 0x47]));

        Assert.IsType<ViewResult>(result);
        Assert.Equal("venues/saved.jpg", Assert.Single(storage.Deleted));
        Assert.Equal("Refused.", controller.TempData["ErrorMessage"]);
    }

    private static IFormFile Photo(string fileName, byte[] content) => new FormFile(new MemoryStream(content), 0, content.Length, "photo", fileName);

    private sealed class RecordingVenueImageStorage(string savedPath) : IVenueImageStorage
    {
        public List<string> Deleted { get; } = [];

        public Task<string> SaveAsync(IFormFile image) => Task.FromResult(savedPath);

        public void Delete(string relativePath) => Deleted.Add(relativePath);
    }

    private sealed class RefusingVenueService : IVenueService
    {
        public List<VenueViewModel> GetVenues() => [];

        public VenueViewModel GetVenue(int id) => null!;

        public Task AddVenueAsync(VenueViewModel model, int actorUserId, CancellationToken cancellationToken = default) =>
            throw new DomainException(OutcomeCode.Invalid, "Refused.");

        public Task UpdateVenueAsync(VenueViewModel model, int actorUserId, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task ToggleActiveAsync(int id, int actorUserId, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }

    private sealed class NullTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();

        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
        }
    }

    private sealed class TestCurrentActor(int userId) : ICurrentActor
    {
        public bool IsAuthenticated => true;
        public int UserId => userId;
        public string Role => "admin";
    }

    private sealed class TestWebHostEnvironment(string webRoot) : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "ASI.Basecode.IntegrationTests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = webRoot;
        public string EnvironmentName { get; set; } = "Development";
        public string ContentRootPath { get; set; } = webRoot;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    private sealed class EventTypeRepositoryThatSavesThenFails(IEventTypeRepository inner, IUnitOfWork unitOfWork) : IEventTypeRepository
    {
        public IQueryable<EventType> GetEventTypes() => inner.GetEventTypes();
        public EventType GetEventType(int typeId) => inner.GetEventType(typeId);
        public bool EventTypeExists(int typeId) => inner.EventTypeExists(typeId);
        public bool TypeNameExists(string typeName, int? excludeId = null) => inner.TypeNameExists(typeName, excludeId);
        public Task UpdateEventTypeAsync(EventType eventType, CancellationToken cancellationToken = default) => inner.UpdateEventTypeAsync(eventType, cancellationToken);

        public async Task AddEventTypeAsync(EventType eventType, CancellationToken cancellationToken = default)
        {
            await inner.AddEventTypeAsync(eventType, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            throw new InvalidOperationException("The repository failed after saving.");
        }
    }

    private sealed class VenueRepositoryThatSavesThenFails(IVenueRepository inner, IUnitOfWork unitOfWork) : IVenueRepository
    {
        public IQueryable<Venue> GetVenues() => inner.GetVenues();
        public Venue GetVenue(int venueId) => inner.GetVenue(venueId);
        public bool VenueExists(int venueId) => inner.VenueExists(venueId);
        public Task UpdateVenueAsync(Venue venue, CancellationToken cancellationToken = default) => inner.UpdateVenueAsync(venue, cancellationToken);

        public async Task AddVenueAsync(Venue venue, CancellationToken cancellationToken = default)
        {
            await inner.AddVenueAsync(venue, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            throw new InvalidOperationException("The repository failed after saving.");
        }
    }

    private sealed class MemberRepositoryThatSavesThenFails(IMemberRepository inner, IUnitOfWork unitOfWork) : IMemberRepository
    {
        public IQueryable<MemberRecord> GetMemberRecords() => inner.GetMemberRecords();
        public MemberRecord GetMemberRecord(int memberId) => inner.GetMemberRecord(memberId);
        public Task<MemberRecord> GetMemberRecordByUserIdAsync(int userId, CancellationToken cancellationToken = default) => inner.GetMemberRecordByUserIdAsync(userId, cancellationToken);
        public Member GetMember(int memberId) => inner.GetMember(memberId);
        public bool UserHasMemberProfile(int userId) => inner.UserHasMemberProfile(userId);
        public bool StudentNumberExists(string studentNumber, int? excludeId = null) => inner.StudentNumberExists(studentNumber, excludeId);
        public Task UpdateMemberAsync(Member member, CancellationToken cancellationToken = default) => inner.UpdateMemberAsync(member, cancellationToken);

        public async Task AddMemberAsync(Member member, CancellationToken cancellationToken = default)
        {
            await inner.AddMemberAsync(member, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            throw new InvalidOperationException("The repository failed after saving.");
        }
    }
}
