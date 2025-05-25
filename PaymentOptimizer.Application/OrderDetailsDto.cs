namespace PaymentOptimizer.Application.DTOs
{
    /// <summary>
    /// DTO que representa una vista detallada de una orden.
    /// Se utiliza cuando se necesita información más completa para mostrar
    /// los detalles de una orden específica.
    /// </summary>
    public class OrderDetailsDto : OrderDto
    {
        /// <summary>
        /// Información adicional sobre la comisión del proveedor
        /// </summary>
        public string CommissionDetails { get; set; }

        /// <summary>
        /// Detalles sobre el proceso de pago
        /// </summary>
        public string PaymentDetails { get; set; }

        /// <summary>
        /// Motivo de cancelación (si la orden fue cancelada)
        /// </summary>
        public string CancellationReason { get; set; }

        /// <summary>
        /// Información de seguimiento para la orden
        /// </summary>
        public string TrackingInformation { get; set; }
    }
}