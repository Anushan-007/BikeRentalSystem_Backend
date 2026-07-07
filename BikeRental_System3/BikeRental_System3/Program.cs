using BikeRental_System3.AI.Documents;
using BikeRental_System3.AI.Interfaces;
using BikeRental_System3.AI.Services;
using BikeRental_System3.Data;
using BikeRental_System3.IRepository;
using BikeRental_System3.IService;
using BikeRental_System3.Models;
using BikeRental_System3.Repository;
using BikeRental_System3.Services;
using BikeRental_System3.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.SemanticKernel;
using System.Text;

namespace BikeRental_System3
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // Add services to the container.

            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter your JWT token"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                        Array.Empty<string>()
                    }
                });
            });

            // Register EmailConfig
            builder.Services.Configure<EmailConfig>(builder.Configuration.GetSection("EmailConfig"));

            // Register services
            builder.Services.AddScoped<sendmailService>();
            builder.Services.AddScoped<SendMailRepository>();
            builder.Services.AddScoped<EmailServiceProvider>();

            // builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddDbContext<AppDbContext>(options =>
                        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")),
             ServiceLifetime.Transient);

            // Ensure EmailConfig is available as a singleton if needed
            builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<EmailConfig>>().Value);

            builder.Services.AddScoped<IBikeRepository, BikeRepository>();
            builder.Services.AddScoped<IBikeService, BikeService>();

            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IUserService, UserService>();

            builder.Services.AddScoped<IRentalRequestRepository, RentalRequestRepository>();
            builder.Services.AddScoped<IRentalRequestService, RentalRequestService>();

            builder.Services.AddScoped<IRentalRecordRepository, RentalRecordRepository>();
            builder.Services.AddScoped<IRentalRecordService, RentalRecordService>();

            builder.Services.AddScoped<IBikeUnitRepository, BikeUnitRepository>();
            builder.Services.AddScoped<IBikeUnitService, BikeUnitService>();

            // ── Localization Services ─────────────────────────────────────────
            // MemoryCache: stores translation bundles for 30 min (Cache-Aside pattern)
            builder.Services.AddMemoryCache();
            builder.Services.AddScoped<ILocalizationService, LocalizationService>();

            // ── AI / Semantic Kernel Services (Phase 3) ───────────────────────
            //
            // 1. Semantic Kernel — one Kernel instance wired to GPT-4o-mini.
            //    AddOpenAIChatCompletion registers IChatCompletionService inside
            //    the Kernel so BikeRentalChatChain can resolve it via
            //    _kernel.GetRequiredService<IChatCompletionService>().
            var openAiApiKey = builder.Configuration["OpenAI:ApiKey"]
                ?? throw new InvalidOperationException("OpenAI:ApiKey is missing from appsettings.json");
            var openAiModel = builder.Configuration["OpenAI:Model"] ?? "gpt-4o-mini";

            builder.Services.AddSingleton(sp =>
            {
                var kernelBuilder = Kernel.CreateBuilder();
                kernelBuilder.AddOpenAIChatCompletion(openAiModel, openAiApiKey);
                return kernelBuilder.Build();
            });

            // 2. Prompt template — reads AI/Prompts/bike-rental-system.txt at startup.
            //    Singleton: the file is loaded once; RenderSystemPrompt() is stateless.
            builder.Services.AddSingleton<IPromptTemplateService, BikeRentalPromptTemplate>();

            // 3. Context providers — supply the {{context}} injected into the system prompt.
            //
            //    PdfDocumentLoader: iText7-based PDF reader (IDocumentLoader).
            //    DatabaseContextProvider: queries live bike inventory from SQL Server.
            //    PdfContextProvider: loads FAQ.pdf + Terms.pdf once and caches them.
            //    CompositeContextProvider: runs both in parallel, combines results.
            //
            //    DatabaseContextProvider and PdfContextProvider are registered by their
            //    CONCRETE TYPE (not as IContextProvider) so CompositeContextProvider can
            //    inject them directly without circular dependency.
            //    Only CompositeContextProvider is registered as IContextProvider —
            //    that is what BikeRentalChatChain resolves.
            builder.Services.AddSingleton<IDocumentLoader, PdfDocumentLoader>();
            builder.Services.AddSingleton<DatabaseContextProvider>();
            builder.Services.AddSingleton<PdfContextProvider>();
            builder.Services.AddSingleton<IContextProvider, CompositeContextProvider>();

            // 4. Chat chain — the SK pipeline (Prompt → GPT → Response).
            //    Singleton: all dependencies (Kernel, IPromptTemplateService, IContextProvider)
            //    are themselves Singletons, so no lifetime mismatch.
            builder.Services.AddSingleton<IChatChainService, BikeRentalChatChain>();

            // 5. Conversation memory — stores per-session chat history in memory.
            //    Singleton: ConcurrentDictionary must survive across HTTP requests.
            builder.Services.AddSingleton<IConversationMemoryService, ConversationMemoryService>();

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]));
            builder.Services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                  {
                      options.MapInboundClaims = false;
                      options.TokenValidationParameters = new TokenValidationParameters
                      {
                          ValidateIssuerSigningKey = true,
                          IssuerSigningKey = key,
                          ValidateIssuer = true,
                          ValidIssuer = builder.Configuration["Jwt:Issuer"],
                          ValidateAudience = true,
                          ValidAudience = builder.Configuration["Jwt:Audience"],
                          ValidateLifetime = true,
                          ClockSkew = TimeSpan.Zero
                      };
                  });

            builder.Services.AddAuthorization();



            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: "CORSOpenPolicy",
                                  policy =>
                                  {
                                      policy.WithOrigins("*").AllowAnyHeader().AllowAnyMethod();

                                  });
            });




            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("CORSOpenPolicy");
            app.UseHttpsRedirection();

            app.UseStaticFiles();  // Ensure static files are served from wwwroot

            app.UseRouting();

            app.UseAuthentication();  // Add this before UseAuthorization
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapControllers();


            app.Run();
        }
    }
}
