using System.Collections.Generic;
using System.IO;
using ec.edu.monster.consola.controlador;
using ec.edu.monster.consola.modelo;
using ec.edu.monster.consola.util;
using ec.edu.monster.consola.ws;

namespace ec.edu.monster.consola.pruebas
{
    // Una llamada recibida por el cliente falso, con los datos que llegaron.
    public sealed class Llamada
    {
        public string Metodo { get; set; }
        public string Usuario { get; set; }
        public string ClaveHash { get; set; }
        public string Token { get; set; }
        public string Cedula { get; set; }
        public string Cuenta { get; set; }
        public string CuentaDestino { get; set; }
        public decimal? Monto { get; set; }
    }

    // Implementación de IClienteEurekabank que no usa red: registra cada llamada y devuelve respuestas configurables.
    public class ClienteFalso : IClienteEurekabank
    {
        public ClienteFalso()
        {
            RespuestaAutenticar = new RespuestaAutenticacion
            {
                Estado = "OK",
                Codigo = "200",
                Mensaje = "Autenticación correcta",
                Token = "tok-1",
                Empleado = "E-001",
                NombreEmpleado = "Ana Pérez",
                Rol = "Cajero",
                Expiracion = "2026-11-04T23:00:00"
            };
            RespuestaCerrar = new RespuestaBase("OK", "200", "Sesión cerrada correctamente");
            RespuestaListar = new RespuestaCuentas { Estado = "OK", Codigo = "200", Mensaje = "Cuentas listadas", Cedula = "1712345678", NombreCliente = "Juan Ruiz" };
            RespuestaConsultarCuenta = new RespuestaCuenta
            {
                Estado = "OK",
                Codigo = "200",
                Mensaje = "Cuenta consultada",
                Cuenta = new CuentaDTO { Codigo = "CTA-1", DescripcionMoneda = "Dólar", Saldo = 100m, Estado = "ACTIVA", NombreCliente = "Juan Ruiz", CodigoSucursal = "S01", FechaCreacion = "2024-01-02", OperacionesRealizadas = 3 }
            };
            RespuestaMovimientosConsulta = new RespuestaMovimientos { Estado = "OK", Codigo = "200", Mensaje = "Movimientos consultados", Cuenta = "CTA-1" };
            RespuestaDeposito = new RespuestaMovimiento { Estado = "OK", Codigo = "200", Mensaje = "Depósito realizado", Movimiento = new MovimientoDTO { Numero = 7 }, SaldoNuevo = 125.5m, CostoAplicado = 0.5m };
            RespuestaRetiro = new RespuestaMovimiento { Estado = "OK", Codigo = "200", Mensaje = "Retiro realizado", Movimiento = new MovimientoDTO { Numero = 8 }, SaldoNuevo = 90m, CostoAplicado = 0.5m };
            RespuestaTransferencia = new RespuestaTransferencia { Estado = "OK", Codigo = "200", Mensaje = "Transferencia realizada", NumeroOrigen = 9, NumeroDestino = 10, SaldoOrigen = 50m, SaldoDestino = 60m, ItfAplicado = 0.1m };
            RespuestaCostos = new RespuestaCostos { Estado = "OK", Codigo = "200", Mensaje = "Costos consultados", SaldoActual = 100m, CostoMantenimiento = 2m, CostoMovimiento = 0.25m, InteresProyectado = 0.4m, LimiteMantenimiento = 500m, OperacionesSinCosto = 5 };
        }

        public List<Llamada> Llamadas { get; } = new List<Llamada>();

        // Respuestas de autenticación que se devuelven en orden antes de usar RespuestaAutenticar.
        public Queue<RespuestaAutenticacion> ColaAutenticar { get; } = new Queue<RespuestaAutenticacion>();

