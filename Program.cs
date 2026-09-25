using System.Text;
using ApiEcommerce.Constants;
using ApiEcommerce.Models;
using ApiEcommerce.Repository;
using ApiEcommerce.Repository.IRepository;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

//db
var dbConnectionString = builder.Configuration.GetConnectionString("ConexionSql");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(dbConnectionString));

//caché
builder.Services.AddResponseCaching(options =>
{
  options.MaximumBodySize = 1024 * 1024;
  options.UseCaseSensitivePaths = true;
});

//automapper
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>(); //instancia de Product Repository para las solicitudes http
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddAutoMapper(typeof(Program).Assembly);


//servicio de authorice
//key
var secretKey = builder.Configuration.GetValue<string>("ApiSettings:SecretKey");
if (string.IsNullOrEmpty(secretKey))
{
  throw new InvalidOperationException("La secret key no está configurada");
}

//Servicio authorice
builder.Services.AddAuthentication(options =>
{
  options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
  options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
  options.RequireHttpsMetadata = false; //dev: false, producción: true
  options.SaveToken = true;             //guarda token
  options.TokenValidationParameters = new TokenValidationParameters
  {
    ValidateIssuerSigningKey = true,    //validar token? si
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)), //verifiar firma token
    ValidateIssuer = false,            //validar emisor? no
    ValidateAudience = false,          //validar audience? no
  };
})
;

//controllers
builder.Services.AddControllers(option =>
{

  option.CacheProfiles.Add(CacheProfiles.Default10, CacheProfiles.profile10);
  option.CacheProfiles.Add(CacheProfiles.Default20, CacheProfiles.profile10); //(propieda, instancia)


});




builder.Services.AddOpenApi();

//Documentación de la Api en swagger
builder.Services.AddSwaggerGen(options =>
  {
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
      Description = "Nuestra API utiliza la Autenticación JWT usando el esquema Bearer. \n\r\n\r" +
                    "Ingresa la palabra a continuación el token generado en login.\n\r\n\r" +
                    "Ejemplo: \"12345abcdef\"",
      Name = "Authorization",
      In = ParameterLocation.Header,
      Type = SecuritySchemeType.Http,
      Scheme = "Bearer"
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
      [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
    options.SwaggerDoc("v1", new OpenApiInfo
    {
      Version = "v1",
      Title = "API Ecommerce",
      Description = "API para gestionar productos y usuarios",
      TermsOfService = new Uri("http://example.com/terms"),
      Contact = new OpenApiContact
      {
        Name = "DevTalles",
        Url = new Uri("https://devtalles.com")
      },
      License = new OpenApiLicense
      {
        Name = "Licencia de uso",
        Url = new Uri("https://example.com/license")
      }

    });
    options.SwaggerDoc("v2", new OpenApiInfo
    {
      Version = "v2",
      Title = "API Ecommerce V2",
      Description = "API para gestionar productos y usuarios",
      TermsOfService = new Uri("http://example.com/terms"),
      Contact = new OpenApiContact
      {
        Name = "DevTalles",
        Url = new Uri("https://devtalles.com")
      },
      License = new OpenApiLicense
      {
        Name = "Licencia de uso",
        Url = new Uri("https://example.com/license")
      }

    });
    
    ;
  }

);


var apiVersioningBuilder = builder.Services.AddApiVersioning(option =>
{
  option.AssumeDefaultVersionWhenUnspecified = true;   //versio por defecto
  option.DefaultApiVersion = new ApiVersion(1,0);      //decfecto = ApiVersion(grupo, versionMinima)
  option.ReportApiVersions = true;   //reportar versiones para clientes o dbgin
  //option.ApiVersionReader = ApiVersionReader.Combine(new QueryStringApiVersionReader("api-version"));  //parametro en la url de la version de la api   ?api-version
});

//para que swagger muestre las versiones
apiVersioningBuilder.AddApiExplorer(option =>
{
  option.GroupNameFormat= "'v'VVV"; //formato de la api = v1, v2, v3...
  option.SubstituteApiVersionInUrl = true;   //  api/v{version}/products
});


//cors
builder.Services.AddCors(options =>
  {
    options.AddPolicy(PolicyNames.AllowSpecificOrigin,
    builder =>
    {
      builder.WithOrigins("http//localhost:3000").AllowAnyMethod().AllowAnyHeader();
    });
  });

var app = builder.Build();





if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
  app.UseSwagger();
  app.UseSwaggerUI(options =>
  {
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "v2");
  });

}
//middlewrs
app.UseHttpsRedirection();

app.UseCors(PolicyNames.AllowSpecificOrigin);
app.UseResponseCaching();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
