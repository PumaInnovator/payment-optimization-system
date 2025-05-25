using System;

namespace PaymentOptimizer.Domain.Enums
{
    /// <summary>
    /// Define los métodos de pago soportados por el sistema PaymentOptimizer.
    /// Esta enumeración es la única fuente de verdad para los tipos de pago.
    /// </summary>
    public enum PaymentMethod
    {
        /// <summary>
        /// Pago en efectivo procesado a través de puntos físicos o redes de cobranza.
        /// </summary>
        Cash = 0,

        /// <summary>
        /// Pago con tarjeta de crédito procesado electrónicamente.
        /// </summary>
        CreditCard = 1,

        /// <summary>
        /// Pago con tarjeta de débito con cargo directo a cuenta bancaria.
        /// </summary>
        DebitCard = 2,

        /// <summary>
        /// Transferencia bancaria directa entre cuentas.
        /// </summary>
        BankTransfer = 3
    }
}