        public RespuestaAutenticacion RespuestaAutenticar { get; set; }
        public RespuestaBase RespuestaCerrar { get; set; }
        public RespuestaCuentas RespuestaListar { get; set; }
        public RespuestaCuenta RespuestaConsultarCuenta { get; set; }
        public RespuestaMovimientos RespuestaMovimientosConsulta { get; set; }
        public RespuestaMovimiento RespuestaDeposito { get; set; }
        public RespuestaMovimiento RespuestaRetiro { get; set; }
        public RespuestaTransferencia RespuestaTransferencia { get; set; }
        public RespuestaCostos RespuestaCostos { get; set; }

        public int Cuantas(string metodo)
        {
            int n = 0;
            foreach (Llamada l in Llamadas)
            {
                if (l.Metodo == metodo) n++;
            }
            return n;
        }

        public Llamada Ultima
        {
            get { return Llamadas[Llamadas.Count - 1]; }
        }

        // Pone el mismo código y mensaje en todas las respuestas configuradas.
        public void ResponderTodoCon(string codigo, string mensaje)
        {
            RespuestaAutenticar.Codigo = codigo;
            RespuestaAutenticar.Mensaje = mensaje;
            RespuestaCerrar.Codigo = codigo;
            RespuestaCerrar.Mensaje = mensaje;
            RespuestaListar.Codigo = codigo;
            RespuestaListar.Mensaje = mensaje;
            RespuestaConsultarCuenta.Codigo = codigo;
            RespuestaConsultarCuenta.Mensaje = mensaje;
            RespuestaMovimientosConsulta.Codigo = codigo;
            RespuestaMovimientosConsulta.Mensaje = mensaje;
            RespuestaDeposito.Codigo = codigo;
            RespuestaDeposito.Mensaje = mensaje;
            RespuestaRetiro.Codigo = codigo;
            RespuestaRetiro.Mensaje = mensaje;
            RespuestaTransferencia.Codigo = codigo;
            RespuestaTransferencia.Mensaje = mensaje;
            RespuestaCostos.Codigo = codigo;
            RespuestaCostos.Mensaje = mensaje;
        }

        public RespuestaAutenticacion Autenticar(string usuario, string claveHash)
        {
            Llamadas.Add(new Llamada { Metodo = "Autenticar", Usuario = usuario, ClaveHash = claveHash });
            if (ColaAutenticar.Count > 0)
            {
                return ColaAutenticar.Dequeue();
            }
            return RespuestaAutenticar;
        }

        public RespuestaBase CerrarSesion(string token)
        {
            Llamadas.Add(new Llamada { Metodo = "CerrarSesion", Token = token });
            return RespuestaCerrar;
        }

        public RespuestaCuentas ListarCuentasPorCliente(string cedula, string token)
        {
            Llamadas.Add(new Llamada { Metodo = "ListarCuentasPorCliente", Cedula = cedula, Token = token });
            return RespuestaListar;
        }

        public RespuestaCuenta ConsultarCuenta(string cuenta, string token)
        {
            Llamadas.Add(new Llamada { Metodo = "ConsultarCuenta", Cuenta = cuenta, Token = token });
            return RespuestaConsultarCuenta;
        }

        public RespuestaMovimientos ConsultarMovimientos(string cuenta, string token)
        {
            Llamadas.Add(new Llamada { Metodo = "ConsultarMovimientos", Cuenta = cuenta, Token = token });
            return RespuestaMovimientosConsulta;
        }

        public RespuestaMovimiento RealizarDeposito(string cuenta, decimal monto, string token)
        {
            Llamadas.Add(new Llamada { Metodo = "RealizarDeposito", Cuenta = cuenta, Monto = monto, Token = token });
            return RespuestaDeposito;
        }

        public RespuestaMovimiento RealizarRetiro(string cuenta, decimal monto, string token)
        {
            Llamadas.Add(new Llamada { Metodo = "RealizarRetiro", Cuenta = cuenta, Monto = monto, Token = token });
            return RespuestaRetiro;
        }

        public RespuestaTransferencia RealizarTransferencia(string cuentaOrigen, string cuentaDestino, decimal monto, string token)
        {
            Llamadas.Add(new Llamada { Metodo = "RealizarTransferencia", Cuenta = cuentaOrigen, CuentaDestino = cuentaDestino, Monto = monto, Token = token });
            return RespuestaTransferencia;
        }

