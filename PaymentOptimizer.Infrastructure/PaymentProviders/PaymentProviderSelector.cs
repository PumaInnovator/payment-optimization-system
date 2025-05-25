using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PaymentOptimizer.Domain.Entities;
using PaymentOptimizer.Domain.Enums;
using PaymentOptimizer.Domain.Interfaces.Services;

namespace PaymentOptimizer.Infrastructure.PaymentProviders
{
    /// </summary>
    public class PaymentProviderSelector : IPaymentProviderSelector
    {
        private readonly IEnumerable<IPaymentProvider> _providers;
        private readonly ILogger<PaymentProviderSelector> _logger;

        /// <summary>
        /// Constructor que recibe todos los proveedores registrados en el sistema.
        /// El contenedor de DI automáticamente inyecta todos los IPaymentProvider registrados.
        /// </summary>
        public PaymentProviderSelector(
            IEnumerable<IPaymentProvider> providers,
            ILogger<PaymentProviderSelector> logger)
        {
            _providers = providers ?? throw new ArgumentNullException(nameof(providers));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Logging de diagnóstico para verificar qué proveedores están disponibles
            _logger.LogInformation("PaymentProviderSelector inicializado con {ProviderCount} proveedores: {ProviderNames}",
                _providers.Count(),
                string.Join(", ", _providers.Select(p => p.ProviderName)));
        }

        /// <summary>
        /// Método principal que implementa la lógica de optimización.
        /// Este es el algoritmo que hace que tu sistema sea "inteligente" - 
        /// evalúa automáticamente todos los proveedores y encuentra el más económico.
        /// </summary>
        public async Task<(IPaymentProvider Provider, decimal Commission)> SelectOptimalProviderAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            _logger.LogInformation(
                "=== INICIANDO PROCESO DE OPTIMIZACIÓN ===");
            _logger.LogInformation(
                "Seleccionando proveedor óptimo para orden {OrderId} con método {PaymentMethod} y monto {Amount:C}",
                order.Id, order.PaymentMethod, order.Amount);

            // Paso 1: Filtrar proveedores que soportan el método de pago solicitado
            var supportedProviders = _providers
                .Where(p => p.SupportsPaymentMethod(order.PaymentMethod))
                .ToList();

            _logger.LogInformation("Proveedores que soportan {PaymentMethod}: {SupportedCount} de {TotalCount}",
                order.PaymentMethod, supportedProviders.Count, _providers.Count());

            if (!supportedProviders.Any())
            {
                var errorMessage = $"No hay proveedores disponibles para el método de pago {order.PaymentMethod}";
                _logger.LogError(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            // Paso 2: Calcular comisiones para cada proveedor compatible
            var providerEvaluations = new List<(IPaymentProvider Provider, decimal Commission)>();

            foreach (var provider in supportedProviders)
            {
                try
                {
                    _logger.LogInformation("Evaluando proveedor: {ProviderName}", provider.ProviderName);

                    var commission = await provider.CalculateCommissionAsync(order);

                    _logger.LogInformation("✓ {ProviderName}: Comisión = {Commission:C} (Total con comisión: {Total:C})",
                        provider.ProviderName, commission, order.Amount + commission);

                    providerEvaluations.Add((provider, commission));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "✗ Error evaluando {ProviderName}: {ErrorMessage} - Este proveedor será excluido de la selección",
                        provider.ProviderName, ex.Message);
                    // Continuamos con los otros proveedores - no fallamos todo el proceso por un proveedor problemático
                }
            }

            // Verificar que al menos un proveedor pudo ser evaluado exitosamente
            if (!providerEvaluations.Any())
            {
                var errorMessage = "No se pudo evaluar ningún proveedor disponible para esta orden";
                _logger.LogError(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            // Paso 3: Seleccionar el proveedor con la comisión más baja (optimización)
            var optimalSelection = providerEvaluations
                .OrderBy(evaluation => evaluation.Commission)
                .First();

            // Logging detallado del resultado de la optimización
            _logger.LogInformation("=== RESULTADO DE LA OPTIMIZACIÓN ===");
            _logger.LogInformation("🏆 PROVEEDOR ÓPTIMO SELECCIONADO: {ProviderName}", optimalSelection.Provider.ProviderName);
            _logger.LogInformation("💰 Comisión más baja: {Commission:C}", optimalSelection.Commission);

            // Mostrar comparación con otros proveedores para transparencia
            if (providerEvaluations.Count > 1)
            {
                _logger.LogInformation("📊 COMPARACIÓN DE OPCIONES:");
                foreach (var evaluation in providerEvaluations.OrderBy(e => e.Commission))
                {
                    var isSelected = evaluation.Provider.ProviderName == optimalSelection.Provider.ProviderName;
                    var indicator = isSelected ? "👑 SELECCIONADO" : "  ";
                    _logger.LogInformation("    {Indicator} {ProviderName}: {Commission:C}",
                        indicator, evaluation.Provider.ProviderName, evaluation.Commission);
                }

                var savings = providerEvaluations.Max(e => e.Commission) - optimalSelection.Commission;
                if (savings > 0)
                {
                    _logger.LogInformation("💵 AHORRO LOGRADO: {Savings:C} vs la opción más cara", savings);
                }
            }

            _logger.LogInformation("=== FIN PROCESO DE OPTIMIZACIÓN ===");

            return optimalSelection;
        }

        /// <summary>
        /// Busca un proveedor específico por nombre.
        /// Útil para operaciones donde necesitas trabajar con un proveedor específico
        /// (como cancelaciones o consultas de órdenes existentes).
        /// </summary>
        public async Task<IPaymentProvider> GetProviderByNameAsync(string providerName)
        {
            if (string.IsNullOrWhiteSpace(providerName))
                throw new ArgumentException("El nombre del proveedor no puede estar vacío", nameof(providerName));

            _logger.LogInformation("Buscando proveedor específico: '{ProviderName}'", providerName);

            var provider = _providers.FirstOrDefault(p =>
                p.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase));

            if (provider == null)
            {
                _logger.LogWarning("❌ No se encontró el proveedor '{ProviderName}'. Proveedores disponibles: {AvailableProviders}",
                    providerName, string.Join(", ", _providers.Select(p => p.ProviderName)));
            }
            else
            {
                _logger.LogInformation("✅ Proveedor '{ProviderName}' encontrado exitosamente", provider.ProviderName);
            }

            return provider;
        }

        /// <summary>
        /// Método de diagnóstico para verificar el estado del selector.
        /// Útil durante el desarrollo y debugging.
        /// </summary>
        public void LogProviderStatus()
        {
            _logger.LogInformation("=== ESTADO DEL SELECTOR DE PROVEEDORES ===");
            _logger.LogInformation("Total de proveedores registrados: {Count}", _providers.Count());

            foreach (var provider in _providers)
            {
                var supportedMethods = Enum.GetValues<PaymentMethod>()
                    .Where(method => provider.SupportsPaymentMethod(method))
                    .Select(method => method.ToString());

                _logger.LogInformation("📦 {ProviderName}: Soporta {Methods}",
                    provider.ProviderName, string.Join(", ", supportedMethods));
            }
            _logger.LogInformation("=== FIN ESTADO DEL SELECTOR ===");
        }
    }
}