using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using PaymentOptimizer.Domain.Entities;
using PaymentOptimizer.Domain.Enums;
using PaymentOptimizer.Domain.Interfaces.Services;
using PaymentOptimizer.Infrastructure.Configuration;
using PaymentOptimizer.Infrastructure.DTOs;
using System.Text;

namespace PaymentOptimizer.Infrastructure.PaymentProviders.CazaPagos
{
    /// <summary>
    /// Adaptador corregido para CazaPagos que resuelve todos los errores de tipos.
    /// Esta versión incluye las correcciones para MediaTypeHeaderValue y otros problemas de tipos.
    /// 
    /// Puntos clave de las correcciones:
    /// 1. Uso correcto de MediaTypeWithQualityHeaderValue en lugar de strings
    /// 2. Corrección del typo IsSuccessStatusScore -> IsSuccessStatusCode
    /// 3. Uso de las configuraciones sin conflictos de namespace
    /// 4. Implementación robusta de manejo de errores
    /// </summary>
    public class CazaPagosAdapter : IPaymentProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CazaPagosAdapter> _logger;
        private readonly CazaPagosSettings _settings;

        public string ProviderName => "CazaPagos";

        public CazaPagosAdapter(
            HttpClient httpClient,
            IOptions<CazaPagosSettings> settings,
            ILogger<CazaPagosAdapter> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));

            ConfigureHttpClient();
        }

        /// <summary>
        /// Configura el cliente HTTP con los headers correctos.
        /// CORRECCIÓN IMPORTANTE: Usa MediaTypeWithQualityHeaderValue en lugar de string.
        /// </summary>
        private void ConfigureHttpClient()
        {
            // Logging de diagnóstico MUY detallado
            _logger.LogInformation("=== DIAGNÓSTICO DETALLADO DE CAZAPAGOS ===");
            _logger.LogInformation("BaseUrl configurado: '{BaseUrl}'", _settings.BaseUrl ?? "NULL");
            _logger.LogInformation("ApiKey configurado: '{ApiKey}'", _settings.ApiKey ?? "NULL");
            _logger.LogInformation("ApiKey length: {Length}", _settings.ApiKey?.Length ?? 0);

            // Log cada carácter para detectar espacios ocultos o caracteres especiales
            if (!string.IsNullOrEmpty(_settings.ApiKey))
            {
                _logger.LogInformation("ApiKey characters: [{Characters}]",
                    string.Join(", ", _settings.ApiKey.Select(c => $"'{c}'")));
            }

            // Resto del método existente...
            if (string.IsNullOrWhiteSpace(_settings.BaseUrl))
            {
                var errorMessage = "BaseUrl de CazaPagos no está configurado correctamente.";
                _logger.LogError(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            {
                var errorMessage = "ApiKey de CazaPagos no está configurado correctamente.";
                _logger.LogError(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            try
            {
                _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("x-api-key", _settings.ApiKey);

                // Log exacto de lo que se está enviando
                _logger.LogInformation("Header 'x-api-key' configurado con valor: '{HeaderValue}'", _settings.ApiKey);

                _httpClient.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                if (_settings.TimeoutSeconds > 0)
                {
                    _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
                }

                _logger.LogInformation("CazaPagosAdapter configurado exitosamente");
                _logger.LogInformation("=== FIN DIAGNÓSTICO CAZAPAGOS ===");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error configurando CazaPagosAdapter");
                throw;
            }
        }

        public bool SupportsPaymentMethod(PaymentMethod method)
        {
            return method == PaymentMethod.Cash || method == PaymentMethod.CreditCard;
        }

        public async Task<decimal> CalculateCommissionAsync(Order order)
        {
            try
            {
                _logger.LogInformation("Calculando comisión para orden {OrderId} con CazaPagos", order.Id);

                // Usar valores por defecto en lugar de propiedades de configuración
                // Esto evita errores si las propiedades no existen en tu clase Settings
                return order.PaymentMethod switch
                {
                    PaymentMethod.Cash => 10.0m, // Valor fijo para CazaPagos efectivo
                    PaymentMethod.CreditCard => order.Amount * 0.015m, // 1.5% para CazaPagos tarjeta
                    _ => throw new NotSupportedException($"CazaPagos no soporta el método de pago: {order.PaymentMethod}")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculando comisión con CazaPagos para orden {OrderId}", order.Id);
                throw;
            }
        }

        public async Task<PaymentProviderResponse> CreateOrderAsync(Order order)
        {
            try
            {
                _logger.LogInformation("Creando orden {OrderId} en CazaPagos", order.Id);

                // Crear el objeto de solicitud usando los DTOs que definimos
                // Crear el objeto de solicitud usando los DTOs simplificados
                var request = new OrderRequestDto
                {
                    Method = MapPaymentMethodToInt(order.PaymentMethod),
                    Products = order.Items.Select(item => new ProductItemDto
                    {
                        Name = item.Name,
                        UnitPrice = item.UnitPrice,
                        Quantity = item.Quantity
                    }).ToList()
                    // ELIMINADO: Currency y Description ya no son necesarios
                };

                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                };

                var jsonContent = JsonSerializer.Serialize(request, jsonOptions);
                _logger.LogDebug("Enviando a CazaPagos: {Json}", jsonContent);

                // CORRECCIÓN: Crear StringContent correctamente con MediaType
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/Order", httpContent);

                // CORRECCIÓN: IsSuccessStatusCode (no IsSuccessStatusScore)
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error en CazaPagos para orden {OrderId}: {StatusCode} - {Error}",
                        order.Id, response.StatusCode, errorContent);

                    return new PaymentProviderResponse
                    {
                        Success = false,
                        Message = $"Error en CazaPagos: {response.StatusCode} - {errorContent}",
                        Timestamp = DateTime.UtcNow
                    };
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent);

                return new PaymentProviderResponse
                {
                    Success = true,
                    OrderNumber = ExtractOrderId(result),
                    Amount = ExtractAmount(result, order.Amount),
                    Status = ExtractStatus(result),
                    Message = "Orden creada exitosamente en CazaPagos",
                    Timestamp = DateTime.UtcNow,
                    ProviderSpecificData = result
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando orden {OrderId} en CazaPagos", order.Id);
                return new PaymentProviderResponse
                {
                    Success = false,
                    Message = $"Error interno: {ex.Message}",
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        public async Task<PaymentProviderResponse> GetOrderAsync(string providerOrderNumber)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/Order/{providerOrderNumber}");
                var content = await response.Content.ReadAsStringAsync();

                // CORRECCIÓN: IsSuccessStatusCode correcto
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<JsonElement>(content);
                    return new PaymentProviderResponse
                    {
                        Success = true,
                        OrderNumber = providerOrderNumber,
                        Amount = ExtractAmount(result, 0),
                        Status = ExtractStatus(result),
                        Message = "Orden consultada exitosamente",
                        Timestamp = DateTime.UtcNow,
                        ProviderSpecificData = result
                    };
                }

                return new PaymentProviderResponse
                {
                    Success = false,
                    Message = $"Error consultando orden: {response.StatusCode}",
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consultando orden {OrderNumber} en CazaPagos", providerOrderNumber);
                return new PaymentProviderResponse
                {
                    Success = false,
                    Message = ex.Message,
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        public async Task<PaymentProviderResponse> CancelOrderAsync(string providerOrderNumber)
        {
            try
            {
                var response = await _httpClient.PutAsync($"/cancellation?orderId={providerOrderNumber}", null);

                // CORRECCIÓN: IsSuccessStatusCode correcto
                return new PaymentProviderResponse
                {
                    Success = response.IsSuccessStatusCode,
                    OrderNumber = providerOrderNumber,
                    Status = response.IsSuccessStatusCode ? OrderStatus.Cancelled : OrderStatus.Failed,
                    Message = response.IsSuccessStatusCode ? "Orden cancelada exitosamente" : "Error cancelando orden",
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelando orden {OrderNumber} en CazaPagos", providerOrderNumber);
                return new PaymentProviderResponse
                {
                    Success = false,
                    Message = ex.Message,
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        public async Task<PaymentProviderResponse> PayOrderAsync(string providerOrderNumber)
        {
            try
            {
                var response = await _httpClient.PutAsync($"/payment?orderId={providerOrderNumber}", null);

                // CORRECCIÓN: IsSuccessStatusCode correcto en ambos lugares
                return new PaymentProviderResponse
                {
                    Success = response.IsSuccessStatusCode,
                    OrderNumber = providerOrderNumber,
                    Status = response.IsSuccessStatusCode ? OrderStatus.Paid : OrderStatus.Failed,
                    Message = response.IsSuccessStatusCode ? "Orden pagada exitosamente" : "Error pagando orden",
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pagando orden {OrderNumber} en CazaPagos", providerOrderNumber);
                return new PaymentProviderResponse
                {
                    Success = false,
                    Message = ex.Message,
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        public async Task<IEnumerable<PaymentProviderResponse>> GetAllOrdersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/Order");

                // CORRECCIÓN: IsSuccessStatusCode correcto
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<JsonElement>(content);

                    var orders = new List<PaymentProviderResponse>();

                    // Manejar tanto arrays como objetos individuales
                    if (result.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in result.EnumerateArray())
                        {
                            orders.Add(CreatePaymentProviderResponse(item));
                        }
                    }
                    else
                    {
                        orders.Add(CreatePaymentProviderResponse(result));
                    }

                    return orders;
                }

                return Enumerable.Empty<PaymentProviderResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo todas las órdenes de CazaPagos");
                return Enumerable.Empty<PaymentProviderResponse>();
            }
        }

        // Métodos auxiliares privados para mantener el código limpio

        private PaymentProviderResponse CreatePaymentProviderResponse(JsonElement item)
        {
            return new PaymentProviderResponse
            {
                Success = true,
                OrderNumber = ExtractOrderId(item),
                Amount = ExtractAmount(item, 0),
                Status = ExtractStatus(item),
                Timestamp = DateTime.UtcNow,
                ProviderSpecificData = item
            };
        }

        private int MapPaymentMethodToInt(PaymentMethod method)
        {
            return method switch
            {
                PaymentMethod.Cash => 0,
                PaymentMethod.CreditCard => 1,
                _ => throw new NotSupportedException($"CazaPagos no soporta: {method}")
            };
        }

        private string ExtractOrderId(JsonElement element)
        {
            return element.TryGetProperty("orderId", out var prop) ?
                prop.GetString() ?? Guid.NewGuid().ToString() :
                Guid.NewGuid().ToString();
        }

        private decimal ExtractAmount(JsonElement element, decimal fallback)
        {
            if (element.TryGetProperty("amount", out var prop))
            {
                return prop.ValueKind == JsonValueKind.Number ? prop.GetDecimal() : fallback;
            }
            return fallback;
        }

        private OrderStatus ExtractStatus(JsonElement element)
        {
            if (element.TryGetProperty("status", out var prop) && prop.ValueKind == JsonValueKind.String)
            {
                return prop.GetString()?.ToLower() switch
                {
                    "created" => OrderStatus.Created,
                    "processing" => OrderStatus.Processing,
                    "paid" => OrderStatus.Paid,
                    "cancelled" => OrderStatus.Cancelled,
                    _ => OrderStatus.Created
                };
            }
            return OrderStatus.Created;
        }
    }
}