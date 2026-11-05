using System;
using ec.edu.monster.consola.controlador;
using ec.edu.monster.consola.modelo;
using ec.edu.monster.consola.util;

namespace ec.edu.monster.consola.vista
{
    public class VistaAutenticacion
    {
        private readonly Consola consola;
        private readonly ControladorAutenticacion controlador;
        private readonly SesionUsuario sesion;

        public VistaAutenticacion(Consola consola, ControladorAutenticacion controlador, SesionUsuario sesion)
        {
            this.consola = consola;
            this.controlador = controlador;
            this.sesion = sesion;
        }

        // Devuelve false cuando el usuario decide salir (usuario vacío o fin de la entrada).
        public bool IniciarSesion()
        {
            while (true)
            {
                consola.Linea();
                consola.Linea("=== EURECABANK - Inicio de sesión ===");
                string usuario = consola.Leer("Usuario (vacío para salir)");
                if (string.IsNullOrWhiteSpace(usuario))
                {
                    return false;
                }
                string clave = consola.LeerSecreto("Clave");
                if (clave == null)
                {
                    return false;
                }
                RespuestaAutenticacion respuesta = controlador.Iniciar(usuario.Trim(), clave);
                consola.Linea(Formato.Resultado(respuesta.Codigo, respuesta.Mensaje));
                if (sesion.EstaVigente)
                {
                    consola.Linea("Bienvenido, " + Formato.Texto(respuesta.NombreEmpleado) + " (" + Formato.Texto(respuesta.Rol) + ")");
                    return true;
                }
            }
        }

        public void CerrarSesion()
        {
            RespuestaBase respuesta = controlador.Cerrar();
            consola.Linea(Formato.Resultado(respuesta.Codigo, respuesta.Mensaje));
        }
    }

    public class VistaCuentas
    {
        private readonly Consola consola;
        private readonly ControladorCuentas controlador;

        public VistaCuentas(Consola consola, ControladorCuentas controlador)
        {
            this.consola = consola;
            this.controlador = controlador;
        }

        public void ListarPorCliente()
        {
            string cedula = consola.Leer("Cédula del cliente");
            RespuestaCuentas r = controlador.ListarPorCliente(cedula ?? "");
            if (r.Codigo != "200")
            {
                consola.Linea(Formato.Resultado(r.Codigo, r.Mensaje));
                return;
            }
            consola.Linea(Formato.Resultado(r.Codigo, r.Mensaje));
            consola.Linea("Cliente: " + Formato.Texto(r.NombreCliente) + " (" + Formato.Texto(r.Cedula) + ")");
            if (r.Cuentas.Count == 0)
            {
                consola.Linea("El cliente no tiene cuentas.");
                return;
            }
            consola.Linea(string.Format("{0,-10}{1,-10}{2,-14}{3,-12}", "Cuenta", "Moneda", "Saldo", "Estado"));
            foreach (CuentaDTO c in r.Cuentas)
            {
                consola.Linea(string.Format("{0,-10}{1,-10}{2,-14}{3,-12}", c.Codigo, Formato.Texto(c.DescripcionMoneda),
                    Formato.Monto(c.Saldo), c.Estado));
            }
        }

        public void Consultar()
        {
            string cuenta = consola.Leer("Código de cuenta");
            RespuestaCuenta r = controlador.Consultar(cuenta ?? "");
            consola.Linea(Formato.Resultado(r.Codigo, r.Mensaje));
            if (r.Codigo != "200" || r.Cuenta == null)
            {
                return;
            }
            CuentaDTO c = r.Cuenta;
            consola.Linea("Cuenta        : " + c.Codigo);
            consola.Linea("Titular       : " + Formato.Texto(c.NombreCliente));
            consola.Linea("Moneda        : " + Formato.Texto(c.DescripcionMoneda));
            consola.Linea("Saldo         : " + Formato.Monto(c.Saldo));
            consola.Linea("Estado        : " + c.Estado);
            consola.Linea("Sucursal      : " + Formato.Texto(c.CodigoSucursal));
            consola.Linea("Creada        : " + Formato.Texto(c.FechaCreacion));
            consola.Linea("Operaciones   : " + c.OperacionesRealizadas);
        }
    }

    public class VistaMovimientos
    {
        private readonly Consola consola;
        private readonly ControladorMovimientos controlador;

        public VistaMovimientos(Consola consola, ControladorMovimientos controlador)
        {
            this.consola = consola;
            this.controlador = controlador;
        }

        public void Consultar()
        {
            string cuenta = consola.Leer("Código de cuenta");
            RespuestaMovimientos r = controlador.Consultar(cuenta ?? "");
            consola.Linea(Formato.Resultado(r.Codigo, r.Mensaje));
            if (r.Codigo != "200")
            {
                return;
            }
            if (r.Movimientos.Count == 0)
            {
                consola.Linea("La cuenta no tiene movimientos.");
                return;
            }
            consola.Linea(string.Format("{0,-6}{1,-12}{2,-24}{3,-9}{4,12}", "Nº", "Fecha", "Tipo", "Acción", "Importe"));
            foreach (MovimientoDTO m in r.Movimientos)
            {
                consola.Linea(string.Format("{0,-6}{1,-12}{2,-24}{3,-9}{4,12}", m.Numero, Formato.Texto(m.Fecha),
                    Formato.Texto(m.TipoDescripcion), Formato.Texto(m.TipoAccion), Formato.Monto(m.Importe)));
            }
        }

