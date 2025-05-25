namespace PaymentOptimizer.Domain.Enums
{
    /// <summary>
    /// Representa los diferentes estados posibles de una orden de pago.
    /// Esta enumeración define el ciclo de vida completo de una orden.
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// La orden ha sido creada inicialmente pero aún no se ha enviado a ningún proveedor
        /// </summary>
        Created = 1,

        /// <summary>
        /// La orden ha sido enviada a un proveedor y está en proceso de pago
        /// </summary>
        Processing = 2,

        /// <summary>
        /// La orden ha sido pagada exitosamente
        /// </summary>
        Paid = 3,

        /// <summary>
        /// La orden ha sido cancelada y no se procesará
        /// </summary>
        Cancelled = 4,

        /// <summary>
        /// La orden ha fallado durante el procesamiento
        /// </summary>
        Failed = 5
    }
}