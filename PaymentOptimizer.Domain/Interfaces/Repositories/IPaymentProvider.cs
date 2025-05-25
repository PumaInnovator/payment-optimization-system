using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PaymentOptimizer.Domain.Entities;
using PaymentOptimizer.Domain.Enums;

namespace PaymentOptimizer.Domain.Interfaces.Services
{
    /// <summary>
    /// Define el contrato que cualquier proveedor de pago debe implementar.
    /// Esta interfaz es fundamental para nuestra arquitectura ya que permite
    /// tratar de manera uniforme a diferentes proveedores de pago.
    /// </summary>
    public interface IPaymentProvider
    {
        /// <summary>
        /// Nombre identificativo del proveedor
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// Calcula la comisión que cobraría este proveedor para una orden específica
        /// </summary>
        /// <param name="order">La orden para la que se calcula la comisión</param>
        /// <returns>El monto de la comisión</returns>
        Task<decimal> CalculateCommissionAsync(Order order);

        /// <summary>
        /// Verifica si este proveedor soporta un método de pago específico
        /// </summary>
        /// <param name="method">El método de pago a verificar</param>
        /// <returns>Verdadero si el proveedor soporta el método de pago</returns>
        bool SupportsPaymentMethod(PaymentMethod method);

        /// <summary>
        /// Crea una nueva orden en el sistema del proveedor
        /// </summary>
        /// <param name="order">Datos de la orden a crear</param>
        /// <returns>Respuesta del proveedor con el estado de la operación</returns>
        Task<PaymentProviderResponse> CreateOrderAsync(Order order);

        /// <summary>
        /// Obtiene información de una orden existente en el sistema del proveedor
        /// </summary>
        /// <param name="providerOrderNumber">Número de referencia de la orden en el proveedor</param>
        /// <returns>Respuesta del proveedor con los datos de la orden</returns>
        Task<PaymentProviderResponse> GetOrderAsync(string providerOrderNumber);

        /// <summary>
        /// Cancela una orden existente en el sistema del proveedor
        /// </summary>
        /// <param name="providerOrderNumber">Número de referencia de la orden en el proveedor</param>
        /// <returns>Respuesta del proveedor con el resultado de la operación</returns>
        Task<PaymentProviderResponse> CancelOrderAsync(string providerOrderNumber);

        /// <summary>
        /// Marca una orden como pagada en el sistema del proveedor
        /// </summary>
        /// <param name="providerOrderNumber">Número de referencia de la orden en el proveedor</param>
        /// <returns>Respuesta del proveedor con el resultado de la operación</returns>
        Task<PaymentProviderResponse> PayOrderAsync(string providerOrderNumber);

        /// <summary>
        /// Obtiene todas las órdenes existentes en el sistema del proveedor
        /// </summary>
        /// <returns>Lista de respuestas con los datos de las órdenes</returns>
        Task<IEnumerable<PaymentProviderResponse>> GetAllOrdersAsync();
    }

    /// <summary>
    /// Clase que representa la respuesta estandarizada de cualquier proveedor de pago
    /// </summary>
    public class PaymentProviderResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string OrderNumber { get; set; }
        public decimal Amount { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime Timestamp { get; set; }
        public object ProviderSpecificData { get; set; }
    }
}