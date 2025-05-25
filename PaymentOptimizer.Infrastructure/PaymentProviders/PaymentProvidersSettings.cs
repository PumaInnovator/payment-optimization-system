using System.Collections.Generic;

// ARCHIVO: PaymentOptimizer.Infrastructure/PaymentProviders/PaymentProvidersSettings.cs
// PROPÓSITO: Definición única y limpia de todas las configuraciones de proveedores de pago
// INSTRUCCIONES: Este archivo debe REEMPLAZAR completamente cualquier otro archivo de configuración

namespace PaymentOptimizer.Infrastructure.Configuration
{
    /// <summary>
    /// Configuración para el proveedor de pago PagaFacil.
    /// Esta clase mapea directamente con la sección "PaymentProviders:PagaFacil" en appsettings.json
    /// </summary>
    public class PagaFacilSettings
    {
        /// <summary>
        /// URL base de la API de PagaFacil.
        /// Ejemplo: "https://app-paga-chg-aviva.azurewebsites.net"
        /// CRÍTICO: Si este valor es null, causará errores de "uriString cannot be null"
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// Clave de API para autenticación con PagaFacil.
        /// Se envía como header "x-api-key" en todas las solicitudes
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Tiempo límite en segundos para solicitudes HTTP.
        /// Valor recomendado: 30 segundos
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Habilita logging detallado para debugging.
        /// Útil en desarrollo, considera deshabilitar en producción
        /// </summary>
        public bool EnableLogging { get; set; } = true;
    }

    /// <summary>
    /// Configuración para el proveedor de pago CazaPagos.
    /// Esta clase mapea directamente con la sección "PaymentProviders:CazaPagos" en appsettings.json
    /// </summary>
    public class CazaPagosSettings
    {
        /// <summary>
        /// URL base de la API de CazaPagos.
        /// Ejemplo: "https://app-caza-chg-aviva.azurewebsites.net"
        /// CRÍTICO: Si este valor es null, causará errores de "uriString cannot be null"
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// Clave de API para autenticación con CazaPagos.
        /// Se envía como header "x-api-key" en todas las solicitudes
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Tiempo límite en segundos para solicitudes HTTP.
        /// Valor recomendado: 30 segundos
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Habilita logging detallado para debugging.
        /// Útil en desarrollo, considera deshabilitar en producción
        /// </summary>
        public bool EnableLogging { get; set; } = true;
    }
}