        public RespuestaCostos ConsultarCostosMantenimiento(string cuenta, string token)
        {
            Llamadas.Add(new Llamada { Metodo = "ConsultarCostosMantenimiento", Cuenta = cuenta, Token = token });
            return RespuestaCostos;
        }
    }

    // Arma los controladores y vistas sobre un mismo cliente falso y una misma sesión.
    public sealed class Ambiente
    {
        public Ambiente()
        {
            Cliente = new ClienteFalso();
            Sesion = new SesionUsuario();
            Salida = new StringWriter();
            Autenticacion = new ControladorAutenticacion(Cliente, Sesion);
            Cuentas = new ControladorCuentas(Cliente, Sesion);
            Movimientos = new ControladorMovimientos(Cliente, Sesion);
            Costos = new ControladorCostos(Cliente, Sesion);
        }

        public ClienteFalso Cliente { get; private set; }
        public SesionUsuario Sesion { get; private set; }
        public StringWriter Salida { get; private set; }
        public ControladorAutenticacion Autenticacion { get; private set; }
        public ControladorCuentas Cuentas { get; private set; }
        public ControladorMovimientos Movimientos { get; private set; }
        public ControladorCostos Costos { get; private set; }

        // Deja una sesión vigente sin pasar por el login.
        public void IniciarSesionDirecta(string token)
        {
            Sesion.Iniciar(new RespuestaAutenticacion { Codigo = "200", Token = token, Empleado = "E-001", NombreEmpleado = "Ana Pérez", Rol = "Cajero" });
        }

        // Ejecuta el programa completo con la entrada dada y devuelve lo que se escribió en consola.
        public string Programa(string entrada)
        {
            Consola consola = new Consola(new StringReader(entrada), Salida);
            Program.Ejecutar(consola, Cliente);
            return Salida.ToString();
        }

        public static string Lineas(params string[] lineas)
        {
            return string.Join("\n", lineas) + "\n";
        }

        // Nombres de operación protegida usados por las pruebas de controladores.
        public const string OpCuentasListar = "Cuentas.Listar";
        public const string OpCuentasConsultar = "Cuentas.Consultar";
        public const string OpMovimientosConsultar = "Movimientos.Consultar";
        public const string OpMovimientosDepositar = "Movimientos.Depositar";
        public const string OpMovimientosRetirar = "Movimientos.Retirar";
        public const string OpMovimientosTransferir = "Movimientos.Transferir";
        public const string OpCostosConsultar = "Costos.Consultar";

        public RespuestaBase Invocar(string operacion)
        {
            switch (operacion)
            {
                case OpCuentasListar: return Cuentas.ListarPorCliente("1712345678");
                case OpCuentasConsultar: return Cuentas.Consultar("CTA-1");
                case OpMovimientosConsultar: return Movimientos.Consultar("CTA-1");
                case OpMovimientosDepositar: return Movimientos.Depositar("CTA-1", 25.5m);
                case OpMovimientosRetirar: return Movimientos.Retirar("CTA-1", 25.5m);
                case OpMovimientosTransferir: return Movimientos.Transferir("CTA-1", "CTA-2", 25.5m);
                case OpCostosConsultar: return Costos.Consultar("CTA-1");
                default: throw new System.ArgumentException("Operación desconocida: " + operacion);
            }
        }

        // Nombre del método del cliente que cada operación debe invocar.
        public static string MetodoDe(string operacion)
        {
            switch (operacion)
            {
                case OpCuentasListar: return "ListarCuentasPorCliente";
                case OpCuentasConsultar: return "ConsultarCuenta";
                case OpMovimientosConsultar: return "ConsultarMovimientos";
                case OpMovimientosDepositar: return "RealizarDeposito";
                case OpMovimientosRetirar: return "RealizarRetiro";
                case OpMovimientosTransferir: return "RealizarTransferencia";
                case OpCostosConsultar: return "ConsultarCostosMantenimiento";
                default: throw new System.ArgumentException("Operación desconocida: " + operacion);
            }
        }
    }
}
