using System;
using System.Collections.Generic;

namespace PaymentOptimizer.Application.DTOs
{
    /// <summary>
    /// DTO que representa una orden para ser expuesta a través de la API.
    /// Este objeto es una versión simplificada y "plana" de la entidad Order
    /// diseñada específicamente para transferir datos al cliente.
    /// </summary>
    public class OrderDto
    {
        /// <summary>
        /// Identificador único de la orden
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Monto total de la orden incluyendo productos y comisiones
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Estado actual de la orden (Created, Processing, Paid, Cancelled, Failed)
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Método de pago utilizado (Cash, CreditCard, BankTransfer)
        /// </summary>
        public string PaymentMethod { get; set; }

        /// <summary>
        /// Nombre del proveedor de pago seleccionado para procesar esta orden
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// Número de referencia asignado por el proveedor de pago
        /// </summary>
        public string ProviderOrderNumber { get; set; }

        /// <summary>
        /// Lista de productos incluidos en la orden
        /// </summary>
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();

        /// <summary>
        /// Lista de comisiones aplicadas a la orden
        /// </summary>
        public List<FeeDto> Fees { get; set; } = new List<FeeDto>();

        /// <summary>
        /// Fecha y hora de creación de la orden
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Fecha y hora en que se marcó como pagada la orden (si aplica)
        /// </summary>
        public DateTime? PaidAt { get; set; }

        /// <summary>
        /// Fecha y hora en que se canceló la orden (si aplica)
        /// </summary>
        public DateTime? CancelledAt { get; set; }
    }
}