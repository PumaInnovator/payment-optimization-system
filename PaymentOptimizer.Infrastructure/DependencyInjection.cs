using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentOptimizer.Domain.Interfaces.Repositories;
using PaymentOptimizer.Domain.Interfaces.Services;
using PaymentOptimizer.Infrastructure.PaymentProviders;
using PaymentOptimizer.Infrastructure.PaymentProviders.CazaPagos;
using PaymentOptimizer.Infrastructure.PaymentProviders.PagaFacil;
using PaymentOptimizer.Infrastructure.Configuration;
using PaymentOptimizer.Infrastructure.Persistence;

// IMPORTANTE: Esta es la única referencia a configuraciones que debes tener
// Elimina cualquier otra referencia a Configuration en diferentes namespaces
using PaymentOptimizer.Infrastructure.Configuration;

namespace PaymentOptimizer.Infrastructure
{
    /// <summary>
    /// Configuración de inyección de dependencias corregida para eliminar referencias ambiguas.
    /// 
    /// Esta versión usa explícitamente el namespace correcto para las clases de configuración,
    /// eliminando cualquier ambigüedad que el compilador pueda encontrar.
    /// 
    /// Principio clave: Un proyecto bien organizado debe tener una sola definición
    /// de cada clase, ubicada en el namespace más apropiado según su responsabilidad.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Método principal para registrar todos los servicios de infraestructura.
        /// 
        /// Este método sigue el patrón de "configuración fluida" donde cada paso
        /// construye sobre el anterior, creando una configuración completa y coherente.
        /// </summary>
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Registrar configuraciones - ahora sin ambigüedad
            RegisterPaymentConfigurations(services, configuration);

            // Configurar clientes HTTP
            ConfigureHttpClients(services);

            // Registrar proveedores de pago
            RegisterPaymentProviders(services);

            // Registrar otros servicios necesarios
            RegisterDomainServices(services);

            return services;
        }

        /// <summary>
        /// Registra las configuraciones de proveedores de pago usando las clases definitivas.
        /// 
        /// Al especificar explícitamente el tipo completo con namespace, eliminamos
        /// cualquier posibilidad de referencias ambiguas. Es como dar la dirección
        /// completa de una casa en lugar de solo el nombre de la calle.
        /// </summary>
        private static void RegisterPaymentConfigurations(
    IServiceCollection services,
    IConfiguration configuration)
        {
            // Usar el método más directo de configuración
            services.Configure<CazaPagosSettings>(
                configuration.GetSection("PaymentProviders:CazaPagos"));

            services.Configure<PagaFacilSettings>(
                configuration.GetSection("PaymentProviders:PagaFacil"));

            // Verificación de diagnóstico
            var cazaBaseUrl = configuration["PaymentProviders:CazaPagos:BaseUrl"];
            var pagaBaseUrl = configuration["PaymentProviders:PagaFacil:BaseUrl"];

            Console.WriteLine($"[DIAG] CazaPagos URL: {cazaBaseUrl ?? "NO ENCONTRADO"}");
            Console.WriteLine($"[DIAG] PagaFacil URL: {pagaBaseUrl ?? "NO ENCONTRADO"}");
        }

        /// <summary>
        /// Método de diagnóstico para verificar que las configuraciones se están cargando correctamente.
        /// 
        /// Este método es temporal y puede ser removido una vez que confirmes que todo funciona.
        /// En un entorno de producción, podrías querer mantener solo logs de nivel Info o Warning.
        /// </summary>
        private static void LogConfigurationDiagnostics(IConfiguration configuration)
        {
            var cazaPagosBaseUrl = configuration["PaymentProviders:CazaPagos:BaseUrl"];
            var pagaFacilBaseUrl = configuration["PaymentProviders:PagaFacil:BaseUrl"];

            Console.WriteLine("=== DIAGNÓSTICO DE CONFIGURACIÓN ===");
            Console.WriteLine($"CazaPagos BaseUrl: {cazaPagosBaseUrl ?? "NO CONFIGURADO"}");
            Console.WriteLine($"PagaFacil BaseUrl: {pagaFacilBaseUrl ?? "NO CONFIGURADO"}");
            Console.WriteLine("=====================================");
        }

        /// <summary>
        /// Configura los clientes HTTP para cada proveedor de pago.
        /// 
        /// Cada proveedor obtiene su propio HttpClient con configuraciones específicas.
        /// Esto permite optimizar timeouts, headers, y otras configuraciones HTTP
        /// según las características de cada API externa.
        /// </summary>
        private static void ConfigureHttpClients(IServiceCollection services)
        {
            // HttpClient para CazaPagos con configuración específica
            services.AddHttpClient<CazaPagosAdapter>("CazaPagosClient", client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                // Podrías agregar headers comunes, políticas de retry, etc.
            });

            // HttpClient para PagaFacil con configuración específica
            services.AddHttpClient<PagaFacilAdapter>("PagaFacilClient", client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                // Configuraciones específicas para PagaFacil si son necesarias
            });
        }

        /// <summary>
        /// Registra todos los adaptadores de proveedores de pago.
        /// 
        /// Cada adaptador implementa IPaymentProvider, lo que permite que el sistema
        /// los trate polimórficamente. El contenedor de DI puede inyectar automáticamente
        /// la colección completa de proveedores donde sea necesario.
        /// </summary>
        private static void RegisterPaymentProviders(IServiceCollection services)
        {
            // Registrar adaptadores como implementaciones de IPaymentProvider
            services.AddTransient<IPaymentProvider, CazaPagosAdapter>();
            services.AddTransient<IPaymentProvider, PagaFacilAdapter>();
            services.AddTransient<IPaymentProvider, MockPaymentProvider>();

            // Registrar el selector que implementa la lógica de optimización
            services.AddTransient<IPaymentProviderSelector, PaymentProviderSelector>();
        }

        /// <summary>
        /// Registra servicios adicionales del dominio y persistencia.
        /// 
        /// Separamos este registro para mantener el código organizado y facilitar
        /// futuras expansiones del sistema de servicios.
        /// </summary>
        private static void RegisterDomainServices(IServiceCollection services)
        {
            // Repositorio para persistencia de órdenes
            services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();

            // Aquí puedes agregar otros servicios de dominio según sea necesario
            // Por ejemplo: services.AddTransient<IOrderService, OrderService>();
        }
    }
}