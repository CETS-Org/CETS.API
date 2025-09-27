using Amazon.S3;
using Application.Implementations;
using Application.Implementations.ACAD;
using Application.Implementations.COM;
using Application.Implementations.CORE;
using Application.Implementations.EVT;
using Application.Implementations.FAC;
using Application.Implementations.FIN;
using Application.Implementations.HR;
using Application.Implementations.IDN;
using Application.Implementations.RPT;
using Application.Interfaces;
using Application.Interfaces.ACAD;
using Application.Interfaces.COM;
using Application.Interfaces.Common.Storage;
using Application.Interfaces.CORE;
using Application.Interfaces.EVT;
using Application.Interfaces.ExternalServices.Email;
using Application.Interfaces.ExternalServices.Security;
using Application.Interfaces.FAC;
using Application.Interfaces.FIN;
using Application.Interfaces.HR;
using Application.Interfaces.IDN;
using Application.Interfaces.RPT;
using Domain.Data;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Interfaces.ACAD;
using Domain.Interfaces.COM;
using Domain.Interfaces.CORE;
using Domain.Interfaces.EVT;
using Domain.Interfaces.FAC;
using Domain.Interfaces.FIN;
using Domain.Interfaces.HR;
using Domain.Interfaces.IDN;
using Domain.Interfaces.RPT;
using DTOs.ACAD.ACAD_ClassReservation.Responses;
using Infrastructure.Implementations.Common.Storage;
using Infrastructure.Implementations.Repositories;
using Infrastructure.Implementations.Repositories.ACAD;
using Infrastructure.Implementations.Repositories.COM;
using Infrastructure.Implementations.Repositories.CORE;
using Infrastructure.Implementations.Repositories.EVT;
using Infrastructure.Implementations.Repositories.FAC;
using Infrastructure.Implementations.Repositories.FIN;
using Infrastructure.Implementations.Repositories.HR;
using Infrastructure.Implementations.Repositories.IDN;
using Infrastructure.Implementations.Repositories.RPT;
using Infrastructure.Implementations.Services.Email;
using Infrastructure.Implementations.Services.Security;
using MassTransit;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.ModelBuilder;
using MongoDB.Driver;
using System.Diagnostics;
using System.Net;
using System.Reflection.Emit;
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
            var modelbuilder = new ODataConventionModelBuilder();
            modelbuilder.EntitySet<ACAD_Course>("ACAD_Course");
            modelbuilder.EntitySet<ACAD_CoursePackage>("ACAD_CoursePackage");
            modelbuilder.EntitySet<ACAD_CoursePackageItem>("ACAD_CoursePackageItem");
            modelbuilder.EntitySet<IDN_Account>("IDN_Accounts");
            modelbuilder.EntitySet<ClassReservationResponse>("ACAD_ClassReservations");

            builder.Services.AddControllers().AddOData(opt =>
                opt.AddRouteComponents("odata", modelbuilder.GetEdmModel())
                    .Select().Filter().Expand().Count().OrderBy().SetMaxTop(100));
            ;
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<AppDbContext>(opts =>
                opts.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerDb")));

            // Configure R2 File Storage
            builder.Services.Configure<CloudflareR2Settings>(builder.Configuration.GetSection("CloudflareR2"));


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
            builder.Services.AddScoped<IEVT_EventService, EVT_EventService>();
            builder.Services.AddScoped<IEVT_EventFeedbackService, EVT_EventFeedbackService>();
            builder.Services.AddScoped<IEVT_EventRegistrationService, EVT_EventRegistrationService>();
            builder.Services.AddScoped<IFAC_RoomService, FAC_RoomService>();
            builder.Services.AddScoped<IHR_ContractService, HR_ContractService>();
            builder.Services.AddScoped<IHR_TeacherAvailabilityService, HR_TeacherAvailabilityService>();
            builder.Services.AddScoped<IRPT_ReportService, RPT_ReportService>();
            builder.Services.AddScoped<ICOM_NotificationService, COM_NotificationService>();
            builder.Services.AddScoped<ICOM_FeedbackService, COM_FeedbackService>();
            builder.Services.AddScoped<ICOM_FeedbackRecordService, COM_FeedbackRecordService>();
            builder.Services.AddScoped<ICOM_ConversationService, COM_ConversationService>();
            builder.Services.AddScoped<IACAD_CourseService, ACAD_CourseService>();
            builder.Services.AddScoped<IACAD_CourseBenefitService, ACAD_CourseBenefitService>();
            builder.Services.AddScoped<IACAD_CourseRequirementService, ACAD_CourseRequirementService>();
            builder.Services.AddScoped<IACAD_CourseSkillService, ACAD_CourseSkillService>();
            builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();  
            builder.Services.AddScoped<IJwtService, JwtService>();
            builder.Services.AddScoped<IACAD_EnrollmentService, ACAD_EnrollmentService>();
            builder.Services.AddScoped<IACAD_CourseTeacherAssignmentService, ACAD_CourseTeacherAssignmentService>();
            builder.Services.AddScoped<IACAD_AttendanceService, AttendanceService>();
            builder.Services.AddScoped<IACAD_SubmissionService, ACAD_SubmissionService>();
            builder.Services.AddScoped<IACAD_LearningMaterialService, ACAD_LearningMaterialService>();
            builder.Services.AddScoped<IACAD_CourseScheduleService, ACAD_CourseScheduleService>();
            builder.Services.AddScoped<IACAD_CoursePackageService, ACAD_CoursePackageService>();
            builder.Services.AddScoped<IMailService, MailService>();
            builder.Services.AddScoped<IACAD_ClassReservationService, ACAD_ClassReservationService>();
            builder.Services.AddScoped<IACAD_ReservationItemService, ACAD_ReservationItemService>();
            
            builder.Services.AddScoped<IACAD_ClassMeetingsService, ACAD_ClassMeetingsService>();





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
            builder.Services.AddScoped<IEVT_EventRepository, EVT_EventRepository>();
            builder.Services.AddScoped<IEVT_EventFeedbackRepository, EVT_EventFeedbackRepository>();
            builder.Services.AddScoped<IEVT_EventRegistrationRepository, EVT_EventRegistrationRepository>();
            builder.Services.AddScoped<IFAC_RoomRepository, FAC_RoomRepository>();
            builder.Services.AddScoped<IHR_ContractRepository, HR_ContractRepository>();
            builder.Services.AddScoped<IHR_TeacherAvailabilityRepository, HR_TeacherAvailabilityRepository>();
            builder.Services.AddScoped<IRPT_ReportRepository, RPT_ReportRepository>();
            builder.Services.AddScoped<ICOM_NotificationRepository, COM_NotificationRepository>();
            builder.Services.AddScoped<ICOM_FeedbackRepository, COM_FeedbackRepository>();
            builder.Services.AddScoped<ICOM_FeedbackRecordRepository, COM_FeedbackRecordRepository>();
            builder.Services.AddScoped<ICOM_ConversationRepository, COM_ConversationRepository>();
            builder.Services.AddScoped<IACAD_CourseRepository, ACAD_CourseRepository>();
            builder.Services.AddScoped<IACAD_CourseBenefitRepository, ACAD_CourseBenefitRepository>();
            builder.Services.AddScoped<IACAD_CourseRequirementRepository, ACAD_CourseRequirementRepository>();
            builder.Services.AddScoped<IACAD_CourseSkillRepository, ACAD_CourseSkillRepository>();
            builder.Services.AddScoped<IACAD_EnrollmentRepository, ACAD_EnrollmentRepository>();
            builder.Services.AddScoped<IACAD_CourseTeacherAssignmentRepository, ACAD_CourseTeacherAssignmentRepository>();
            builder.Services.AddScoped<IACAD_AttendanceRepository, ACAD_AttendanceRepository>();
            builder.Services.AddScoped<IACAD_SubmissionRepository, ACAD_SubmissionRepository>();
            builder.Services.AddScoped<IACAD_LearningMaterialRepository, ACAD_LearningMaterialRepository>();
            builder.Services.AddScoped<IFileStorageService, R2FileStorageService>();
            builder.Services.AddScoped<IACAD_ClassMeetingRepository, ACAD_ClassMeetingRepository>();
            builder.Services.AddScoped<IACAD_ReservationItemRepository, ACAD_ReservationItemRepository>();
            builder.Services.AddScoped<IACAD_CourseScheduleRepository, ACAD_CourseScheduleRepository>();
            builder.Services.AddScoped<IACAD_CoursePackageRepository, ACAD_CoursePackageRepository>();
            builder.Services.AddScoped<IACAD_CoursePackageItemRepository, ACAD_CoursePackageItemRepository>();

            builder.Services.AddScoped<IACAD_ReservationItemRepository, ACAD_ReservationItemRepository>();
            builder.Services.AddScoped<IACAD_ClassReservationRepository, ACAD_ClassReservationRepository>();
            


            builder.Services.AddScoped<IdGenerator>();



            // Configure CORS
            var allowedOrigins = builder.Configuration
              .GetSection("AllowedCorsOrigins")
              .Get<string[]>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("ApiCors", p => p
                    .WithOrigins(allowedOrigins!)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
            });



            // Configure Authentication
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

            // Configure RabbitMQ with MassTransit
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
            builder.Services.AddAutoMapper(typeof(Application.Mappers.CORE.CORE_LookUpProfile));

          

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseCors("ApiCors");
            app.UseHttpsRedirection();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
