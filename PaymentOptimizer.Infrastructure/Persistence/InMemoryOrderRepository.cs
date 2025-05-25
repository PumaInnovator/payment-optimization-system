using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PaymentOptimizer.Domain.Entities;
using PaymentOptimizer.Domain.Interfaces.Repositories;

namespace PaymentOptimizer.Infrastructure.Persistence
{
    public class InMemoryOrderRepository : IOrderRepository
    {
        // IMPORTANTE: Hacer static el diccionario para asegurar que todas las instancias 
        // compartan el mismo almacenamiento, independientemente del ciclo de vida
        private static readonly ConcurrentDictionary<int, Order> _orders = new();
        private static int _nextId = 1;
        private static readonly object _lockObject = new object();

        // Método público para acceso de diagnóstico
        public void LogRepositoryState()
        {
            Console.WriteLine("=== ESTADO DEL REPOSITORIO ===");
            Console.WriteLine($"Próximo ID: {_nextId}");
            Console.WriteLine($"Total órdenes en memoria: {_orders.Count}");
            Console.WriteLine($"IDs disponibles: [{string.Join(", ", _orders.Keys)}]");

            foreach (var kvp in _orders)
            {
                Console.WriteLine($"  - ID: {kvp.Key}, Monto: {kvp.Value.Amount}, Estado: {kvp.Value.Status}");
            }

            Console.WriteLine("=== FIN ESTADO ===");
        }

        public async Task SaveAsync(Order order)
        {
            lock (_lockObject)
            {
                try
                {
                    if (order.Id == 0)
                    {
                        // Establecer el Id en la orden original usando el método público
                        order.SetId(_nextId);
                        _orders[_nextId] = order;
                        _nextId++;

                        Console.WriteLine($"DEBUG: Orden guardada con nuevo ID: {order.Id}");
                    }
                    else
                    {
                        _orders[order.Id] = order;
                        Console.WriteLine($"DEBUG: Orden actualizada con ID existente: {order.Id}");
                    }

                    Console.WriteLine($"DEBUG: Total órdenes en repositorio: {_orders.Count}");
                    LogRepositoryState();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR en SaveAsync: {ex.Message}");
                    throw;
                }
            }
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            Console.WriteLine($"DEBUG: GetByIdAsync buscando orden ID: {id}");
            _orders.TryGetValue(id, out var order);
            Console.WriteLine($"DEBUG: GetByIdAsync - Orden encontrada: {order != null}");
            return order;
        }

        public async Task UpdateAsync(Order order)
        {
            lock (_lockObject)
            {
                try
                {
                    Console.WriteLine($"DEBUG: UpdateAsync - Actualizando orden ID: {order.Id}");

                    if (_orders.ContainsKey(order.Id))
                    {
                        _orders[order.Id] = order;
                        Console.WriteLine($"DEBUG: Orden actualizada exitosamente con ID: {order.Id}");
                    }
                    else
                    {
                        // Intento de recuperación: guardar como nueva orden si no existe
                        Console.WriteLine($"ALERTA: Orden con ID {order.Id} no encontrada para actualizar. Se guardará como nueva.");
                        _orders[order.Id] = order;
                    }

                    LogRepositoryState();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR en UpdateAsync: {ex.Message}");
                    throw;
                }
            }
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            // MÉTODO CRÍTICO QUE ESTABA FALLANDO
            Console.WriteLine("DEBUG: Entrando en GetAllAsync");

            try
            {
                // Registrar el estado del repositorio para diagnóstico
                LogRepositoryState();

                // Explícitamente crear una copia de la colección de valores
                var allOrders = _orders.Values.ToList();

                Console.WriteLine($"DEBUG: GetAllAsync encontró {allOrders.Count} órdenes");

                if (allOrders.Count == 0)
                {
                    Console.WriteLine("ALERTA: No se encontraron órdenes en el repositorio");
                }
                else
                {
                    foreach (var order in allOrders)
                    {
                        Console.WriteLine($"DEBUG: Orden encontrada - ID: {order.Id}, Monto: {order.Amount}");
                    }
                }

                return allOrders;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR en GetAllAsync: {ex.Message}");
                return new List<Order>();
            }
        }

        public async Task DeleteAsync(int id)
        {
            Console.WriteLine($"DEBUG: DeleteAsync - Eliminando orden ID: {id}");
            _orders.TryRemove(id, out _);
            LogRepositoryState();
        }
        
    }
}