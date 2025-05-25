using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Reflection;
using PaymentOptimizer.Application.DTOs;
using PaymentOptimizer.Domain.Entities;
using PaymentOptimizer.Domain.Enums;
using PaymentOptimizer.Domain.Interfaces.Repositories;
using PaymentOptimizer.Domain.Interfaces.Services;
using PaymentOptimizer.Infrastructure.Persistence;

namespace PaymentOptimizer.Application.Services
{
    /// <summary>
    /// Implementación del servicio de gestión de órdenes.
    /// Coordina las operaciones entre repositorios, selectores de proveedores y adaptadores.
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IPaymentProviderSelector _providerSelector;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository orderRepository,
            IPaymentProviderSelector providerSelector,
            ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _providerSelector = providerSelector ?? throw new ArgumentNullException(nameof(providerSelector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Crea una nueva orden de pago seleccionando el proveedor óptimo
        /// </summary>

        public async Task<OrderDto> CreateOrderAsync(CreateOrderRequest request)
        {
            _logger.LogInformation("Creando nueva orden con {ProductCount} productos y método de pago {PaymentMethod}",
                request.Products.Count, request.PaymentMethod);

            // 1. Convertir DTO a entidad de dominio
            var orderItems = request.Products
                .Select(p => new OrderItem
                {
                    Name = p.Name,
                    UnitPrice = p.UnitPrice,
                    Quantity = p.Quantity
                })
                .ToList();

            var order = new Order(request.PaymentMethod, orderItems);

            // 2. Persistir la orden inicial en el repositorio
            await _orderRepository.SaveAsync(order);
            _logger.LogInformation("DEBUG: Orden procesada para guardado con ID: {OrderId}", order.Id);
            Console.WriteLine($"DEBUG PASO 1: Orden inicial guardada con ID: {order.Id}");

            try
            {
                // 3. Seleccionar el proveedor óptimo y obtener la comisión - USANDO EL MÉTODO CORRECTO
                Console.WriteLine("DEBUG PASO 2: Iniciando selección de proveedor óptimo");
                var (provider, commission) = await _providerSelector.SelectOptimalProviderAsync(order);
                Console.WriteLine($"DEBUG PASO 3: Proveedor seleccionado: {provider.ProviderName}, comisión: {commission}");

                // 4. Añadir la comisión a la orden
                Console.WriteLine("DEBUG PASO 4: Agregando comisión a la orden");
                order.AddFee($"Comisión {provider.ProviderName}", commission);
                Console.WriteLine($"DEBUG PASO 5: Comisión agregada. Total fees: {order.Fees.Count}");

                // 5. Crear la orden en el proveedor seleccionado
                Console.WriteLine("DEBUG PASO 6: Creando orden en proveedor seleccionado");
                var providerResponse = await provider.CreateOrderAsync(order);
                Console.WriteLine($"DEBUG PASO 7: Respuesta del proveedor - Éxito: {providerResponse.Success}");

                if (providerResponse.Success)
                {
                    Console.WriteLine("DEBUG PASO 8: Asignando información del proveedor a la orden");
                    // 6. Actualizar la orden con la información del proveedor
                    order.AssignToProvider(provider.ProviderName, providerResponse.OrderNumber);
                    Console.WriteLine($"DEBUG PASO 9: Información asignada - Proveedor: {order.SelectedProvider}, OrderNumber: {order.ProviderOrderNumber}");

                    Console.WriteLine("DEBUG PASO 10: Iniciando actualización en repositorio");
                    await _orderRepository.UpdateAsync(order);
                    Console.WriteLine("DEBUG PASO 11: Actualización en repositorio completada");

                    // Logging adicional para verificar el repositorio
                    Console.WriteLine($"=== DEBUG FINAL CREATE ORDER ===");
                    Console.WriteLine($"Orden creada - ID: {order.Id}, Monto: {order.Amount}");

                    // Verificar tipo de repositorio
                    if (_orderRepository is InMemoryOrderRepository inMemoryRepo)
                    {
                        Console.WriteLine("DEBUG: Repositorio identificado como InMemoryOrderRepository");
                        inMemoryRepo.LogRepositoryState();
                    }
                    else
                    {
                        Console.WriteLine($"DEBUG: Repositorio es de tipo: {_orderRepository.GetType().Name}");
                    }

                    // Verificación inmediata de recuperación
                    Console.WriteLine("DEBUG PASO 12: Verificando recuperación inmediata de la orden");
                    try
                    {
                        var retrievedOrder = await _orderRepository.GetByIdAsync(order.Id);
                        Console.WriteLine($"DEBUG PASO 13: Verificación - Orden recuperada: {retrievedOrder != null}");
                        if (retrievedOrder != null)
                        {
                            Console.WriteLine($"DEBUG: Orden recuperada exitosamente - ID: {retrievedOrder.Id}, Monto: {retrievedOrder.Amount}");
                        }
                        else
                        {
                            Console.WriteLine("ERROR CRÍTICO: No se pudo recuperar la orden inmediatamente después de crearla");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"ERROR en verificación inmediata: {ex.Message}");
                    }

                    Console.WriteLine($"DEBUG PASO 14: Generando respuesta DTO");
                    var result = MapToOrderDto(order);
                    Console.WriteLine($"DEBUG PASO 15: DTO generado exitosamente para orden ID: {result.Id}");
                    Console.WriteLine($"=== FIN DEBUG CREATE ORDER ===");

                    return result;
                }
                else
                {
                    Console.WriteLine($"ERROR: Falla en proveedor - {providerResponse.Message}");
                    _logger.LogWarning("Error al crear orden en proveedor {ProviderName}: {ErrorMessage}",
                        provider.ProviderName, providerResponse.Message);

                    throw new ApplicationException($"Error al crear orden: {providerResponse.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPCIÓN CAPTURADA en CreateOrderAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                _logger.LogError(ex, "Error al procesar la orden {OrderId}", order.Id);
                throw;
            }
        }
        /// <summary>
        /// Obtiene información detallada de una orden específica
        /// </summary>
        public async Task<OrderDto> GetOrderAsync(int id)
        {
            _logger.LogInformation("Obteniendo orden con ID: {OrderId}", id);

            var order = await _orderRepository.GetByIdAsync(id);

            if (order == null)
            {
                _logger.LogWarning("No se encontró la orden con ID: {OrderId}", id);
                throw new KeyNotFoundException($"No se encontró la orden con ID {id}");
            }

            return MapToOrderDto(order);
        }

        /// <summary>
        /// Obtiene un listado de todas las órdenes
        /// </summary>
        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            _logger.LogInformation("Obteniendo todas las órdenes del repositorio");

            if (_orderRepository is InMemoryOrderRepository inMemoryRepo)
            {
                inMemoryRepo.LogRepositoryState();
            }

            // Obtener las órdenes del repositorio
            var orders = await _orderRepository.GetAllAsync();

            // Logging detallado de lo que recibimos
            _logger.LogInformation("Recuperadas {OrderCount} órdenes del repositorio",
                orders?.Count() ?? 0);

            if (orders?.Any() == true)
            {
                foreach (var order in orders)
                {
                    _logger.LogInformation("Orden recuperada - ID: {OrderId}, Monto: {Amount}, Estado: {Status}",
                        order.Id, order.Amount, order.Status);
                }
            }
            else
            {
                _logger.LogWarning("No se encontraron órdenes en el repositorio");
            }

            // Mapear y retornar los resultados
            var result = orders.Select(o => MapToOrderDto(o)).ToList();
            _logger.LogInformation("Retornando {OrderCount} DTOs de órdenes", result.Count);

            return result;
        }

        /// <summary>
        /// Cancela una orden específica
        /// </summary>
        public async Task<OrderDto> CancelOrderAsync(int id)
        {
            _logger.LogInformation("Cancelando orden con ID: {OrderId}", id);

            var order = await _orderRepository.GetByIdAsync(id);

            if (order == null)
            {
                _logger.LogWarning("No se encontró la orden con ID: {OrderId}", id);
                throw new KeyNotFoundException($"No se encontró la orden con ID {id}");
            }

            // Si la orden ya ha sido asignada a un proveedor, debemos cancelarla allí también
            if (!string.IsNullOrEmpty(order.ProviderOrderNumber) && !string.IsNullOrEmpty(order.SelectedProvider))
            {
                // Buscar el proveedor adecuado
                IPaymentProvider provider = await _providerSelector.GetProviderByNameAsync(order.SelectedProvider);

                if (provider != null)
                {
                    var response = await provider.CancelOrderAsync(order.ProviderOrderNumber);

                    if (!response.Success)
                    {
                        _logger.LogWarning("Error al cancelar orden en proveedor {ProviderName}: {ErrorMessage}",
                            order.SelectedProvider, response.Message);

                        throw new ApplicationException($"Error al cancelar orden en proveedor: {response.Message}");
                    }
                }
            }

            // Cancelar la orden en nuestro sistema
            order.Cancel();
            Console.WriteLine($"=== DEBUG FINAL CREATE ORDER ===");
            Console.WriteLine($"Orden creada - ID: {order.Id}, Monto: {order.Amount}");

            // Verificar si tenemos acceso al método de logging
            if (_orderRepository is InMemoryOrderRepository inMemoryRepo)
            {
                Console.WriteLine("Repositorio identificado como InMemoryOrderRepository");
                inMemoryRepo.LogRepositoryState();
            }
            else
            {
                Console.WriteLine($"Repositorio es de tipo: {_orderRepository.GetType().Name}");
            }

            // Intentar recuperar la orden inmediatamente después de crearla
            try
            {
                var retrievedOrder = await _orderRepository.GetByIdAsync(order.Id);
                Console.WriteLine($"Verificación inmediata - Orden recuperada: {retrievedOrder != null}");
                if (retrievedOrder != null)
                {
                    Console.WriteLine($"Orden recuperada exitosamente: ID {retrievedOrder.Id}, Monto {retrievedOrder.Amount}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error recuperando orden inmediatamente: {ex.Message}");
            }

            Console.WriteLine($"=== FIN DEBUG CREATE ORDER ===");

            _logger.LogError("DEBUG: Orden guardada con ID: {OrderId}", order.Id);


            return MapToOrderDto(order);
        }

        /// <summary>
        /// Marca una orden como pagada
        /// </summary>
        public async Task<OrderDto> PayOrderAsync(int id)
        {
            _logger.LogInformation("Marcando como pagada la orden con ID: {OrderId}", id);

            var order = await _orderRepository.GetByIdAsync(id);

            if (order == null)
            {
                _logger.LogWarning("No se encontró la orden con ID: {OrderId}", id);
                throw new KeyNotFoundException($"No se encontró la orden con ID {id}");
            }

            // Verificar que la orden esté en estado correcto para ser pagada
            if (order.Status != OrderStatus.Processing)
            {
                _logger.LogWarning("La orden {OrderId} no está en estado correcto para ser pagada. Estado actual: {Status}",
                    order.Id, order.Status);

                throw new InvalidOperationException($"Solo se pueden pagar órdenes en estado Processing. Estado actual: {order.Status}");
            }

            // Si la orden ha sido asignada a un proveedor, debemos marcarla como pagada allí también
            if (!string.IsNullOrEmpty(order.ProviderOrderNumber) && !string.IsNullOrEmpty(order.SelectedProvider))
            {
                // Buscar el proveedor adecuado
                var providers = await _providerSelector.GetProviderByNameAsync(order.SelectedProvider);

                if (providers != null)
                {
                    var response = await providers.PayOrderAsync(order.ProviderOrderNumber);

                    if (!response.Success)
                    {
                        _logger.LogWarning("Error al marcar como pagada la orden en proveedor {ProviderName}: {ErrorMessage}",
                            order.SelectedProvider, response.Message);

                        throw new ApplicationException($"Error al marcar como pagada la orden en proveedor: {response.Message}");
                    }
                }
            }

            // Marcar la orden como pagada en nuestro sistema
            order.MarkAsPaid();
            await _orderRepository.UpdateAsync(order);

            _logger.LogInformation("Orden {OrderId} marcada como pagada exitosamente", order.Id);

            return MapToOrderDto(order);
        }

        /// <summary>
        /// Mapea una entidad Order a un DTO OrderDto
        /// </summary>
        private OrderDto MapToOrderDto(Order order)
        {
            _logger.LogInformation("Mapeando Order a OrderDto - ID original: {OriginalId}", order.Id);
            return new OrderDto
            {
                Id = order.Id,
                Amount = order.Amount,
                Status = order.Status.ToString(),
                PaymentMethod = order.PaymentMethod.ToString(),
                ProviderName = order.SelectedProvider,
                ProviderOrderNumber = order.ProviderOrderNumber,
                Items = order.Items.Select(item => new OrderItemDto
                {
                    Name = item.Name,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity,
                    Subtotal = item.UnitPrice * item.Quantity
                }).ToList(),
                Fees = order.Fees.Select(fee => new FeeDto
                {
                    Name = fee.Name,
                    Amount = fee.Amount
                }).ToList(),
                CreatedAt = order.CreatedAt,
                PaidAt = order.PaidAt,
                CancelledAt = order.CancelledAt
            };
        }
    }
}