using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PaymentOptimizer.Domain.Entities;
using PaymentOptimizer.Domain.Enums;
using PaymentOptimizer.Domain.Interfaces.Services;

namespace PaymentOptimizer.Infrastructure.PaymentProviders
{
    /// <summary>
    /// Proveedor de pago simulado para pruebas y desarrollo.
    /// 
    /// Este MockProvider actúa como un "simulador" que imita el comportamiento
    /// de un proveedor de pago real sin hacer llamadas HTTP reales a APIs externas.
    /// Es extremadamente útil para:
    /// 
    /// 1. Desarrollo: Permite trabajar sin depender de APIs externas
    /// 2. Pruebas: Garantiza resultados predecibles y controlados
    /// 3. Debugging: Facilita la identificación de problemas en nuestra lógica
    /// 4. Demos: Permite mostrar funcionalidad sin costos de transacciones reales
    /// 
    /// Piensa en esto como un "coche de práctica" en una escuela de manejo:
    /// tiene todos los controles y se comporta como un coche real, pero está
    /// diseñado específicamente para el aprendizaje y la práctica segura.
    /// </summary>
    public class MockPaymentProvider : IPaymentProvider
    {
        /// <summary>
        /// Nombre identificador de este proveedor mock.
        /// Útil para logging y debugging cuando necesites saber qué proveedor se está usando.
        /// </summary>
        public string ProviderName => "MockProvider";

        /// <summary>
        /// Simula la verificación de soporte para métodos de pago.
        /// 
        /// En un provider real, esto podría hacer una llamada API para verificar
        /// qué métodos están disponibles. Nuestro mock simplifica esto soportando
        /// todos los métodos principales para máxima flexibilidad en pruebas.
        /// </summary>
        /// <param name="method">Método de pago a verificar</param>
        /// <returns>true si el método es soportado, false en caso contrario</returns>
        public bool SupportsPaymentMethod(PaymentMethod method)
        {
            // El mock provider soporta todos los métodos de pago principales
            // Esto nos da flexibilidad máxima para probar diferentes escenarios
            return method switch
            {
                PaymentMethod.Cash => true,
                PaymentMethod.CreditCard => true,
                PaymentMethod.DebitCard => true,
                PaymentMethod.BankTransfer => true,
                _ => false
            };
        }

        /// <summary>
        /// Simula el cálculo de comisiones para diferentes métodos de pago.
        /// 
        /// En un sistema real, estas comisiones podrían venir de una API,
        /// una base de datos, o ser calculadas usando algoritmos complejos
        /// basados en el volumen de transacciones, el tipo de cliente, etc.
        /// 
        /// Nuestro mock usa valores simples pero realistas que nos permiten
        /// probar toda la lógica de selección de proveedores sin complicaciones.
        /// </summary>
        /// <param name="order">Orden para la cual calcular la comisión</param>
        /// <returns>Monto de la comisión a cobrar</returns>
        public async Task<decimal> CalculateCommissionAsync(Order order)
        {
            // Simular una pequeña latencia como tendría un proveedor real
            await Task.Delay(10);

            // Comisiones simuladas basadas en el método de pago
            // Estos valores están diseñados para ser competitivos pero no siempre los mejores,
            // permitiendo probar la lógica de selección del mejor proveedor
            return order.PaymentMethod switch
            {
                PaymentMethod.Cash => 5.0m, // Comisión fija baja para efectivo
                PaymentMethod.CreditCard => order.Amount * 0.02m, // 2% para tarjeta de crédito
                PaymentMethod.DebitCard => order.Amount * 0.015m, // 1.5% para tarjeta de débito
                PaymentMethod.BankTransfer => 3.0m, // Comisión fija muy baja para transferencias
                _ => throw new NotSupportedException($"MockProvider no soporta el método de pago: {order.PaymentMethod}")
            };
        }

