using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PaymentOptimizer.Domain.Entities;
using PaymentOptimizer.Domain.Enums;
using PaymentOptimizer.Domain.Interfaces.Services;

namespace PaymentOptimizer.Tests.Mocks
{
    /// <summary>
    /// Implementación de IPaymentProvider para pruebas que simula las respuestas
    /// sin realizar llamadas HTTP reales.
    /// </summary>
    public class MockPaymentProvider : IPaymentProvider
    {
        public string ProviderName => "MockProvider";

        // Opcional: propiedades para rastrear llamadas durante las pruebas
        public int CalculateCommissionCalls { get; private set; } = 0;
        public int CreateOrderCalls { get; private set; } = 0;
        public int GetOrderCalls { get; private set; } = 0;
        public List<Order> CreatedOrders { get; } = new List<Order>();

        // Permite personalizar respuestas durante las pruebas
        public bool SimulateFailure { get; set; } = false;
        public decimal CustomCommissionAmount { get; set; } = 10m;

        public bool SupportsPaymentMethod(PaymentMethod method)
        {
            // Simular que soporta todos los métodos de pago para simplificar pruebas
            return true;
        }

        public Task<decimal> CalculateCommissionAsync(Order order)
        {
            CalculateCommissionCalls++;

            // Cálculo simulado de comisión para pruebas
            return Task.FromResult(order.PaymentMethod switch
            {
                PaymentMethod.Cash => 15m,
                PaymentMethod.CreditCard => order.Amount * 0.03m,
                PaymentMethod.BankTransfer => 5m,
                _ => CustomCommissionAmount
            });
        }

        public Task<PaymentProviderResponse> CreateOrderAsync(Order order)
        {
            CreateOrderCalls++;
            CreatedOrders.Add(order);

            if (SimulateFailure)
            {
                return Task.FromResult(new PaymentProviderResponse
                {
                    Success = false,
                    Message = "Error simulado en pruebas",
                    Timestamp = DateTime.UtcNow
                });
            }

            return Task.FromResult(new PaymentProviderResponse
            {
                Success = true,
                OrderNumber = $"MOCK-{Guid.NewGuid()}",
                Amount = order.Amount,
                Status = OrderStatus.Created,
                Message = "Orden creada exitosamente (simulada)",
                Timestamp = DateTime.UtcNow
            });
        }

        public Task<PaymentProviderResponse> GetOrderAsync(string providerOrderNumber)
        {
            GetOrderCalls++;

            return Task.FromResult(new PaymentProviderResponse
            {
                Success = true,
                OrderNumber = providerOrderNumber,
                Amount = 100m, // Monto fijo para pruebas
                Status = OrderStatus.Created,
                Message = "Orden consultada exitosamente (simulada)",
                Timestamp = DateTime.UtcNow
            });
        }

        public Task<PaymentProviderResponse> CancelOrderAsync(string providerOrderNumber)
        {
            return Task.FromResult(new PaymentProviderResponse
            {
                Success = true,
                OrderNumber = providerOrderNumber,
                Status = OrderStatus.Cancelled,
                Message = "Orden cancelada exitosamente (simulada)",
                Timestamp = DateTime.UtcNow
            });
        }

        public Task<PaymentProviderResponse> PayOrderAsync(string providerOrderNumber)
        {
            return Task.FromResult(new PaymentProviderResponse
            {
                Success = true,
                OrderNumber = providerOrderNumber,
                Status = OrderStatus.Paid,
                Message = "Orden marcada como pagada exitosamente (simulada)",
                Timestamp = DateTime.UtcNow
            });
        }

        public Task<IEnumerable<PaymentProviderResponse>> GetAllOrdersAsync()
        {
            var orders = new List<PaymentProviderResponse>
            {
                new PaymentProviderResponse
                {
                    Success = true,
                    OrderNumber = $"MOCK-{Guid.NewGuid()}",
                    Amount = 100m,
                    Status = OrderStatus.Created,
                    Timestamp = DateTime.UtcNow
                },
                new PaymentProviderResponse
                {
                    Success = true,
                    OrderNumber = $"MOCK-{Guid.NewGuid()}",
                    Amount = 200m,
                    Status = OrderStatus.Paid,
                    Timestamp = DateTime.UtcNow
                }
            };

            return Task.FromResult<IEnumerable<PaymentProviderResponse>>(orders);
        }
    }
}