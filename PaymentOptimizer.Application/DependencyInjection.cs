using Microsoft.Extensions.DependencyInjection;
using PaymentOptimizer.Application.Services;

namespace PaymentOptimizer.Application
{
    /// <summary>
    /// Configuración de los servicios de la capa de aplicación para inyección de dependencias.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Agrega los servicios de la capa de aplicación al contenedor de servicios.
        /// </summary>
        /// <param name="services">Colección de servicios donde se registrarán los servicios de aplicación</param>
        /// <returns>La colección de servicios con los servicios de aplicación registrados</returns>
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services)
        {
            // Registramos el servicio de órdenes
            services.AddScoped<IOrderService, OrderService>();

            // Aquí se pueden registrar otros servicios de aplicación en el futuro

            return services;
        }
    }
}