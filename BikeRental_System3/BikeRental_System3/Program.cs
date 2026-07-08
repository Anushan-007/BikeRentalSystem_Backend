using BikeRental_System3.AI.Chunking;
using BikeRental_System3.AI.Documents;
using BikeRental_System3.AI.Embeddings;
using BikeRental_System3.AI.Interfaces;
using BikeRental_System3.AI.Retrieval;
using BikeRental_System3.AI.Services;
using BikeRental_System3.AI.VectorStore;
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
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using Npgsql;
using Pgvector.Npgsql;
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
            var openAiApiKey     = builder.Configuration["OpenAI:ApiKey"]
                ?? throw new InvalidOperationException("OpenAI:ApiKey is missing from appsettings.json");
            var openAiModel      = builder.Configuration["OpenAI:Model"]          ?? "gpt-4o-mini";
            var openAiEmbedModel = builder.Configuration["OpenAI:EmbeddingModel"] ?? "text-embedding-3-small";

            builder.Services.AddSingleton(sp =>
            {
                var kernelBuilder = Kernel.CreateBuilder();
                kernelBuilder.AddOpenAIChatCompletion(openAiModel, openAiApiKey);
                return kernelBuilder.Build();
            });

            // ── Phase 4.3 — Embedding Service ─────────────────────────────────
            // ITextEmbeddingGenerationService and OpenAITextEmbeddingGenerationService
            // are [Experimental] in SK — suppress intentionally (stable in SK 1.x).
#pragma warning disable SKEXP0001, SKEXP0010
            builder.Services.AddSingleton<ITextEmbeddingGenerationService>(
                new OpenAITextEmbeddingGenerationService(openAiEmbedModel, openAiApiKey));

            builder.Services.AddSingleton<IEmbeddingService>(sp =>
                new OpenAIEmbeddingService(
                    sp.GetRequiredService<ITextEmbeddingGenerationService>(),
                    openAiEmbedModel,
                    sp.GetRequiredService<ILogger<OpenAIEmbeddingService>>()));
#pragma warning restore SKEXP0001, SKEXP0010

            // ── Phase 4.4 — Vector Store (PostgreSQL + pgvector) ─────────────────────
            //
            // VectorDatabaseOptions: reads Host/Port/Database/Username/Password/TableName
            //   from appsettings.json "VectorDatabase" section.
            //
            // NpgsqlDataSource: singleton connection pool wired with pgvector type mapping
            //   via UseVector().  The data source does NOT open a connection at startup;
            //   actual connections are established on first use, so a missing PostgreSQL
            //   server will surface only when IVectorStore methods are called.
            //
            // IVectorStore → PgVectorStore: SINGLETON (changed in Phase 4.5).
            //   PgVectorStore is stateless — opens/closes connections per operation.
            //   Singleton required so VectorStoreContextProvider (also Singleton) can inject it.
            builder.Services.Configure<VectorDatabaseOptions>(
                builder.Configuration.GetSection("VectorDatabase"));

            var vectorDbOptions = builder.Configuration
                .GetSection("VectorDatabase")
                .Get<VectorDatabaseOptions>() ?? new VectorDatabaseOptions();

            builder.Services.AddSingleton(_ =>
            {
                var dataSourceBuilder = new NpgsqlDataSourceBuilder(
                    vectorDbOptions.BuildConnectionString());
                dataSourceBuilder.UseVector();
                return dataSourceBuilder.Build();
            });

            // Singleton: PgVectorStore is stateless — it opens/closes connections
            // per operation using the NpgsqlDataSource pool. No request-scoped state.
            // Singleton lifetime is required because VectorStoreContextProvider
            // (Phase 4.5) is itself Singleton and injects IVectorStore.
            builder.Services.AddSingleton<IVectorStore, PgVectorStore>();

            // ── Phase 4.4.1 — Data Ingestion Pipeline ────────────────────────
            //
            // IDocumentChunker → FixedSizeChunker: Singleton (pure in-memory, stateless).
            //   Not registered previously — required now that the ingestion service
            //   needs to split documents into chunks.
            //
            // IDocumentIngestionService → DocumentIngestionService: Scoped.
            //   Scoped for each HTTP ingestion request. IVectorStore is now Singleton
            //   (safe to inject into Scoped — narrower lifetime injecting wider is fine).
            builder.Services.AddSingleton<IDocumentChunker, FixedSizeChunker>();
            builder.Services.AddScoped<IDocumentIngestionService, DocumentIngestionService>();

            // ── Phase 4.5 — Semantic Retrieval (RAG Query Pipeline) ───────────
            //
            // RetrievalOptions: binds TopK / MaximumContextCharacters / MinimumSimilarity
            //   from appsettings.json "Retrieval" section.
            //
            // IQueryEmbeddingService → QueryEmbeddingService (Singleton):
            //   Converts user questions into float[1536] vectors via SK embedding service.
            //   Uses SAME model (text-embedding-3-small) as Phase 4.3 ingestion —
            //   critical for cosine similarity to be meaningful.
            //
            // IContextBuilder → ContextBuilder (Singleton):
            //   Pure text formatter. Turns Top-K VectorDocuments into a structured
            //   context string for injection into {{context}} in the system prompt.
            //
            // IContextProvider → VectorStoreContextProvider (Singleton):
            //   REPLACES CompositeContextProvider from Phase 3.
            //   On each chat request: embed query → cosine search → build context.
            //   BikeRentalChatChain and ChatController do NOT change — they both
            //   depend on IContextProvider which is still the same interface.
            //   Open/Closed Principle: system extended, not modified.
            builder.Services.Configure<RetrievalOptions>(
                builder.Configuration.GetSection(RetrievalOptions.SectionName));

#pragma warning disable SKEXP0001  // ITextEmbeddingGenerationService is [Experimental]
            builder.Services.AddSingleton<IQueryEmbeddingService, QueryEmbeddingService>();
#pragma warning restore SKEXP0001

            builder.Services.AddSingleton<IContextBuilder, ContextBuilder>();

            // 2. Prompt template — reads AI/Prompts/bike-rental-system.txt at startup.
            //    Singleton: the file is loaded once; RenderSystemPrompt() is stateless.
            builder.Services.AddSingleton<IPromptTemplateService, BikeRentalPromptTemplate>();

            // 3. Context provider — Phase 4.5: VectorStoreContextProvider replaces
            //    CompositeContextProvider as the IContextProvider implementation.
            //
            //    Phase 3 providers (DatabaseContextProvider, PdfContextProvider) are
            //    kept registered by concrete type so they remain available for direct
            //    injection in future phases if needed. They are no longer wired as
            //    IContextProvider — VectorStoreContextProvider is the sole implementation.
            builder.Services.AddSingleton<IDocumentLoader, PdfDocumentLoader>();
            builder.Services.AddSingleton<DatabaseContextProvider>();
            builder.Services.AddSingleton<PdfContextProvider>();
            builder.Services.AddSingleton<IContextProvider, VectorStoreContextProvider>();

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
