using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PaymentOptimizer.Application.DTOs;
using PaymentOptimizer.Application.Services;
using PaymentOptimizer.Domain.Interfaces.Repositories;
using PaymentOptimizer.Infrastructure.Persistence;

namespace PaymentOptimizer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;
        private readonly IOrderRepository _orderRepository;

        public OrdersController(
            IOrderService orderService,
            ILogger<OrdersController> logger,
              IOrderRepository orderRepository)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _orderRepository = orderRepository; // Inicializar el campo

            Console.WriteLine($"DEBUG: OrdersController creado con repositorio HashCode: {_orderRepository.GetHashCode()}");
        }

        /// <summary>
        /// Obtiene todas las órdenes existentes
        /// </summary>
        /// <returns>Lista de órdenes</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders()
        {
            _logger.LogInformation("Obteniendo todas las órdenes");
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        /// <summary>
        /// Obtiene una orden específica por su ID
        /// </summary>
        /// <param name="id">ID de la orden</param>
        /// <returns>Detalles de la orden</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderDto>> GetOrder(int id)
        {
            Console.WriteLine($"DEBUG: GetOrder usando repositorio HashCode: {_orderRepository.GetHashCode()}");
            _logger.LogInformation("Obteniendo orden con ID: {OrderId}", id);

            try
            {
                var order = await _orderService.GetOrderAsync(id);
                return Ok(order);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Orden no encontrada: {OrderId}", id);
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Crea una nueva orden
        /// </summary>
        /// <param name="request">Datos de la orden a crear</param>
        /// <returns>Datos de la orden creada</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        public async Task<ActionResult<OrderDto>> CreateOrder(CreateOrderRequest request)
        {
            Console.WriteLine($"DEBUG: CreateOrder usando repositorio HashCode: {_orderRepository.GetHashCode()}");
            // Registra los datos de entrada para depuración
            _logger.LogInformation("Datos recibidos: {RequestData}",
                System.Text.Json.JsonSerializer.Serialize(request));

            try
            {
                // Verificación manual explícita
                if (request.Products == null)
                {
                    return BadRequest(new { message = "El array de productos es nulo" });
                }

                if (request.Products.Count == 0)
                {
                    return BadRequest(new { message = "La orden debe tener al menos un producto" });
                }

                var order = await _orderService.CreateOrderAsync(request);
                _logger.LogInformation("Orden creada exitosamente con ID: {OrderId}. Generando URL con este ID.", order.Id);
                return CreatedAtAction(
            nameof(GetOrder),
            new { id = order.Id }, // Asegurarse que este ID es correcto
            order
        );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completo: {Error}", ex.ToString());
                return StatusCode(500, new
                {
                    message = "Error al procesar la orden",
                    error = ex.Message,
                    innerException = ex.InnerException?.Message,
                    stack = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Marca una orden como pagada
        /// </summary>
        /// <param name="id">ID de la orden a marcar como pagada</param>
        /// <returns>Datos de la orden pagada</returns>
        [HttpPut("{id}/pay")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OrderDto>> PayOrder(int id)
        {
            _logger.LogInformation("Marcando como pagada la orden con ID: {OrderId}", id);

            try
            {
                var order = await _orderService.PayOrderAsync(id);
                return Ok(order);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Orden no encontrada: {OrderId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operación inválida para orden {OrderId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al marcar como pagada la orden {OrderId}", id);
                return StatusCode(500, new { message = "Error interno al pagar la orden" });
            }
        }

        // Añadir a OrdersController.cs
        [HttpGet("diagnostico")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<string> GetDiagnostico()
        {
            _logger.LogInformation("Ejecutando diagnóstico del repositorio");

            try
            {
                if (_orderRepository is InMemoryOrderRepository inMemoryRepo)
                {
                    inMemoryRepo.LogRepositoryState();
                    return Ok("Diagnóstico ejecutado. Revisa los logs para ver el estado del repositorio.");
                }

                return Ok($"El repositorio no es de tipo InMemoryOrderRepository, es de tipo: {_orderRepository.GetType().Name}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error en diagnóstico: {ex.Message}");
            }
        }
    }
}