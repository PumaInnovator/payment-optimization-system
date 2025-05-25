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

namespace PaymentOptimizer.Infrastructure.PaymentProviders.PagaFacil
{
    public class PagaFacilAdapter : IPaymentProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PagaFacilAdapter> _logger;
        private readonly PagaFacilSettings _settings;

        public string ProviderName => "PagaFacil";

        public PagaFacilAdapter(
            HttpClient httpClient,
            IOptions<PagaFacilSettings> settings,
            ILogger<PagaFacilAdapter> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));

            ConfigureHttpClient();
        }

        private void ConfigureHttpClient()
        {
            // Logging forzado que siempre será visible
            Console.WriteLine("=== DIAGNÓSTICO DETALLADO DE PAGAFACIL ===");
            Console.WriteLine($"BaseUrl configurado: '{_settings.BaseUrl ?? "NULL"}'");
            Console.WriteLine($"ApiKey configurado: '{_settings.ApiKey ?? "NULL"}'");
            Console.WriteLine($"ApiKey length: {_settings.ApiKey?.Length ?? 0}");

            // También usar el logger, pero con nivel Error para garantizar visibilidad
            _logger.LogError("=== DIAGNÓSTICO PAGAFACIL ===");
            _logger.LogError("BaseUrl: {BaseUrl}", _settings.BaseUrl);
            _logger.LogError("ApiKey: {ApiKey}", _settings.ApiKey);

            // Log cada carácter para detectar problemas ocultos
            if (!string.IsNullOrEmpty(_settings.ApiKey))
            {
                var characters = string.Join(", ", _settings.ApiKey.Select(c => $"'{c}'"));
                Console.WriteLine($"ApiKey characters: [{characters}]");
                _logger.LogError("ApiKey characters: [{Characters}]", characters);
            }

            // Resto del método...
            if (string.IsNullOrWhiteSpace(_settings.BaseUrl))
            {
                var errorMessage = "BaseUrl de PagaFacil no está configurado correctamente.";
                _logger.LogError(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            {
                var errorMessage = "ApiKey de PagaFacil no está configurado correctamente.";
                _logger.LogError(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            try
            {
                _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("x-api-key", _settings.ApiKey);

                Console.WriteLine($"Header 'x-api-key' configurado con valor: '{_settings.ApiKey}'");
                _logger.LogError("Header x-api-key configurado: {ApiKey}", _settings.ApiKey);

                _httpClient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                if (_settings.TimeoutSeconds > 0)
                {
                    _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
                }

                Console.WriteLine("PagaFacilAdapter configurado exitosamente");
                _logger.LogError("PagaFacilAdapter configurado exitosamente");
                Console.WriteLine("=== FIN DIAGNÓSTICO PAGAFACIL ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error configurando PagaFacilAdapter: {ex.Message}");
                _logger.LogError(ex, "Error configurando PagaFacilAdapter");
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
                _logger.LogInformation("Calculando comisión para orden {OrderId} con PagaFacil", order.Id);

                return order.PaymentMethod switch
                {
                    PaymentMethod.Cash => 15.0m, // Comisión fija para PagaFacil efectivo
                    PaymentMethod.CreditCard => order.Amount * 0.01m, // 1% para PagaFacil tarjeta
                    _ => throw new NotSupportedException($"PagaFacil no soporta el método de pago: {order.PaymentMethod}")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculando comisión con PagaFacil para orden {OrderId}", order.Id);
                throw;
            }
        }

        public async Task<PaymentProviderResponse> CreateOrderAsync(Order order)
        {
            try
            {
                _logger.LogInformation("Creando orden {OrderId} en PagaFacil", order.Id);

                var request = new OrderRequestDto
                {
                    Method = MapPaymentMethodToInt(order.PaymentMethod),
                    Products = order.Items.Select(item => new ProductItemDto
                    {
                        Name = item.Name,
                        UnitPrice = item.UnitPrice,
                        Quantity = item.Quantity
                    }).ToList()
                };

                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                };

                var jsonContent = JsonSerializer.Serialize(request, jsonOptions);

                // LOGGING DETALLADO DE DIAGNÓSTICO
                Console.WriteLine("=== SOLICITUD HTTP COMPLETA ===");
                Console.WriteLine($"URL: {_httpClient.BaseAddress}/Order");
                Console.WriteLine($"Method: POST");
                Console.WriteLine("Headers:");
                foreach (var header in _httpClient.DefaultRequestHeaders)
                {
                    Console.WriteLine($"  {header.Key}: {string.Join(", ", header.Value)}");
                }
                Console.WriteLine($"Content-Type: application/json");
                Console.WriteLine($"JSON Body: {jsonContent}");
                Console.WriteLine("=== FIN SOLICITUD ===");

                _logger.LogError("JSON enviado a PagaFacil: {Json}", jsonContent);
                _logger.LogError("URL completa: {Url}", $"{_httpClient.BaseAddress}/Order");

                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/Order", httpContent);

                // LOGGING DE LA RESPUESTA
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"=== RESPUESTA RECIBIDA ===");
                Console.WriteLine($"Status Code: {response.StatusCode}");
                Console.WriteLine($"Response Body: {responseContent}");
                Console.WriteLine("=== FIN RESPUESTA ===");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = responseContent;
                    _logger.LogError("Error en PagaFacil para orden {OrderId}: {StatusCode} - {Error}",
                        order.Id, response.StatusCode, errorContent);

                    return new PaymentProviderResponse
                    {
                        Success = false,
                        Message = $"Error en PagaFacil: {response.StatusCode} - {errorContent}",
                        Timestamp = DateTime.UtcNow
                    };
                }

                var result = JsonSerializer.Deserialize<JsonElement>(responseContent);

                return new PaymentProviderResponse
                {
                    Success = true,
                    OrderNumber = ExtractOrderId(result),
                    Amount = ExtractAmount(result, order.Amount),
                    Status = ExtractStatus(result),
                    Message = "Orden creada exitosamente en PagaFacil",
                    Timestamp = DateTime.UtcNow,
                    ProviderSpecificData = result
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando orden {OrderId} en PagaFacil", order.Id);
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

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<JsonElement>(content);
                    return new PaymentProviderResponse
                    {
                        Success = true,
                        OrderNumber = providerOrderNumber,
                        Amount = ExtractAmount(result, 0),
                        Status = ExtractStatus(result),
                        Message = "Orden consultada exitosamente en PagaFacil",
                        Timestamp = DateTime.UtcNow,
                        ProviderSpecificData = result
                    };
                }

                return new PaymentProviderResponse
                {
                    Success = false,
                    Message = $"Error consultando orden en PagaFacil: {response.StatusCode}",
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consultando orden {OrderNumber} en PagaFacil", providerOrderNumber);
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
                var response = await _httpClient.PutAsync($"/cancel?orderId={providerOrderNumber}", null);

                return new PaymentProviderResponse
                {
                    Success = response.IsSuccessStatusCode,
                    OrderNumber = providerOrderNumber,
                    Status = response.IsSuccessStatusCode ? OrderStatus.Cancelled : OrderStatus.Failed,
                    Message = response.IsSuccessStatusCode ? "Orden cancelada exitosamente en PagaFacil" : "Error cancelando orden en PagaFacil",
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelando orden {OrderNumber} en PagaFacil", providerOrderNumber);
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
                var response = await _httpClient.PutAsync($"/pay?orderId={providerOrderNumber}", null);

                return new PaymentProviderResponse
                {
                    Success = response.IsSuccessStatusCode,
                    OrderNumber = providerOrderNumber,
                    Status = response.IsSuccessStatusCode ? OrderStatus.Paid : OrderStatus.Failed,
                    Message = response.IsSuccessStatusCode ? "Orden pagada exitosamente en PagaFacil" : "Error pagando orden en PagaFacil",
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pagando orden {OrderNumber} en PagaFacil", providerOrderNumber);
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

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<JsonElement>(content);

                    var orders = new List<PaymentProviderResponse>();

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
                _logger.LogError(ex, "Error obteniendo todas las órdenes de PagaFacil");
                return Enumerable.Empty<PaymentProviderResponse>();
            }
        }

        // Métodos auxiliares privados
        private PaymentProviderResponse CreatePaymentProviderResponse(JsonElement item)
        {
            return new PaymentProviderResponse
            {
                Success = true,
                OrderNumber = ExtractOrderId(item),
                Amount = ExtractAmount(item, 0),
                Status = ExtractStatus(item),
                Message = "Respuesta de PagaFacil",
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
                _ => throw new NotSupportedException($"PagaFacil no soporta: {method}")
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