        public void Depositar()
        {
            string cuenta = consola.Leer("Cuenta destino del depósito");
            decimal monto;
            if (!LeerMonto(out monto))
            {
                return;
            }
            Mostrar(controlador.Depositar(cuenta ?? "", monto));
        }

        public void Retirar()
        {
            string cuenta = consola.Leer("Cuenta de la que se retira");
            decimal monto;
            if (!LeerMonto(out monto))
            {
                return;
            }
            Mostrar(controlador.Retirar(cuenta ?? "", monto));
        }

        public void Transferir()
        {
            string origen = consola.Leer("Cuenta origen");
            string destino = consola.Leer("Cuenta destino");
            decimal monto;
            if (!LeerMonto(out monto))
            {
                return;
            }
            RespuestaTransferencia r = controlador.Transferir(origen ?? "", destino ?? "", monto);
            consola.Linea(Formato.Resultado(r.Codigo, r.Mensaje));
            if (r.Codigo == "200")
            {
                consola.Linea("Movimiento origen : " + r.NumeroOrigen + "   Movimiento destino : " + r.NumeroDestino);
                consola.Linea("ITF aplicado      : " + Formato.Monto(r.ItfAplicado));
                consola.Linea("Saldo origen      : " + Formato.Monto(r.SaldoOrigen));
                consola.Linea("Saldo destino     : " + Formato.Monto(r.SaldoDestino));
            }
        }

        // Si el texto no es un número se muestra el literal de MON-001 sin llamar al servidor.
        private bool LeerMonto(out decimal monto)
        {
            string texto = consola.Leer("Monto");
            if (Formato.TryLeerMonto(texto, out monto))
            {
                return true;
            }
            consola.Linea(Formato.Resultado(Formato.CodigoMontoInvalido, Formato.MensajeMontoInvalido));
            return false;
        }

        private void Mostrar(RespuestaMovimiento r)
        {
            consola.Linea(Formato.Resultado(r.Codigo, r.Mensaje));
            if (r.Codigo == "200")
            {
                consola.Linea("Movimiento nº " + (r.Movimiento == null ? 0 : r.Movimiento.Numero));
                consola.Linea("Cargo por movimiento : " + Formato.Monto(r.CostoAplicado));
                consola.Linea("Saldo nuevo          : " + Formato.Monto(r.SaldoNuevo));
            }
        }
    }

    public class VistaCostos
    {
        private readonly Consola consola;
        private readonly ControladorCostos controlador;

        public VistaCostos(Consola consola, ControladorCostos controlador)
        {
            this.consola = consola;
            this.controlador = controlador;
        }

        public void Consultar()
        {
            string cuenta = consola.Leer("Código de cuenta");
            RespuestaCostos r = controlador.Consultar(cuenta ?? "");
            consola.Linea(Formato.Resultado(r.Codigo, r.Mensaje));
            if (r.Codigo != "200")
            {
                return;
            }
            consola.Linea("Saldo actual              : " + Formato.Monto(r.SaldoActual));
            consola.Linea("Costo de mantenimiento    : " + Formato.Monto(r.CostoMantenimiento));
            consola.Linea("Límite de mantenimiento   : " + Formato.Monto(r.LimiteMantenimiento));
            consola.Linea("Costo por movimiento      : " + Formato.Monto(r.CostoMovimiento));
            consola.Linea("Operaciones sin costo     : " + r.OperacionesSinCosto);
            consola.Linea("Interés mensual proyectado: " + Formato.Monto(r.InteresProyectado));
        }
    }

    public class VistaMenu
    {
        private readonly Consola consola;
        private readonly SesionUsuario sesion;
        private readonly VistaAutenticacion autenticacion;
        private readonly VistaCuentas cuentas;
        private readonly VistaMovimientos movimientos;
        private readonly VistaCostos costos;

        public VistaMenu(Consola consola, SesionUsuario sesion, VistaAutenticacion autenticacion, VistaCuentas cuentas,
            VistaMovimientos movimientos, VistaCostos costos)
        {
            this.consola = consola;
            this.sesion = sesion;
            this.autenticacion = autenticacion;
            this.cuentas = cuentas;
            this.movimientos = movimientos;
            this.costos = costos;
        }

        // Atiende el menú mientras haya sesión. Termina al cerrar sesión, al perderla (AUTH-003/004) o sin más entrada.
        public void Ejecutar()
        {
            while (sesion.EstaVigente)
            {
                consola.Linea();
                consola.Linea("=== EURECABANK - Menú principal ===");
                consola.Linea("1. Cuentas de un cliente");
                consola.Linea("2. Consultar una cuenta");
                consola.Linea("3. Movimientos de una cuenta");
                consola.Linea("4. Depósito");
                consola.Linea("5. Retiro");
                consola.Linea("6. Transferencia");
                consola.Linea("7. Costos de mantenimiento");
                consola.Linea("0. Cerrar sesión");
                string opcion = consola.Leer("Opción");
                if (opcion == null)
                {
                    autenticacion.CerrarSesion();
                    return;
                }
                switch (opcion.Trim())
                {
                    case "1": cuentas.ListarPorCliente(); break;
                    case "2": cuentas.Consultar(); break;
                    case "3": movimientos.Consultar(); break;
                    case "4": movimientos.Depositar(); break;
                    case "5": movimientos.Retirar(); break;
                    case "6": movimientos.Transferir(); break;
                    case "7": costos.Consultar(); break;
                    case "0": autenticacion.CerrarSesion(); return;
                    default: consola.Linea("Opción no válida."); break;
                }
            }
            consola.Linea("La sesión terminó. Inicie sesión nuevamente.");
        }
    }
}
