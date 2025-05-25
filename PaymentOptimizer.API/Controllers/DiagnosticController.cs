using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PaymentOptimizer.Domain.Entities;
using PaymentOptimizer.Domain.Enums;
using PaymentOptimizer.Domain.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentOptimizer.API.Controllers
{
    /// <summary>
    /// Controlador de diagnóstico corregido para eliminar errores CS0173.
    /// 
    /// Los errores CS0173 ocurren cuando C# no puede determinar un tipo común
    /// para las expresiones ternarias que usan objetos anónimos con estructuras diferentes.
    /// 
    /// Solución aplicada: Todos los objetos anónimos en expresiones ternarias
    /// mantienen la misma estructura, usando valores null cuando las propiedades
    /// no son aplicables en ciertos escenarios.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DiagnosticController : ControllerBase
    {
        // Diccionario en memoria para tracking de órdenes durante pruebas
        private static readonly Dictionary<string, (Order order, string provider, string providerOrderId)> _testOrders
            = new Dictionary<string, (Order, string, string)>();

        private readonly ILogger<DiagnosticController> _logger;

        public DiagnosticController(ILogger<DiagnosticController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Endpoint básico de salud del sistema.
        /// </summary>
        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                service = "PaymentOptimizer CRUD Testing Suite",
                version = "1.0.0",
                testOrdersCount = _testOrders.Count
            });
        }

        /// <summary>
        /// OPERACIÓN CREATE: Prueba comprehensiva de creación de órdenes.
        /// 
        /// Esta versión elimina los problemas de tipos en expresiones condicionales
        /// manteniendo estructuras consistentes en todos los objetos de respuesta.
        /// </summary>
        [HttpPost("crud/create/{provider}")]
        public async Task<IActionResult> TestCreateOperation(string provider)
        {
            try
            {
                _logger.LogInformation("=== INICIANDO PRUEBA CREATE ===");
                _logger.LogInformation("Proveedor solicitado: {Provider}", provider);

                // Obtener el proveedor específico
                var paymentProviders = HttpContext.RequestServices.GetServices<IPaymentProvider>();
                var selectedProvider = paymentProviders.FirstOrDefault(p =>
                    p.ProviderName.Equals(provider, StringComparison.OrdinalIgnoreCase));

                if (selectedProvider == null)
                {
                    return BadRequest(new
                    {
                        operation = "CREATE",
                        success = false,
                        error = $"Proveedor '{provider}' no encontrado",
                        availableProviders = paymentProviders.Select(p => p.ProviderName).ToArray(),
                        provider = (string)null,
                        localOrder = (object)null,
                        commission = (decimal?)null,
                        totalCost = (decimal?)null,
                        providerResponse = (object)null,
                        trackingInfo = (object)null,
                        nextSteps = (string)null
                    });
                }

                // Crear orden de prueba
                var orderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        Name = "Laptop Dell Inspiron",
                        UnitPrice = 1200.00m,
                        Quantity = 1
                    },
                    new OrderItem
                    {
                        Name = "Mouse Inalámbrico",
                        UnitPrice = 25.50m,
                        Quantity = 2
                    }
                };

                var testOrder = new Order(PaymentMethod.CreditCard, orderItems);

                _logger.LogInformation("Orden creada localmente - ID: {OrderId}, Monto: {Amount}",
                    testOrder.Id, testOrder.Amount);

                // Calcular comisión
                var commission = await selectedProvider.CalculateCommissionAsync(testOrder);
                _logger.LogInformation("Comisión calculada: {Commission}", commission);

                // Enviar al proveedor
                var createResult = await selectedProvider.CreateOrderAsync(testOrder);
                _logger.LogInformation("Resultado de creación - Éxito: {Success}, Mensaje: {Message}",
                    createResult.Success, createResult.Message);

                // Guardar para tracking si fue exitoso
                string trackingKey = null;
                if (createResult.Success && !string.IsNullOrEmpty(createResult.OrderNumber))
                {
                    trackingKey = $"{provider}_{testOrder.Id}";
                    _testOrders[trackingKey] = (testOrder, provider, createResult.OrderNumber);
                    _logger.LogInformation("Orden guardada para tracking con clave: {TrackingKey}", trackingKey);
                }

                _logger.LogInformation("=== PRUEBA CREATE COMPLETADA ===");

                // CORRECCIÓN: Estructura consistente sin usar operador ternario problemático
                return Ok(new
                {
                    operation = "CREATE",
                    success = createResult.Success,
                    error = createResult.Success ? null : "Error en la creación de la orden",
                    availableProviders = (string[])null, // Solo se llena en caso de error de proveedor no encontrado
                    provider = selectedProvider.ProviderName,
                    localOrder = new
                    {
                        id = testOrder.Id,
                        amount = testOrder.Amount,
                        paymentMethod = testOrder.PaymentMethod.ToString(),
                        itemCount = testOrder.Items.Count,
                        items = testOrder.Items.Select(item => new {
                            name = item.Name,
                            unitPrice = item.UnitPrice,
                            quantity = item.Quantity,
                            totalPrice = item.UnitPrice * item.Quantity
                        }).ToArray()
                    },
                    commission = commission,
                    totalCost = testOrder.Amount + commission,
                    providerResponse = new
                    {
                        success = createResult.Success,
                        providerOrderId = createResult.OrderNumber,
                        status = createResult.Status.ToString(),
                        message = createResult.Message,
                        timestamp = createResult.Timestamp
                    },
                    trackingInfo = trackingKey != null ? new
                    {
                        trackingKey = trackingKey,
                        message = "Usa este trackingKey para probar otras operaciones CRUD"
                    } : null,
                    nextSteps = createResult.Success ?
                        "Ahora puedes probar READ, UPDATE, o DELETE usando el providerOrderId" :
                        "Revisa el mensaje de error para diagnosticar el problema"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante prueba CREATE con {Provider}", provider);
                return StatusCode(500, new
                {
                    operation = "CREATE",
                    success = false,
                    error = ex.Message,
                    availableProviders = (string[])null,
                    provider = provider,
                    localOrder = (object)null,
                    commission = (decimal?)null,
                    totalCost = (decimal?)null,
                    providerResponse = (object)null,
                    trackingInfo = (object)null,
                    nextSteps = "Revisa los logs para más detalles del error"
                });
            }
        }

        /// <summary>
        /// OPERACIÓN READ: Prueba comprehensiva de consulta de órdenes.
        /// 
        /// Esta versión corregida evita los errores CS0173 usando estructuras
        /// consistentes en lugar de objetos anónimos condicionales.
        /// </summary>
        [HttpGet("crud/read/{provider}/{providerOrderId}")]
        public async Task<IActionResult> TestReadOperation(string provider, string providerOrderId)
        {
            try
            {
                _logger.LogInformation("=== INICIANDO PRUEBA READ ===");
                _logger.LogInformation("Proveedor: {Provider}, OrderId: {OrderId}", provider, providerOrderId);

                var paymentProviders = HttpContext.RequestServices.GetServices<IPaymentProvider>();
                var selectedProvider = paymentProviders.FirstOrDefault(p =>
                    p.ProviderName.Equals(provider, StringComparison.OrdinalIgnoreCase));

                if (selectedProvider == null)
                {
                    return BadRequest(new
                    {
                        operation = "READ",
                        success = false,
                        error = $"Proveedor '{provider}' no encontrado",
                        provider = (string)null,
                        searchCriteria = (object)null,
                        providerResponse = (object)null,
                        localInfo = (object)null,
                        analysis = "Error: Proveedor no disponible"
                    });
                }

                // Buscar información local
                var trackingKey = $"{provider}_{providerOrderId}";
                var hasLocalInfo = _testOrders.ContainsKey(trackingKey);

                _logger.LogInformation("Información local disponible: {HasLocal}", hasLocalInfo);

                // Consultar al proveedor
                var readResult = await selectedProvider.GetOrderAsync(providerOrderId);
                _logger.LogInformation("Resultado READ - Éxito: {Success}", readResult.Success);

                _logger.LogInformation("=== PRUEBA READ COMPLETADA ===");

                // CORRECCIÓN: Estructura consistente, evitando operadores ternarios problemáticos
                return Ok(new
                {
                    operation = "READ",
                    success = readResult.Success,
                    error = readResult.Success ? null : readResult.Message,
                    provider = selectedProvider.ProviderName,
                    searchCriteria = new
                    {
                        providerOrderId = providerOrderId,
                        trackingKey = trackingKey,
                        hadLocalInfo = hasLocalInfo
                    },
                    // Estructura consistente para providerResponse
                    providerResponse = new
                    {
                        // Propiedades que siempre están presentes
                        success = readResult.Success,
                        timestamp = readResult.Timestamp,
                        message = readResult.Message,
                        // Propiedades que pueden ser null si no hay éxito
                        orderNumber = readResult.Success ? readResult.OrderNumber : null,
                        amount = readResult.Success ? readResult.Amount : (decimal?)null,
                        status = readResult.Success ? readResult.Status.ToString() : null,
                        additionalData = readResult.Success ? readResult.ProviderSpecificData : null
                    },
                    localInfo = hasLocalInfo ? new
                    {
                        originalOrder = new
                        {
                            id = _testOrders[trackingKey].order.Id,
                            amount = _testOrders[trackingKey].order.Amount,
                            paymentMethod = _testOrders[trackingKey].order.PaymentMethod.ToString(),
                            createdAt = _testOrders[trackingKey].order.CreatedAt
                        }
                    } : null,
                    analysis = readResult.Success ?
                        "✅ Consulta exitosa - El proveedor mantiene información de la orden" :
                        $"❌ Error en consulta: {readResult.Message}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante prueba READ");
                return StatusCode(500, new
                {
                    operation = "READ",
                    success = false,
                    error = ex.Message,
                    provider = provider,
                    searchCriteria = (object)null,
                    providerResponse = (object)null,
                    localInfo = (object)null,
                    analysis = $"Error interno: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// OPERACIÓN CANCEL: Prueba de cancelación de órdenes.
        /// 
        /// Versión corregida que mantiene estructuras consistentes y evita
        /// los errores de tipos en expresiones condicionales.
        /// </summary>
        [HttpPut("crud/cancel/{provider}/{providerOrderId}")]
        public async Task<IActionResult> TestCancelOperation(string provider, string providerOrderId)
        {
            try
            {
                _logger.LogInformation("=== INICIANDO PRUEBA CANCEL (UPDATE) ===");
                _logger.LogInformation("Proveedor: {Provider}, OrderId: {OrderId}", provider, providerOrderId);

                var paymentProviders = HttpContext.RequestServices.GetServices<IPaymentProvider>();
                var selectedProvider = paymentProviders.FirstOrDefault(p =>
                    p.ProviderName.Equals(provider, StringComparison.OrdinalIgnoreCase));

                if (selectedProvider == null)
                {
                    return BadRequest(new
                    {
                        operation = "CANCEL",
                        success = false,
                        error = $"Proveedor '{provider}' no encontrado",
                        provider = (string)null,
                        orderFlow = (object)null,
                        analysis = "Error: Proveedor no disponible",
                        businessRules = (object)null
                    });
                }

                // Consultar estado actual
                var currentStateResult = await selectedProvider.GetOrderAsync(providerOrderId);
                _logger.LogInformation("Estado actual antes de cancelar - Éxito: {Success}, Estado: {Status}",
                    currentStateResult.Success, currentStateResult.Status);

                // Intentar cancelar
                var cancelResult = await selectedProvider.CancelOrderAsync(providerOrderId);
                _logger.LogInformation("Resultado CANCEL - Éxito: {Success}", cancelResult.Success);

                // Estado después de cancelar
                var afterCancelResult = await selectedProvider.GetOrderAsync(providerOrderId);
                _logger.LogInformation("Estado después de cancelar - Éxito: {Success}, Estado: {Status}",
                    afterCancelResult.Success, afterCancelResult.Status);

                _logger.LogInformation("=== PRUEBA CANCEL COMPLETADA ===");

                // CORRECCIÓN: Estructura consistente usando propiedades opcionales
                return Ok(new
                {
                    operation = "CANCEL",
                    success = cancelResult.Success,
                    error = cancelResult.Success ? null : cancelResult.Message,
                    provider = selectedProvider.ProviderName,
                    orderFlow = new
                    {
                        // Estado antes de cancelar - estructura consistente
                        beforeCancel = new
                        {
                            success = currentStateResult.Success,
                            status = currentStateResult.Success ? currentStateResult.Status.ToString() : null,
                            message = currentStateResult.Success ? currentStateResult.Message : "No se pudo consultar estado inicial",
                            error = currentStateResult.Success ? null : "Error consultando estado inicial"
                        },

                        // Operación de cancelación
                        cancelOperation = new
                        {
                            success = cancelResult.Success,
                            newStatus = cancelResult.Status.ToString(),
                            message = cancelResult.Message,
                            timestamp = cancelResult.Timestamp
                        },

                        // Estado después de cancelar - estructura consistente
                        afterCancel = new
                        {
                            success = afterCancelResult.Success,
                            status = afterCancelResult.Success ? afterCancelResult.Status.ToString() : null,
                            confirmed = afterCancelResult.Success ? afterCancelResult.Status == OrderStatus.Cancelled : (bool?)null,
                            error = afterCancelResult.Success ? null : "No se pudo verificar estado final"
                        }
                    },
                    analysis = cancelResult.Success ?
                        $"✅ Cancelación exitosa - Estado cambió a {cancelResult.Status}" :
                        $"❌ Error en cancelación: {cancelResult.Message}",
                    businessRules = new
                    {
                        message = "Esta operación revela las reglas de negocio del proveedor sobre cancelaciones",
                        observations = new[]
                        {
                            "¿Se puede cancelar en cualquier momento?",
                            "¿Hay restricciones basadas en el estado actual?",
                            "¿El proveedor confirma inmediatamente el cambio de estado?"
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante prueba CANCEL");
                return StatusCode(500, new
                {
                    operation = "CANCEL",
                    success = false,
                    error = ex.Message,
                    provider = provider,
                    orderFlow = (object)null,
                    analysis = $"Error interno: {ex.Message}",
                    businessRules = (object)null
                });
            }
        }

        /// <summary>
        /// OPERACIÓN PAYMENT: Prueba de marcado de órdenes como pagadas.
        /// 
        /// Versión corregida con estructura consistente.
        /// </summary>
        [HttpPut("crud/pay/{provider}/{providerOrderId}")]
        public async Task<IActionResult> TestPayOperation(string provider, string providerOrderId)
        {
            try
            {
                _logger.LogInformation("=== INICIANDO PRUEBA PAY ===");
                _logger.LogInformation("Proveedor: {Provider}, OrderId: {OrderId}", provider, providerOrderId);

                var paymentProviders = HttpContext.RequestServices.GetServices<IPaymentProvider>();
                var selectedProvider = paymentProviders.FirstOrDefault(p =>
                    p.ProviderName.Equals(provider, StringComparison.OrdinalIgnoreCase));

                if (selectedProvider == null)
                {
                    return BadRequest(new
                    {
                        operation = "PAY",
                        success = false,
                        error = $"Proveedor '{provider}' no encontrado",
                        provider = (string)null,
                        paymentFlow = (object)null,
                        analysis = "Error: Proveedor no disponible",
                        businessInsights = (object)null
                    });
                }

                // Estado antes del pago
                var beforePayResult = await selectedProvider.GetOrderAsync(providerOrderId);
                _logger.LogInformation("Estado antes del pago - Estado: {Status}", beforePayResult.Status);

                // Marcar como pagado
                var payResult = await selectedProvider.PayOrderAsync(providerOrderId);
                _logger.LogInformation("Resultado PAY - Éxito: {Success}", payResult.Success);

                // Estado después del pago
                var afterPayResult = await selectedProvider.GetOrderAsync(providerOrderId);
                _logger.LogInformation("Estado después del pago - Estado: {Status}", afterPayResult.Status);

                _logger.LogInformation("=== PRUEBA PAY COMPLETADA ===");

                // CORRECCIÓN: Estructura consistente
                return Ok(new
                {
                    operation = "PAY",
                    success = payResult.Success,
                    error = payResult.Success ? null : payResult.Message,
                    provider = selectedProvider.ProviderName,
                    paymentFlow = new
                    {
                        // Estructura consistente para antes del pago
                        beforePayment = new
                        {
                            success = beforePayResult.Success,
                            status = beforePayResult.Success ? beforePayResult.Status.ToString() : null,
                            amount = beforePayResult.Success ? beforePayResult.Amount : (decimal?)null,
                            error = beforePayResult.Success ? null : "No se pudo consultar estado inicial"
                        },

                        // Operación de pago
                        paymentOperation = new
                        {
                            success = payResult.Success,
                            newStatus = payResult.Status.ToString(),
                            message = payResult.Message,
                            timestamp = payResult.Timestamp
                        },

                        // Estructura consistente para después del pago
                        afterPayment = new
                        {
                            success = afterPayResult.Success,
                            status = afterPayResult.Success ? afterPayResult.Status.ToString() : null,
                            confirmed = afterPayResult.Success ? afterPayResult.Status == OrderStatus.Paid : (bool?)null,
                            error = afterPayResult.Success ? null : "No se pudo verificar estado final"
                        }
                    },
                    analysis = payResult.Success ?
                        $"✅ Pago registrado exitosamente - Estado: {payResult.Status}" :
                        $"❌ Error registrando pago: {payResult.Message}",
                    businessInsights = new
                    {
                        message = "Esta operación revela cómo cada proveedor maneja la confirmación de pagos",
                        keyQuestions = new[]
                        {
                            "¿El estado cambia inmediatamente a 'Paid'?",
                            "¿Hay estados intermedios como 'Processing'?",
                            "¿Se puede marcar como pagado desde cualquier estado?"
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante prueba PAY");
                return StatusCode(500, new
                {
                    operation = "PAY",
                    success = false,
                    error = ex.Message,
                    provider = provider,
                    paymentFlow = (object)null,
                    analysis = $"Error interno: {ex.Message}",
                    businessInsights = (object)null
                });
            }
        }

        /// <summary>
        /// Lista todas las órdenes rastreadas durante las pruebas.
        /// </summary>
        [HttpGet("crud/tracked-orders")]
        public IActionResult GetTrackedOrders()
        {
            var orders = _testOrders.Select(kvp => new
            {
                trackingKey = kvp.Key,
                localOrderId = kvp.Value.order.Id,
                provider = kvp.Value.provider,
                providerOrderId = kvp.Value.providerOrderId,
                amount = kvp.Value.order.Amount,
                paymentMethod = kvp.Value.order.PaymentMethod.ToString(),
                createdAt = kvp.Value.order.CreatedAt,
                itemCount = kvp.Value.order.Items.Count
            }).ToArray();

            return Ok(new
            {
                totalTrackedOrders = orders.Length,
                orders = orders,
                instructions = new
                {
                    message = "Usa los providerOrderId de estas órdenes para probar operaciones READ, CANCEL, y PAY",
                    endpoints = new
                    {
                        read = "/api/diagnostic/crud/read/{provider}/{providerOrderId}",
                        cancel = "/api/diagnostic/crud/cancel/{provider}/{providerOrderId}",
                        pay = "/api/diagnostic/crud/pay/{provider}/{providerOrderId}"
                    }
                }
            });
        }
    }
}