
using MassTransit;
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
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();




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


            builder.Services.AddSingleton<IMongoClient>(sp =>
            {
                var mongoConnection = builder.Configuration.GetConnectionString("MongoDb");
                var settings = MongoClientSettings.FromConnectionString(mongoConnection);
                return new MongoClient(settings);
            });


            // Register AutoMapper
            builder.Services.AddAutoMapper(typeof(Program));

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
