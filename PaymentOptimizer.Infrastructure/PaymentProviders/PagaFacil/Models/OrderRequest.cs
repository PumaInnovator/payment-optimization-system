// PaymentOptimizer.Infrastructure/PaymentProviders/PagaFacil/Models/OrderRequest.cs
using System.Text.Json.Serialization;

namespace PaymentOptimizer.Infrastructure.PaymentProviders.PagaFacil.Models
{
    public class OrderRequest
    {
        [JsonPropertyName("model")]
        public OrderModel Model { get; set; }
    }

    public class OrderModel
    {
        [JsonPropertyName("paymentMethod")]
        public int PaymentMethod { get; set; }

        [JsonPropertyName("Products")]  // Nombre exacto con P mayúscula
        public ProductItem[] Products { get; set; }
    }

    public class ProductItem
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("unitPrice")]
        public decimal UnitPrice { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
    }
}