        /// <summary>
        /// Simula la creación de una orden en el sistema del proveedor.
        /// 
        /// Un proveedor real haría una llamada HTTP POST a su API, enviaría
        /// los datos de la orden, y recibiría una respuesta con el ID de la orden
        /// y otra información relevante.
        /// 
        /// Nuestro mock simula este proceso creando una respuesta realista
        /// que incluye todos los campos que esperaríamos de un proveedor real.
        /// </summary>
        /// <param name="order">Orden a crear en el sistema del proveedor</param>
        /// <returns>Respuesta simulada del proveedor con detalles de la orden creada</returns>
        public async Task<PaymentProviderResponse> CreateOrderAsync(Order order)
        {
            // Simular latencia de red típica de una API real
            await Task.Delay(100);

            // Simular ocasionalmente un "error de red" para probar manejo de errores
            // En un entorno de pruebas real, podrías controlar esto con configuración
            var random = new Random();
            if (random.Next(1, 100) <= 2) // 2% de probabilidad de error simulado
            {
                return new PaymentProviderResponse
                {
                    Success = false,
                    Message = "Error simulado de red para pruebas de robustez",
                    Timestamp = DateTime.UtcNow
                };
            }

            // Generar un ID de orden simulado que se vea realista
            var mockOrderId = $"MOCK-{DateTime.UtcNow:yyyyMMdd}-{random.Next(10000, 99999)}";

            return new PaymentProviderResponse
            {
                Success = true,
                OrderNumber = mockOrderId,
                Amount = order.Amount,
                Status = OrderStatus.Created,
                Message = $"Orden creada exitosamente en MockProvider con ID: {mockOrderId}",
                Timestamp = DateTime.UtcNow,
                // ProviderSpecificData podría incluir datos adicionales que un proveedor real retornaría
                ProviderSpecificData = new
                {
                    mockProviderId = mockOrderId,
                    estimatedProcessingTime = "2-5 minutos",
                    mockApiVersion = "1.0",
                    testMode = true
                }
            };
        }

        /// <summary>
        /// Simula la consulta del estado de una orden existente.
        /// En un proveedor real, esto haría una llamada GET a su API.
        /// </summary>
        public async Task<PaymentProviderResponse> GetOrderAsync(string providerOrderNumber)
        {
            await Task.Delay(50);

            // Simular que encontramos la orden (en un sistema real, podría no existir)
            return new PaymentProviderResponse
            {
                Success = true,
                OrderNumber = providerOrderNumber,
                Amount = 100.00m, // En un sistema real, esto vendría de la base de datos del proveedor
                Status = OrderStatus.Created,
                Message = "Orden consultada exitosamente en MockProvider",
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Simula la cancelación de una orden.
        /// En un proveedor real, esto haría una llamada PUT o DELETE a su API.
        /// </summary>
        public async Task<PaymentProviderResponse> CancelOrderAsync(string providerOrderNumber)
        {
            await Task.Delay(75);

            return new PaymentProviderResponse
            {
                Success = true,
                OrderNumber = providerOrderNumber,
                Status = OrderStatus.Cancelled,
                Message = $"Orden {providerOrderNumber} cancelada exitosamente en MockProvider",
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Simula el procesamiento de pago de una orden.
        /// En un proveedor real, esto iniciaría el proceso de cobro al cliente.
        /// </summary>
        public async Task<PaymentProviderResponse> PayOrderAsync(string providerOrderNumber)
        {
            await Task.Delay(200); // Simular que el pago toma más tiempo que otras operaciones

            return new PaymentProviderResponse
            {
                Success = true,
                OrderNumber = providerOrderNumber,
                Status = OrderStatus.Paid,
                Message = $"Orden {providerOrderNumber} pagada exitosamente en MockProvider",
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Simula la obtención de todas las órdenes del proveedor.
        /// En un proveedor real, esto podría retornar cientos o miles de órdenes.
        /// </summary>
        public async Task<IEnumerable<PaymentProviderResponse>> GetAllOrdersAsync()
        {
            await Task.Delay(150);

            // Simular algunas órdenes de ejemplo
            var mockOrders = new List<PaymentProviderResponse>
            {
                new PaymentProviderResponse
                {
                    Success = true,
                    OrderNumber = "MOCK-20241201-12345",
                    Amount = 150.00m,
                    Status = OrderStatus.Paid,
                    Message = "Orden de ejemplo 1",
                    Timestamp = DateTime.UtcNow.AddHours(-2)
                },
                new PaymentProviderResponse
                {
                    Success = true,
                    OrderNumber = "MOCK-20241201-12346",
                    Amount = 75.50m,
                    Status = OrderStatus.Created,
                    Message = "Orden de ejemplo 2",
                    Timestamp = DateTime.UtcNow.AddMinutes(-30)
                }
            };

            return mockOrders;
        }
    }
}