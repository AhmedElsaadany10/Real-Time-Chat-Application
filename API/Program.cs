using System.Text;
using API.Data;
using API.Helpers;
using API.Interfaces;
using API.Middleware;
using API.Models;
using API.Services;
using API.SignalR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});
//builder.Services.AddControllers();
builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
});
builder.Services.AddCors();
builder.Services.AddSignalR();
builder.Services.AddDbContext<AppDbContext>(Options => {
    Options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddIdentityCore<AppUser>(Options=>{
    Options.Password.RequireNonAlphanumeric=false;
    Options.Password.RequiredLength=3;
}).AddRoles<AppRole>()
.AddRoleManager<RoleManager<AppRole>>()
.AddSignInManager<SignInManager<AppUser>>()
.AddRoleValidator<RoleValidator<AppRole>>()
.AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme
).AddJwtBearer(Options=>{
                    Options.TokenValidationParameters=new TokenValidationParameters{
                        ValidateIssuer=false,
                        ValidateAudience=false,
                        ValidateIssuerSigningKey=true,
                        IssuerSigningKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SigningKey"])),
                    };
                    Options.Events=new JwtBearerEvents{
                        OnMessageReceived=context=>{
                            var accessToken=context.Request.Query["access_token"];
                            var path=context.HttpContext.Request.Path;
                            if(!string.IsNullOrEmpty(accessToken)&&path.StartsWithSegments("/hubs")){
                                context.Token=accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });
                //roles 
                builder.Services.AddAuthorization(Options=>{
                    Options.AddPolicy("AdminRole",policy=>policy.RequireRole(Roles.Admin.ToString()));
                    Options.AddPolicy("ModeratorRole",policy=>policy.RequireRole(Roles.Admin.ToString(),Roles.Moderator.ToString()));
                });
                
builder.Services.AddSingleton<PresencState>();
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.AddScoped<ITokenService,TokenService>();
builder.Services.AddScoped<IPhotoService,PhotoService>();
builder.Services.AddScoped<LogUserActivity>();
builder.Services.AddScoped<IMessageRepository,MessageRepository>();
builder.Services.AddScoped<ILikesRepository,LikesRepository>();
builder.Services.AddScoped<IUserRepository,UserRepository>();
builder.Services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
}
app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors(x=>x.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins("http://localhost:4200"));
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<PresenceHub>("hubs/presence");
app.MapHub<MessageHub>("hubs/message");
//seed data
using var scope =app.Services.CreateScope();
var services=scope.ServiceProvider;
var logger=services.GetService<ILogger<Program>>();
try{
    var context=services.GetRequiredService<AppDbContext>();
    var userManager=services.GetRequiredService<UserManager<AppUser>>();
    var roleManager=services.GetRequiredService<RoleManager<AppRole>>();
    await context.Database.MigrateAsync();
    await SeedData.SeedUsers(userManager,roleManager);
}
catch(Exception ex){
    logger.LogError(ex,"an error accurd");
}
//end seedin data

app.Run();

// record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
// {
//     public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
// }
