using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using PaymentOptimizer.Domain.Interfaces.Services;
using PaymentOptimizer.Infrastructure.PaymentProviders.PagaFacil;
using Microsoft.OpenApi.Models;
using PaymentOptimizer.Application;
using PaymentOptimizer.Infrastructure;
using PaymentOptimizer.Domain.Interfaces.Services;
using PaymentOptimizer.Infrastructure.PaymentProviders.PagaFacil;
using PaymentOptimizer.Infrastructure.PaymentProviders.CazaPagos;
using PaymentOptimizer.Infrastructure.PaymentProviders;
using PaymentOptimizer.Domain.Interfaces.Repositories; // Añade esta línea
using System; // Añade esta línea

var builder = WebApplication.CreateBuilder(args);
// Añadir servicios al contenedor
builder.Services.AddControllers();
//Dependencias
builder.Services.AddHttpClient();
// Registrar servicios de aplicación
builder.Services.AddApplicationServices();

// Registrar servicios de infraestructura
builder.Services.AddInfrastructureServices(builder.Configuration);

// Configurar Swagger/OpenAPI
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Payment Optimizer API",
        Version = "v1",
        Description = "API para gestionar órdenes de pago con selección inteligente de proveedores"
    });
});

// CÓDIGO DE DIAGNÓSTICO: Verificar que IOrderRepository está registrado como Singleton
// Añade este bloque justo antes de 'var app = builder.Build();'
{
    using var tempServiceProvider = builder.Services.BuildServiceProvider();
    var repo1 = tempServiceProvider.GetService<IOrderRepository>();
    var repo2 = tempServiceProvider.GetService<IOrderRepository>();

    Console.WriteLine("=== VERIFICACIÓN DE SINGLETON ===");
    Console.WriteLine($"Repositorio 1 HashCode: {repo1?.GetHashCode()}");
    Console.WriteLine($"Repositorio 2 HashCode: {repo2?.GetHashCode()}");
    Console.WriteLine($"¿Son la misma instancia? {Object.ReferenceEquals(repo1, repo2)}");
    Console.WriteLine("=== FIN VERIFICACIÓN ===");
}

// CONSTRUIR LA APLICACIÓN - Todo lo anterior debe registrarse antes de esta línea
var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Aplicación iniciada. Modo: {Environment}",
    app.Environment.IsDevelopment() ? "Desarrollo" : "Producción");

// Configurar el pipeline de solicitudes HTTP
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Payment Optimizer API v1"));
}

// Comentado para desarrollo local - descomenta en producción si es necesario
//app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

// CÓDIGO DE DIAGNÓSTICO ADICIONAL: Verificar estado inicial del repositorio
// Añade este bloque antes de app.Run()
{
    using var scope = app.Services.CreateScope();
    var repository = scope.ServiceProvider.GetService<IOrderRepository>();

    if (repository != null)
    {
        Console.WriteLine("=== VERIFICACIÓN INICIAL DEL REPOSITORIO ===");
        var repoType = repository.GetType().Name;
        Console.WriteLine($"Tipo de repositorio: {repoType}");
        Console.WriteLine($"HashCode: {repository.GetHashCode()}");

        // Si el repositorio tiene un método LogRepositoryState, llamarlo
        if (repository.GetType().GetMethod("LogRepositoryState") != null)
        {
            repository.GetType().GetMethod("LogRepositoryState").Invoke(repository, null);
        }
        Console.WriteLine("=== FIN VERIFICACIÓN REPOSITORIO ===");
    }
}

app.Run();