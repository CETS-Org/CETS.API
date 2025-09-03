using Application.Implementations;
using Application.Implementations.CORE;
using Application.Implementations.FIN;
using Application.Implementations.IDN;
using Application.Implementations.COM;
using Application.Interfaces;
using Application.Interfaces.CORE;
using Application.Interfaces.FIN;
using Application.Interfaces.IDN;
using Application.Interfaces.COM;
using Domain.Data;
using Domain.Interfaces;
using Domain.Interfaces.CORE;
using Domain.Interfaces.FIN;
using Domain.Interfaces.IDN;
using Domain.Interfaces.COM;
using Infrastructure.Repositories;
using Infrastructure.Repositories.CORE;
using Infrastructure.Repositories.FIN;
using Infrastructure.Repositories.IDN;
using Infrastructure.Repositories.COM;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

using System.Text;
using Utils.Helpers;

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
            builder.Services.AddScoped<IIDN_StudentService, IDN_StudentService>();
            builder.Services.AddScoped<IIDN_TeacherService, IDN_TeacherService>();
            builder.Services.AddScoped<IIDN_TeacherCredentialService, IDN_TeacherCredentialService>();
            builder.Services.AddScoped<IIDN_RoleService, IDN_RoleService>();
            builder.Services.AddScoped<IIDN_AccountRoleService, IDN_AccountRoleService>();
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
            builder.Services.AddScoped<ICORE_LookUpService, CORE_LookUpService>();
            builder.Services.AddScoped<ICORE_LookUpTypeService, CORE_LookUpTypeService>();
            builder.Services.AddScoped<IFIN_InvoiceService, FIN_InvoiceService>();
            builder.Services.AddScoped<IFIN_InvoiceItemService, FIN_InvoiceItemService>();
            builder.Services.AddScoped<IFIN_PaymentService, FIN_PaymentService>();
            builder.Services.AddScoped<IFIN_PaymentRefundService, FIN_PaymentRefundService>();
            builder.Services.AddScoped<IFIN_PaymentWebhookService, FIN_PaymentWebhookService>();
            builder.Services.AddScoped<IFIN_PromotionService, FIN_PromotionService>();
            builder.Services.AddScoped<ICOM_NotificationService, COM_NotificationService>();
            builder.Services.AddScoped<ICOM_FeedbackService, COM_FeedbackService>();
            builder.Services.AddScoped<ICOM_FeedbackRecordService, COM_FeedbackRecordService>();
            builder.Services.AddScoped<ICOM_ConversationService, COM_ConversationService>();

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IIDN_AccountRepository, IDN_AccountRepository>();
            builder.Services.AddScoped<IIDN_StudentRepository, IDN_StudentRepository>();
            builder.Services.AddScoped<IIDN_TeacherRepository, IDN_TeacherRepository>();
            builder.Services.AddScoped<IIDN_TeacherCredentialRepository, IDN_TeacherCredentialRepository>();
            builder.Services.AddScoped<IIDN_RoleRepository, IDN_RoleRepository>();
            builder.Services.AddScoped<IIDN_AccountRoleRepository, IDN_AccountRoleRepository>();
            builder.Services.AddScoped<ICORE_LookUpRepository, CORE_LookUpRepository>();
            builder.Services.AddScoped<ICORE_LookUpTypeRepository, CORE_LookUpTypeRepository>();
            builder.Services.AddScoped<IFIN_InvoiceRepository, FIN_InvoiceRepository>();
            builder.Services.AddScoped<IFIN_InvoiceItemRepository, FIN_InvoiceItemRepository>();
            builder.Services.AddScoped<IFIN_PaymentRepository, FIN_PaymentRepository>();
            builder.Services.AddScoped<IFIN_PaymentRefundRepository, FIN_PaymentRefundRepository>();
            builder.Services.AddScoped<IFIN_PaymentWebhookRepository, FIN_PaymentWebhookRepository>();
            builder.Services.AddScoped<IFIN_PromotionRepository, FIN_PromotionRepository>();
            builder.Services.AddScoped<ICOM_NotificationRepository, COM_NotificationRepository>();
            builder.Services.AddScoped<ICOM_FeedbackRepository, COM_FeedbackRepository>();
            builder.Services.AddScoped<ICOM_FeedbackRecordRepository, COM_FeedbackRecordRepository>();
            builder.Services.AddScoped<ICOM_ConversationRepository, COM_ConversationRepository>();

            builder.Services.AddScoped<IdGenerator>();




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
