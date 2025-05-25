using System;

namespace PaymentOptimizer.Domain.Exceptions
{
    /// <summary>
    /// Excepción que se lanza cuando se intenta realizar una operación inválida según las reglas de negocio.
    /// </summary>
    public class InvalidOperationDomainException : DomainException
    {
        public InvalidOperationDomainException()
        {
        }

        public InvalidOperationDomainException(string message)
            : base(message)
        {
        }

        public InvalidOperationDomainException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}