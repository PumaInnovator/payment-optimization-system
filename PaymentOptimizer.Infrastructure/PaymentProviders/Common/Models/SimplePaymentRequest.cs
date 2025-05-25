// PaymentOptimizer.Infrastructure/PaymentProviders/Common/Models/SimplePaymentRequest.cs
using System.Text.Json.Serialization;

namespace PaymentOptimizer.Infrastructure.PaymentProviders.Common.Models
{
    /// <summary>
    /// Estructura simplificada requerida por CazaPagos (y posiblemente PagaFacil)
    /// Basada en los resultados de las pruebas exhaustivas realizadas
    /// </summary>
    public class SimplePaymentRequest
    {
        /// <summary>
        /// Método de pago como número: 0 = Cash, 1 = CreditCard
        /// Campo requerido por la API (no "paymentMethod")
        /// </summary>
        [JsonPropertyName("method")]
        public int Method { get; set; }

        /// <summary>
        /// Lista de productos (con 'p' minúscula como lo requiere la API)
        /// </summary>
        [JsonPropertyName("products")]
        public List<SimpleProduct> Products { get; set; }
    }

    /// <summary>
    /// Representa un producto en la estructura simplificada
    /// </summary>
    public class SimpleProduct
    {
        /// <summary>
        /// Nombre del producto
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Precio unitario del producto
        /// </summary>
        [JsonPropertyName("unitPrice")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Cantidad del producto (opcional según la respuesta de la API)
        /// </summary>
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
    }
}