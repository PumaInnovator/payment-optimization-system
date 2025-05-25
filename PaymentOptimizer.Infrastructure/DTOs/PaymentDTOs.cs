using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PaymentOptimizer.Infrastructure.DTOs
{
    /// <summary>
    /// DTO (Data Transfer Object) para solicitudes de orden a APIs externas.
    /// 
    /// Los DTOs son como "formularios estandarizados" que usamos para comunicarnos
    /// con servicios externos. Cada API externa espera los datos en un formato específico,
    /// y este DTO nos asegura que enviemos la información exactamente como la esperan.
    /// 
    /// Piensa en esto como la diferencia entre hablar en tu idioma nativo versus
    /// usar un formulario oficial para comunicarte con una institución gubernamental.
    /// El formulario tiene campos específicos que debes completar de cierta manera.
    /// </summary>
    public class OrderRequestDto
    {
        /// <summary>
        /// Método de pago como entero: 0 = Cash, 1 = CreditCard
        /// CRÍTICO: Los proveedores reales esperan "method", no "paymentMethod"
        /// </summary>
        [JsonPropertyName("method")]
        public int Method { get; set; }

        /// <summary>
        /// Array de productos - debe coincidir exactamente con la estructura esperada
        /// </summary>
        [JsonPropertyName("products")]
        public List<ProductItemDto> Products { get; set; } = new List<ProductItemDto>();
    }

    public class ProductItemDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("unitPrice")]
        public decimal UnitPrice { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
    }
}