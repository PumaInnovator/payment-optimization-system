namespace PaymentOptimizer.Domain.Entities
{
    /// <summary>
    /// Representa un ítem individual dentro de una orden de pago.
    /// Contiene información sobre el producto, precio y cantidad.
    /// </summary>
    public class OrderItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }

        /// <summary>
        /// Calcula el subtotal para este ítem (precio unitario * cantidad)
        /// </summary>
        public decimal Subtotal => UnitPrice * Quantity;
    }
}