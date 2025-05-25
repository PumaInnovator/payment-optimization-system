using System;
using System.Collections.Generic;
using PaymentOptimizer.Domain.Enums;

namespace PaymentOptimizer.Domain.Models
{
    /// <summary>
    /// Modelo para solicitudes de pago a APIs externas.
    /// Esta clase encapsula toda la información necesaria para crear una orden
    /// en cualquier proveedor de pago integrado en el sistema.
    /// </summary>
    public class PaymentApiRequest
    {
        /// <summary>
        /// Identificador único de la orden en nuestro sistema.
        /// </summary>
        public string OrderId { get; set; }

        /// <summary>
        /// Monto total de la transacción.
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Método de pago seleccionado para esta transacción.
        /// </summary>
        public PaymentMethod PaymentMethod { get; set; }

        /// <summary>
        /// Lista de productos incluidos en esta orden.
        /// </summary>
        public List<PaymentProduct> Products { get; set; } = new List<PaymentProduct>();

        /// <summary>
        /// Código de moneda (por defecto USD).
        /// </summary>
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// Descripción adicional de la transacción.
        /// </summary>
        public string Description { get; set; }
    }

    /// <summary>
    /// Representa un producto dentro de una solicitud de pago.
    /// Esta estructura es compatible con la mayoría de APIs de pago comerciales.
    /// </summary>
    public class PaymentProduct
    {
        /// <summary>
        /// Nombre del producto o servicio.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Precio unitario del producto.
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Cantidad de unidades del producto.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Precio total calculado (UnitPrice * Quantity).
        /// </summary>
        public decimal TotalPrice => UnitPrice * Quantity;

        /// <summary>
        /// Descripción detallada del producto.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Categoría del producto para fines de clasificación.
        /// </summary>
        public string Category { get; set; }
    }

    /// <summary>
    /// Modelo unificado para manejar información de pago.
    /// Esta clase actúa como puente entre nuestros modelos de dominio
    /// y los formatos requeridos por las APIs externas.
    /// </summary>
    public class PaymentModel
    {
        /// <summary>
        /// Identificador único del pago.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Monto del pago.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Método de pago utilizado.
        /// </summary>
        public PaymentMethod Method { get; set; }

        /// <summary>
        /// Estado actual del pago.
        /// </summary>
        public Domain.Enums.OrderStatus Status { get; set; }

        /// <summary>
        /// Nombre del proveedor que procesó el pago.
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// Identificador de la orden en el sistema del proveedor.
        /// </summary>
        public string ProviderOrderId { get; set; }

        /// <summary>
        /// Fecha y hora de creación del pago.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Fecha y hora de última actualización.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Lista de productos incluidos en el pago.
        /// </summary>
        public List<PaymentProduct> Items { get; set; } = new List<PaymentProduct>();
    }
}