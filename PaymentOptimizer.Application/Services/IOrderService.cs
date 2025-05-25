using System.Collections.Generic;
using System.Threading.Tasks;
using PaymentOptimizer.Application.DTOs;

namespace PaymentOptimizer.Application.Services
{
    /// <summary>
    /// Define el contrato para el servicio de gestión de órdenes.
    /// Este servicio coordina las operaciones relacionadas con las órdenes de pago.
    /// </summary>
    public interface IOrderService
    {
        /// <summary>
        /// Crea una nueva orden de pago con los productos y método de pago especificados.
        /// Selecciona automáticamente el proveedor de pago más económico.
        /// </summary>
        /// <param name="request">Datos de la orden a crear</param>
        /// <returns>DTO con la información de la orden creada</returns>
        Task<OrderDto> CreateOrderAsync(CreateOrderRequest request);

        /// <summary>
        /// Obtiene información detallada de una orden específica
        /// </summary>
        /// <param name="id">Identificador de la orden</param>
        /// <returns>DTO con la información detallada de la orden</returns>
        Task<OrderDto> GetOrderAsync(int id);

        /// <summary>
        /// Obtiene un listado de todas las órdenes
        /// </summary>
        /// <returns>Lista de DTOs con información resumida de las órdenes</returns>
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync();

        /// <summary>
        /// Cancela una orden específica
        /// </summary>
        /// <param name="id">Identificador de la orden a cancelar</param>
        /// <returns>DTO con la información actualizada de la orden</returns>
        Task<OrderDto> CancelOrderAsync(int id);

        /// <summary>
        /// Marca una orden como pagada
        /// </summary>
        /// <param name="id">Identificador de la orden a marcar como pagada</param>
        /// <returns>DTO con la información actualizada de la orden</returns>
        Task<OrderDto> PayOrderAsync(int id);
    }
}