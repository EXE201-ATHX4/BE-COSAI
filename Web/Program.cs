
using Contract.Repositories.Entity;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Repositories.Base;
using Services.Mappings;
using Services.Service;
using StackExchange.Redis;
using System.Text;

namespace Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DotNetEnv.Env.Load();
            //var base64 = Environment.GetEnvironmentVariable("FIREBASE_KEY_JSON");


            var builder = WebApplication.CreateBuilder(args);
            var firebase = builder.Configuration["FIREBASE_KEY_JSON"];
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(firebase));
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromJson(json)
            });
            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddConfig(builder.Configuration);
            builder.Services.Configure<JsonOptions>(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = null;
                options.JsonSerializerOptions.MaxDepth = 64;
            });
            builder.Services.AddIdentity<User, ApplicationRole>()
                .AddEntityFrameworkStores<ComesticsSalesDBContext>()
                .AddDefaultTokenProviders();
            // Configure AutoMapper
            builder.Services.AddAutoMapper(typeof(MappingProfile));
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            });
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

            }).AddJwtBearer(options =>
                {

                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnChallenge = context =>
                        {
                            context.HandleResponse();
                            context.Response.StatusCode = 401;
                            context.Response.ContentType = "application/json";
                            return context.Response.WriteAsync(Newtonsoft.Json.JsonConvert.SerializeObject(new
                            {
                                error = "Unauthorized",
                                error_description = "You need to provide a valid token to access this resource."
                            }));
                        }
                    };
                });
            builder.Services.AddSingleton<IConnectionMultiplexer>(opts =>
            {
                var options = new ConfigurationOptions
                {
                    EndPoints = { builder.Configuration["Redis:Server"]! },
                    User = builder.Configuration["Redis:User"],
                    Password = builder.Configuration["Redis:Password"],
                    Ssl = true,
                    AbortOnConnectFail = false,
                    ConnectRetry = 3,
                    ConnectTimeout = 10000,
                    KeepAlive = 30,
                    SyncTimeout = 10000,
                };
                return ConnectionMultiplexer.Connect(options);
            });
            builder.Services.AddSwaggerGen(c =>
            {
                //c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });
                c.EnableAnnotations();
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter JWT with Bearer prefix in the field",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });

            });
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseCors(policy =>
                policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
