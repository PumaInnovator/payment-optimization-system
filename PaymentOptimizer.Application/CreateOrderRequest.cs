using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PaymentOptimizer.Domain.Enums;

namespace PaymentOptimizer.Application.DTOs
{
    /// <summary>
    /// DTO que representa una solicitud para crear una nueva orden.
    /// Este objeto se recibe desde el cliente a través de la API y contiene
    /// toda la información necesaria para crear una nueva orden en el sistema.
    /// </summary>
    public class CreateOrderRequest
    {
        /// <summary>
        /// Método de pago seleccionado para la orden
        /// </summary>
        [Required(ErrorMessage = "El método de pago es obligatorio")]
        public PaymentMethod PaymentMethod { get; set; }

        /// <summary>
        /// Lista de productos incluidos en la orden
        /// </summary>
        [Required(ErrorMessage = "La orden debe incluir al menos un producto")]
        [MinLength(1, ErrorMessage = "La orden debe tener al menos un producto")]
        public List<CreateOrderProductRequest> Products { get; set; } = new List<CreateOrderProductRequest>();
    }

    /// <summary>
    /// DTO que representa un producto en la solicitud de creación de orden.
    /// Contiene la información mínima necesaria para incluir un producto en una orden.
    /// </summary>
    public class CreateOrderProductRequest
    {
        /// <summary>
        /// Nombre del producto
        /// </summary>
        [Required(ErrorMessage = "El nombre del producto es obligatorio")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "El nombre del producto debe tener entre 1 y 200 caracteres")]
        public string Name { get; set; }

        /// <summary>
        /// Precio unitario del producto
        /// </summary>
        [Required(ErrorMessage = "El precio unitario es obligatorio")]
        [Range(0.01, 1000000, ErrorMessage = "El precio unitario debe ser mayor que cero y menor que 1,000,000")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Cantidad del producto
        /// </summary>
        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(1, 100, ErrorMessage = "La cantidad debe estar entre 1 y 100")]
        public int Quantity { get; set; } = 1;
    }
}