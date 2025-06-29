using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartApply.Components;
using SmartApply.Components.Account;
using SmartApply.Data;
using SmartApply.Services;
using SmartApply.Models;
using SmartApply.Services.Crawling;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddScoped<IJobSource, StepStoneSource>();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

// GPT-Service
builder.Services.AddHttpClient<ApplicationService>();
builder.Services.AddScoped<ApplicationService>();
builder.Services.AddSingleton<BrowserHtmlLoader>();
builder.Services.AddScoped<IJobSource, StepStoneSource>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAdditionalIdentityEndpoints();
app.MapGet("/api/jobs/test", async (IJobSource source) =>
{
    var jobs = await source.SearchJobsAsync("Werkstudent Informatik", "Berlin", false);
Console.WriteLine($"Gefundene Jobs: {jobs.Count}");
foreach (var job in jobs)
{
    Console.WriteLine($"➡️ {job.Title} | {job.Company} | {job.Url}");
}

    return Results.Ok(new { count = jobs.Count, jobs });
});

app.MapGet("/api/jobs/live", async (IJobSource source) =>
{
    var jobs = await source.SearchJobsAsync("Werkstudent Informatik", "Berlin", false);
    return Results.Ok(jobs);
});

app.MapPost("/api/gpt/generate", async (JobPosting job, ApplicationService gpt) =>
{
    var result = await gpt.GenerateCoverLetterAsync(job);
    return Results.Ok(result);
});
app.MapPost("/api/ping", () => Results.Ok("pong"));
app.Run();
