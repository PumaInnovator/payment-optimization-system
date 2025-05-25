using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PaymentOptimizer.Domain.Enums;
using PaymentOptimizer.Domain.Exceptions;

namespace PaymentOptimizer.Domain.Entities
{
    /// <summary>
    /// Entidad principal que representa una orden de pago en el sistema.
    /// Contiene toda la información y comportamiento relacionado con las órdenes.
    /// </summary>
    public class Order
    {
        // Propiedades principales
        public int Id { get; private set; }
        public decimal Amount { get; private set; }
        public PaymentMethod PaymentMethod { get; private set; }
        public OrderStatus Status { get; private set; }
        public string ProviderOrderNumber { get; private set; }
        public string SelectedProvider { get; private set; }
        public List<OrderItem> Items { get; private set; } = new List<OrderItem>();
        public List<Fee> Fees { get; private set; } = new List<Fee>();
        public DateTime CreatedAt { get; private set; }
        public DateTime? PaidAt { get; private set; }
        public DateTime? CancelledAt { get; private set; }

        // Constructor protegido para EF Core
        protected Order() { }

        /// <summary>
        /// Constructor principal para crear una nueva orden
        /// </summary>
        /// <param name="paymentMethod">El método de pago seleccionado</param>
        /// <param name="items">Los ítems que componen la orden</param>
        public Order(PaymentMethod paymentMethod, List<OrderItem> items)
        {
            if (items == null || !items.Any())
                throw new DomainException("Una orden debe tener al menos un ítem");

            PaymentMethod = paymentMethod;
            Items = items;
            Status = OrderStatus.Created;
            CreatedAt = DateTime.UtcNow;
            RecalculateAmount();
        }

        /// <summary>
        /// Asigna la orden a un proveedor específico, cambiando su estado a Processing
        /// </summary>
        /// <param name="providerName">Nombre del proveedor seleccionado</param>
        /// <param name="providerOrderNumber">Número de referencia asignado por el proveedor</param>
        public void AssignToProvider(string providerName, string providerOrderNumber)
        {
            if (Status != OrderStatus.Created)
                throw new InvalidOperationDomainException("Solo órdenes en estado Created pueden asignarse a un proveedor");

            SelectedProvider = providerName ?? throw new DomainException("El nombre del proveedor no puede ser nulo");
            ProviderOrderNumber = providerOrderNumber ?? throw new DomainException("El número de orden del proveedor no puede ser nulo");
            Status = OrderStatus.Processing;
        }

        /// <summary>
        /// Marca la orden como pagada, actualizando su estado y registrando la fecha de pago
        /// </summary>
        public void MarkAsPaid()
        {
            if (Status != OrderStatus.Processing)
                throw new InvalidOperationDomainException("Solo órdenes en estado Processing pueden marcarse como pagadas");

            Status = OrderStatus.Paid;
            PaidAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Cancela la orden si su estado actual lo permite
        /// </summary>
        public void Cancel()
        {
            if (Status == OrderStatus.Paid || Status == OrderStatus.Cancelled)
                throw new InvalidOperationDomainException("No se pueden cancelar órdenes pagadas o ya canceladas");

            Status = OrderStatus.Cancelled;
            CancelledAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Marca la orden como fallida, generalmente debido a errores en el proveedor
        /// </summary>
        /// <param name="reason">Motivo del fallo</param>
        public void MarkAsFailed(string reason)
        {
            if (Status == OrderStatus.Paid || Status == OrderStatus.Cancelled)
                throw new InvalidOperationDomainException("No se pueden marcar como fallidas órdenes pagadas o canceladas");

            Status = OrderStatus.Failed;
        }

        /// <summary>
        /// Añade una comisión a la orden y recalcula el monto total
        /// </summary>
        /// <param name="name">Nombre descriptivo de la comisión</param>
        /// <param name="amount">Monto de la comisión</param>
        public void AddFee(string name, decimal amount)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("El nombre de la comisión no puede estar vacío");

            if (amount < 0)
                throw new DomainException("El monto de la comisión no puede ser negativo");

            Fees.Add(new Fee { Name = name, Amount = amount });
            RecalculateAmount();
        }

        /// <summary>
        /// Recalcula el monto total de la orden basado en ítems y comisiones
        /// </summary>
        private void RecalculateAmount()
        {
            decimal itemsTotal = Items.Sum(i => i.UnitPrice * i.Quantity);
            decimal feesTotal = Fees.Sum(f => f.Amount);
            Amount = itemsTotal + feesTotal;
        }
        

        public void SetId(int id)
        {
            if (Id != 0)
                throw new InvalidOperationDomainException("El Id de la orden ya ha sido establecido y no puede modificarse");

            if (id <= 0)
                throw new DomainException("El Id debe ser un número positivo");

            // Usar reflection de manera controlada
            var idProperty = typeof(Order).GetProperty(nameof(Id));
            if (idProperty != null)
            {
                var backingField = typeof(Order).GetField("<Id>k__BackingField",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (backingField != null)
                {
                    backingField.SetValue(this, id);
                }
            }
        }
    }
}