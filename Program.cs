using dotnetapp.Data;
using dotnetapp.Models;
using dotnetapp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<PlantService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();


//added db context to container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var cs = builder.Configuration.GetConnectionString("DataBaseConnectionString");
    var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
    var finalConnString = cs.Replace("{DB_PASSWORD}", dbPassword);
    
    options.UseNpgsql(finalConnString);

});
 
builder.Services.AddIdentity<ApplicationUser,IdentityRole>() // Register identity service
    .AddEntityFrameworkStores<ApplicationDbContext>() // Link Identity to database
    .AddDefaultTokenProviders(); 


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    policy.AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowAnyMethod()
    );
});


builder.Services.AddAuthentication(options =>{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false; // if in production can be stet to true
    options.TokenValidationParameters = new TokenValidationParameters(){
        //1. Validating the secret key
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"])),

        //2 valide the one who created the token
        ValidateIssuer = false,
        // ValidIssuer = builder.Configuration["JWT:ValidIssuer"],

        //3 validate the audience for whihc this token is for
        // frontned
        ValidateAudience = false, // till frontend is not ready
       // ValidAudience = builder.Configuration["JWT:ValidAudience"],

        //4 validate tokne life -> of token is expired -> to check weather the token is expired or not
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero // no clock sket tolerance
    };
});

builder.Services.AddAuthorization();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
//app.UseHttpsRedirection(); // use only when using Https redirection and is also present in appsetting.json application url
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
