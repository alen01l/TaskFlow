using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// EF Core + SQLite
var cs = builder.Configuration.GetConnectionString("dbContext") ?? "Data Source=taskflow.db";
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(cs));

// Identity (cookie auth)
builder.Services.AddIdentityCore<AppUser>(o =>
    {
        o.Password.RequiredLength = 6;
        o.Password.RequireDigit = true;
        o.Password.RequireNonAlphanumeric = false;
        o.Password.RequireUppercase = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager();

builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddCookie(IdentityConstants.ApplicationScheme, o =>
    {
        o.Cookie.Name = "taskflow.auth";
        o.SlidingExpiration = true;
        o.Cookie.HttpOnly = true;
        o.Cookie.SameSite = SameSiteMode.None;
        o.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = ctx =>
            {
                if (ctx.Request.Path.StartsWithSegments("/api"))
                {
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                }

                ctx.Response.Redirect(ctx.RedirectUri);
                return Task.CompletedTask;
            },
            OnRedirectToAccessDenied = ctx =>
            {
                if (ctx.Request.Path.StartsWithSegments("/api"))
                {
                    ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                }

                ctx.Response.Redirect(ctx.RedirectUri);
                return Task.CompletedTask;
            }
        };
    });



builder.Services.AddAuthorization();

// MVC + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "TaskFlow API", Version = "v1" });
});


const string Frontend = "Frontend";
builder.Services.AddCors(o =>
    o.AddPolicy(Frontend, p => p
        .WithOrigins("http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseHttpsRedirection();
app.UseCors(Frontend);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
    var email = "demo@taskflow.local";
    var user = await userMgr.FindByEmailAsync(email);
    if (user is null)
    {
        user = new AppUser { UserName = email, Email = email, EmailConfirmed = true };
        await userMgr.CreateAsync(user, "Pass123$");
    }
}

app.Run();
