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
using PaymentOptimizer.Domain.Interfaces.Repositories; // A�ade esta l�nea
using System; // A�ade esta l�nea

var builder = WebApplication.CreateBuilder(args);
// A�adir servicios al contenedor
builder.Services.AddControllers();
//Dependencias
builder.Services.AddHttpClient();
// Registrar servicios de aplicaci�n
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
        Description = "API para gestionar �rdenes de pago con selecci�n inteligente de proveedores"
    });
});

// C�DIGO DE DIAGN�STICO: Verificar que IOrderRepository est� registrado como Singleton
// A�ade este bloque justo antes de 'var app = builder.Build();'
{
    using var tempServiceProvider = builder.Services.BuildServiceProvider();
    var repo1 = tempServiceProvider.GetService<IOrderRepository>();
    var repo2 = tempServiceProvider.GetService<IOrderRepository>();

    Console.WriteLine("=== VERIFICACI�N DE SINGLETON ===");
    Console.WriteLine($"Repositorio 1 HashCode: {repo1?.GetHashCode()}");
    Console.WriteLine($"Repositorio 2 HashCode: {repo2?.GetHashCode()}");
    Console.WriteLine($"�Son la misma instancia? {Object.ReferenceEquals(repo1, repo2)}");
    Console.WriteLine("=== FIN VERIFICACI�N ===");
}

// CONSTRUIR LA APLICACI�N - Todo lo anterior debe registrarse antes de esta l�nea
var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Aplicaci�n iniciada. Modo: {Environment}",
    app.Environment.IsDevelopment() ? "Desarrollo" : "Producci�n");

// Configurar el pipeline de solicitudes HTTP
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Payment Optimizer API v1"));
}

// Comentado para desarrollo local - descomenta en producci�n si es necesario
//app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

// C�DIGO DE DIAGN�STICO ADICIONAL: Verificar estado inicial del repositorio
// A�ade este bloque antes de app.Run()
{
    using var scope = app.Services.CreateScope();
    var repository = scope.ServiceProvider.GetService<IOrderRepository>();

    if (repository != null)
    {
        Console.WriteLine("=== VERIFICACI�N INICIAL DEL REPOSITORIO ===");
        var repoType = repository.GetType().Name;
        Console.WriteLine($"Tipo de repositorio: {repoType}");
        Console.WriteLine($"HashCode: {repository.GetHashCode()}");

        // Si el repositorio tiene un m�todo LogRepositoryState, llamarlo
        if (repository.GetType().GetMethod("LogRepositoryState") != null)
        {
            repository.GetType().GetMethod("LogRepositoryState").Invoke(repository, null);
        }
        Console.WriteLine("=== FIN VERIFICACI�N REPOSITORIO ===");
    }
}

app.Run();