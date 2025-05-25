Sistema de Optimización de Pagos 💳

Un sistema inteligente que selecciona automáticamente el proveedor de pago más económico para cada transacción, implementando Arquitectura Hexagonal con principios de Domain-Driven Design.

🏗️ Arquitectura Técnica

Arquitectura Hexagonal (Ports & Adapters)
- Puerto Principal:"IPaymentProvider" - Define el contrato para todos los proveedores
- Adaptadores:Implementaciones específicas para cada proveedor (PagaFacil, CazaPagos, Mock)
- Núcleo de Aplicación:Lógica de negocio independiente de tecnologías externas

Domain-Driven Design
- Entidades: "Order", "OrderItem", "Fee" con comportamientos de negocio encapsulados
- Value Objects: "PaymentMethod", "OrderStatus" para representar conceptos del dominio
- Servicios de Dominio: "PaymentProviderSelector" con lógica de optimización automática
- Repositorios: Abstracción de persistencia siguiendo patrones DDD

🚀 Características Principales

✅ Sistema de Optimización Automática
- Evaluación en tiempo real de múltiples proveedores de pago
- Selección automática del proveedor más económico
- Logging detallado del proceso de optimización

✅ Integración Multi-Proveedor
- MockProvider: Para pruebas y desarrollo
- PagaFacil: Integración con API externa
- CazaPagos:Integración con API externa
- Fácil extensión para nuevos proveedores

✅ API REST Completa
- CRUD completo para órdenes
- Endpoints de diagnóstico para testing
- Documentación automática con Swagger

📊 Demostración de Funcionamiento

Ejemplo de Respuesta del Sistema
json
{
  "id": 1,
  "amount": 1255,
  "status": "Processing",
  "paymentMethod": "Cash",
  "providerName": "MockProvider",
  "providerOrderNumber": "MOCK-20250524-65866",
  "items": [
    {
      "name": "Laptop de Prueba",
      "unitPrice": 1200,
      "quantity": 1,
      "subtotal": 1200
    }
  ],
  "fees": [
    {
      "name": "Comisión MockProvider",
      "amount": 5
    }
  ]
}