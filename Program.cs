using System.Text;
using KnowledgeFlowApi.Data;
using KnowledgeFlowApi.Handlers;
using KnowledgeFlowApi.Options;
using KnowledgeFlowApi.Services.CommentServices;
using KnowledgeFlowApi.Services.FileItemServices;
using KnowledgeFlowApi.Services.UserServices;
using LibraryManagementSystemAPI.Services.SendEmailServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

/*
 *  TODOs:  
 *  complete comments feature:
        testing
        add update comment feature
        merging with master branch
 *  
 *  likinga and disliking feature
 *  add report feature
 *  
 *  
 *
 *  future features:
 *  
 *  update user password
 *  Implement virus scanning
 *  Consider implementing file compression
*/

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region database connection
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("Default")
));
#endregion

#region options
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("JwtOptions"));
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("EmailOptions"));
#endregion

#region dependency injection
builder.Services.AddControllers();
builder.Services.AddSingleton<FileHandler>();
builder.Services.AddScoped<FileService>();
builder.Services.AddScoped<AuthUserService>();
builder.Services.AddScoped<SendEmailService>();
builder.Services.AddScoped<UserProfileService>();
builder.Services.AddScoped<CommentService>();
builder.Services.AddScoped<RatingService>();
builder.Services.AddSignalR();

#endregion

#region  Cors
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.WithOrigins("*").AllowAnyMethod().AllowAnyHeader(); ;
        });
});
#endregion


#region Jwt Auth
var jwtOptions = builder.Configuration.GetSection("JwtOptions").Get<JwtOptions>();

object value = builder.Services.AddAuthentication()
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options => 
    {
        options.SaveToken = true;   // to allow getting token string from HttpContext
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ClockSkew = TimeSpan.Zero,  // to expire token directly after finishing its determined duration
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey))
        };
    });
#endregion


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseStaticFiles();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapHub<CommentsHub>("/hubs/commentsHub");

app.UseHttpsRedirection();

app.Run();