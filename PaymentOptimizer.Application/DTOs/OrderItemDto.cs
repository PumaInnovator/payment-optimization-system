namespace PaymentOptimizer.Application.DTOs
{
    /// <summary>
    /// DTO que representa un ítem dentro de una orden.
    /// Simplifica la transferencia de información de productos en una orden.
    /// </summary>
    public class OrderItemDto
    {
        /// <summary>
        /// Nombre del producto
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Precio unitario del producto
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Cantidad de unidades del producto
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Subtotal calculado para este ítem (precio × cantidad)
        /// </summary>
        public decimal Subtotal { get; set; }
    }
}