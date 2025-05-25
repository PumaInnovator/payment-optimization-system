namespace PaymentOptimizer.Domain.Entities
{
    /// <summary>
    /// Representa una comisión aplicada a una orden.
    /// Las comisiones son cargos adicionales añadidos por los proveedores de pago.
    /// </summary>
    public class Fee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
    }
}