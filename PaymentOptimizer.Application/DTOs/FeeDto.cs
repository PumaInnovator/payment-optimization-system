namespace PaymentOptimizer.Application.DTOs
{
    /// <summary>
    /// DTO que representa una comisión aplicada a una orden.
    /// Permite transferir información sobre cargos adicionales.
    /// </summary>
    public class FeeDto
    {
        /// <summary>
        /// Nombre descriptivo de la comisión
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Monto de la comisión
        /// </summary>
        public decimal Amount { get; set; }
    }
}