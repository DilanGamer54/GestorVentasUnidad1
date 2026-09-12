using System;
using System.Collections.Generic;

namespace MiniPOS
{
    internal class Program
    {
        
        static List<string> nombresProductos = new List<string>();
        static List<decimal> preciosProductos = new List<decimal>();
        static List<int> stockProductos = new List<int>();
        static List<int> ventasPorProducto = new List<int>();

        
        static int totalVentasRealizadas = 0;
        static decimal totalDineroIngresado = 0;

        static void Main(string[] args)
        {
            int opcion = 0;

            do
            {
                Console.Clear();
                ImprimirEncabezado("SISTEMA GESTOR DE VENTAS E INVENTARIO (MINI-POS)");
                Console.WriteLine("1. Registrar nuevo producto en inventario");
                Console.WriteLine("2. Consultar inventario completo");
                Console.WriteLine("3. Registrar una venta");
                Console.WriteLine("4. Ver reporte de caja y estadísticas diarias");
                Console.WriteLine("5. Salir");
                Console.WriteLine("====================================================");

                opcion = LeerEntero("Seleccione una opción (1-5): ", 1, 5);

                switch (opcion)
                {
                    case 1:
                        RegistrarProducto();
                        break;
                    case 2:
                        ConsultarInventario();
                        break;
                    case 3:
                        RegistrarVenta();
                        break;
                    case 4:
                        VerReportecaja();
                        break;
                    case 5:
                        ImprimirEncabezado("HASTA LUEGO");
                        Console.WriteLine("Gracias por utilizar el Sistema Mini-POS.");
                        break;
                }

                if (opcion != 5)
                {
                    Console.WriteLine("\nPresione cualquier tecla para continuar...");
                    Console.ReadKey();
                }

            } while (opcion != 5);
        }

        #region Requisito Técnico: Métodos Estáticos Obligatorios

        
        static int LeerEntero(string mensaje, int min, int max)
        {
            int valor;
            while (true)
            {
                Console.Write(mensaje);
                string entrada = Console.ReadLine();

                if (!int.TryParse(entrada, out valor))
                {
                    Console.WriteLine("[ERROR] Entrada no válida. Debe ingresar un número entero.");
                }
                else if (valor < min || valor > max)
                {
                    Console.WriteLine($"[ERROR] Opción fuera de rango. Ingrese un valor entre {min} y {max}.");
                }
                else
                {
                    return valor;
                }
            }
        }

        
        static decimal LeerDecimal(string mensaje, decimal min)
        {
            decimal valor;
            while (true)
            {
                Console.Write(mensaje);
                string entrada = Console.ReadLine();

                if (!decimal.TryParse(entrada, out valor))
                {
                    Console.WriteLine("[ERROR] Entrada no válida. Debe ingresar un valor numérico decimal.");
                }
                else if (valor <= min)
                {
                    Console.WriteLine($"[ERROR] El valor debe ser mayor a {min}.");
                }
                else
                {
                    return valor;
                }
            }
        }

        
        static decimal CalcularFactura(decimal precio, int cantidad, bool tieneDescuento, out decimal montoIva, out decimal montoDescuento)
        {
            decimal subtotal = precio * cantidad;

            if (tieneDescuento)
            {
                montoDescuento = subtotal * 0.10m;
            }
            else
            {
                montoDescuento = 0.0m;
            }

            decimal baseImponible = subtotal - montoDescuento;
            montoIva = baseImponible * 0.19m;

            decimal totalPagar = baseImponible + montoIva;
            return totalPagar;
        }

        
        static void ImprimirEncabezado(string titulo)
        {
            Console.WriteLine("====================================================");
            Console.WriteLine($"                 {titulo.ToUpper()}");
            Console.WriteLine("====================================================");
        }

        #endregion

        #region Opciones del Menú (Lógica de Negocio)

        static void RegistrarProducto()
        {
            Console.Clear();
            ImprimirEncabezado("REGISTRAR NUEVO PRODUCTO");

            string nombre = "";
            bool nombreValido = false;

            while (!nombreValido)
            {
                Console.Write("Ingrese el nombre del producto: ");
                nombre = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(nombre))
                {
                    Console.WriteLine("[ERROR] El nombre del producto no puede estar vacío.");
                    continue;
                }

                
                bool existe = false;
                foreach (string prod in nombresProductos)
                {
                    if (prod.Equals(nombre, StringComparison.OrdinalIgnoreCase))
                    {
                        existe = true;
                        break;
                    }
                }

                if (existe)
                {
                    Console.WriteLine("[ERROR] Ya existe un producto registrado con ese nombre.");
                }
                else
                {
                    nombreValido = true;
                }
            }

            decimal precio = LeerDecimal("Ingrese el precio unitario ($): ", 0);
            int stock = LeerEntero("Ingrese el stock inicial: ", 0, int.MaxValue);

            
            nombresProductos.Add(nombre);
            preciosProductos.Add(precio);
            stockProductos.Add(stock);
            ventasPorProducto.Add(0);

