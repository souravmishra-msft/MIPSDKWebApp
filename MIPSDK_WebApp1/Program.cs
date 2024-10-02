using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using MIPSDK_WebApp1.Data;
using MIPSDK_WebApp1.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Fetch and parse the scopes from the configuration
string? scopesString = builder.Configuration["MicrosoftGraph:Scopes"];
string[] initialScopes = string.IsNullOrWhiteSpace(scopesString) ? Array.Empty<string>() : scopesString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration, "AzureAd")
    .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
    .AddMicrosoftGraph(builder.Configuration.GetSection("MicrosoftGraph"))
    .AddInMemoryTokenCaches();

builder.Services.AddRazorPages().AddMvcOptions(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
}).AddMicrosoftIdentityUI();

// Add services to the container.
builder.Services.AddControllersWithViews();

// Inject ApplicationDBContext into the services container
builder.Services.AddDbContext<ApplicationDBContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("EmployeesDB"));
});


// Register HttpClient
builder.Services.AddHttpClient();

// Register the AuthService
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<GraphService>();
builder.Services.AddScoped<MipService>();
builder.Services.AddScoped<IExportFileService, ExportFileService>();

WebApplication app = builder.Build();

// Apply pending migrations at startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
