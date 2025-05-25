using System.Collections.Generic;
using System.Threading.Tasks;
using PaymentOptimizer.Domain.Entities;

namespace PaymentOptimizer.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Define el contrato para el repositorio de órdenes.
    /// Esta interfaz establece cómo se accederá y manipulará la persistencia de órdenes.
    /// </summary>
    public interface IOrderRepository
    {
        /// <summary>
        /// Obtiene una orden por su identificador
        /// </summary>
        /// <param name="id">Identificador de la orden</param>
        /// <returns>La orden encontrada o null si no existe</returns>
        Task<Order> GetByIdAsync(int id);

        /// <summary>
        /// Obtiene todas las órdenes existentes
        /// </summary>
        /// <returns>Colección de todas las órdenes</returns>
        Task<IEnumerable<Order>> GetAllAsync();

        /// <summary>
        /// Guarda una nueva orden en el repositorio
        /// </summary>
        /// <param name="order">La orden a guardar</param>
        Task SaveAsync(Order order);

        /// <summary>
        /// Actualiza una orden existente en el repositorio
        /// </summary>
        /// <param name="order">La orden con los datos actualizados</param>
        Task UpdateAsync(Order order);
    }
}