            Console.WriteLine("\n[OK] Producto registrado correctamente en el inventario.");
        }

        static void ConsultarInventario()
        {
            Console.Clear();
            ImprimirEncabezado("INVENTARIO COMPLETO");

            if (nombresProductos.Count == 0)
            {
                Console.WriteLine("No hay productos registrados en el inventario.");
                return;
            }

            for (int i = 0; i < nombresProductos.Count; i++)
            {
                string alertaStock = stockProductos[i] < 5 ? " [ALERTA: BAJO STOCK]" : "";
                Console.WriteLine($"{i + 1}. {nombresProductos[i],-22} | Precio: {preciosProductos[i],12:C2} | Stock: {stockProductos[i]}{alertaStock}");
            }
        }

        static void RegistrarVenta()
        {
            Console.Clear();
            ImprimirEncabezado("REGISTRAR VENTA");

            if (nombresProductos.Count == 0)
            {
                Console.WriteLine("No se pueden procesar ventas. No hay productos registrados.");
                return;
            }

            
            ConsultarInventario();
            Console.WriteLine();

            int idProducto = LeerEntero($"Seleccione el número del producto a vender (1-{nombresProductos.Count}): ", 1, nombresProductos.Count) - 1;

            int stockDisponible = stockProductos[idProducto];
            if (stockDisponible == 0)
            {
                Console.WriteLine("\n[ERROR] No hay stock disponible para este producto.");
                return;
            }

            int cantidad = 0;
            while (true)
            {
                cantidad = LeerEntero("Ingrese la cantidad a comprar: ", 1, int.MaxValue);

                if (cantidad > stockDisponible)
                {
                    Console.WriteLine($"[ERROR] Stock insuficiente. Solo quedan {stockDisponible} unidades en inventario.");
                }
                else
                {
                    break;
                }
            }

            bool aplicaDescuento = false;
            while (true)
            {
                Console.Write("¿Aplica descuento de cliente frecuente (10%)? (S/N): ");
                string respuesta = Console.ReadLine()?.Trim().ToUpper();

                if (respuesta == "S")
                {
                    aplicaDescuento = true;
                    break;
                }
                else if (respuesta == "N")
                {
                    aplicaDescuento = false;
                    break;
                }
                else
                {
                    Console.WriteLine("[ERROR] Respuesta no válida. Ingrese 'S' para Sí o 'N' para No.");
                }
            }

            
            decimal montoIva;
            decimal montoDescuento;
            decimal totalPagar = CalcularFactura(preciosProductos[idProducto], cantidad, aplicaDescuento, out montoIva, out montoDescuento);
            decimal subtotal = preciosProductos[idProducto] * cantidad;

            
            stockProductos[idProducto] -= cantidad;
            ventasPorProducto[idProducto] += cantidad;
            totalVentasRealizadas++;
            totalDineroIngresado += totalPagar;

            
            Console.WriteLine();
            ImprimirEncabezado("TICKET DE VENTA");
            Console.WriteLine($" Producto:             {nombresProductos[idProducto]} (x{cantidad})");
            Console.WriteLine($" Subtotal:             {subtotal,12:C2}");
            Console.WriteLine($" Descuento (10%):     -{montoDescuento,12:C2}");
            Console.WriteLine($" IVA (19%):            +{montoIva,12:C2}");
            Console.WriteLine(" ---------------------------------------------------");
            Console.WriteLine($" TOTAL A PAGAR:        {totalPagar,12:C2}");
            Console.WriteLine("====================================================");
            Console.WriteLine($"[OK] Venta efectuada con éxito. Stock actualizado: {stockProductos[idProducto]} unidades.");
        }

        static void VerReportecaja()
        {
            Console.Clear();
            ImprimirEncabezado("REPORTE DE CAJA Y ESTADÍSTICAS");

            Console.WriteLine($"Total de ventas realizadas:   {totalVentasRealizadas}");
            Console.WriteLine($"Total dinero en caja:         {totalDineroIngresado:C2}");

            decimal promedioVenta = totalVentasRealizadas > 0 ? totalDineroIngresado / totalVentasRealizadas : 0;
            Console.WriteLine($"Promedio por venta:           {promedioVenta:C2}");

            
            if (totalVentasRealizadas == 0)
            {
                Console.WriteLine("Producto más vendido:         Ninguno (No hay ventas registradas)");
            }
            else
            {
                int maxVendida = -1;
                string productoMasVendido = "";

                for (int i = 0; i < ventasPorProducto.Count; i++)
                {
                    if (ventasPorProducto[i] > maxVendida)
                    {
                        maxVendida = ventasPorProducto[i];
                        productoMasVendido = nombresProductos[i];
                    }
                }

                Console.WriteLine($"Producto más vendido:         {productoMasVendido} ({maxVendida} unidades)");
            }
        }

        #endregion
    }
}