using System.Threading.Tasks;
using PaymentOptimizer.Domain.Entities;

namespace PaymentOptimizer.Domain.Interfaces.Services
{
    /// <summary>
    /// Define el contrato para el servicio encargado de seleccionar el proveedor de pago óptimo.
    /// </summary>
    public interface IPaymentProviderSelector
    {
        // Método para seleccionar el proveedor óptimo basado en comisiones
        Task<(IPaymentProvider Provider, decimal Commission)> SelectOptimalProviderAsync(Order order);

        // Método para obtener un proveedor por su nombre
        Task<IPaymentProvider> GetProviderByNameAsync(string providerName);
    }
}