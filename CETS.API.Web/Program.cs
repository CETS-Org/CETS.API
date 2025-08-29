using Application.Implementations;
using Application.Interfaces;
using Domain.Data;
using Domain.Interfaces;
using Domain.Interfaces.CORE;
using Domain.Interfaces.IDN;
using Infrastructure.Repositories;
using Infrastructure.Repositories.CORE;
using Infrastructure.Repositories.IDN;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

using System.Text;

namespace WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<AppDbContext>(opts =>
                opts.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerDb")));

            builder.Services.AddScoped<IMessageService, MessageService>();
            builder.Services.AddScoped<IIDN_AccountService, IDN_AccountService>();

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IIDN_AccountRepository, IDN_AccountRepository>();
            builder.Services.AddScoped<ICORE_LookUpRepository, CORE_LookUpRepository>();


            builder.Services.AddHttpContextAccessor();

            builder.Services.AddAuthentication("Bearer")
           .AddJwtBearer("Bearer", options =>
           {
               var jwt = builder.Configuration.GetSection("Jwt");
               options.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateIssuer = true,
                   ValidateAudience = true,
                   ValidateIssuerSigningKey = true,
                   ValidIssuer = jwt["Issuer"],
                   ValidAudience = jwt["Audience"],
                   IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]))
               };
           });
            var rabbitMqSettings = builder.Configuration.GetSection("RabbitMq");

            if (builder.Configuration.GetValue("Features:UseRabbitMq", false))
            {
                builder.Services.AddMassTransit(x =>
                {
                    x.UsingRabbitMq((context, cfg) =>
                    {
                        cfg.Host(
                            rabbitMqSettings["Host"],
                            rabbitMqSettings["VirtualHost"],
                            h =>
                            {
                                h.Username(rabbitMqSettings["Username"]);
                                h.Password(rabbitMqSettings["Password"]);
                            });
                    });
                });
            }

            if (builder.Configuration.GetValue("Features:UseMongo", false))
            {
                builder.Services.AddSingleton<IMongoClient>(sp =>
                {
                    var mongoConnection = builder.Configuration.GetConnectionString("MongoDb");
                    var settings = MongoClientSettings.FromConnectionString(mongoConnection);
                    return new MongoClient(settings);
                });
            }

            // Register AutoMapper
            builder.Services.AddAutoMapper(typeof(Application.Mappers.CORE_LookUpProfile));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
