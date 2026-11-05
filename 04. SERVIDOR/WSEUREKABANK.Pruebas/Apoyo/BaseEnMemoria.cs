using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ec.edu.monster.datos;
using ec.edu.monster.modelos;

namespace ec.edu.monster.pruebas.Apoyo
{
    // Doble de toda la capa de datos: guarda el estado en memoria, bloquea cuentas como FOR UPDATE
    // y deshace los cambios cuando la unidad de trabajo falla.
    public class BaseEnMemoria : IClienteDAO, ICuentaDAO, IMovimientoDAO, IUsuarioDAO, ILogSessionDAO,
        IParametroDAO, ITipoMovimientoDAO, IMonedaDAO, ICostoDAO, IUnidadTrabajo
    {
        private readonly object cerrojo = new object();
        private readonly Dictionary<string, object> candados = new Dictionary<string, object>();
        private readonly ThreadLocal<List<object>> retenidos = new ThreadLocal<List<object>>(() => new List<object>());

        public Dictionary<string, DatosUsuario> Usuarios = new Dictionary<string, DatosUsuario>();
        public Dictionary<string, DatosCliente> Clientes = new Dictionary<string, DatosCliente>();
        public Dictionary<string, CuentaDTO> Cuentas = new Dictionary<string, CuentaDTO>();
        public Dictionary<string, List<MovimientoDTO>> Movimientos = new Dictionary<string, List<MovimientoDTO>>();
        public Dictionary<string, decimal> Parametros = new Dictionary<string, decimal>();
        public Dictionary<string, DatosTipoMovimiento> Tipos = new Dictionary<string, DatosTipoMovimiento>();
        public HashSet<string> MonedasSoportadas = new HashSet<string>();
        public Dictionary<string, DatosCostoMantenimiento> Mantenimientos = new Dictionary<string, DatosCostoMantenimiento>();
        public Dictionary<string, decimal> CostosMovimiento = new Dictionary<string, decimal>();
        public Dictionary<string, decimal> Intereses = new Dictionary<string, decimal>();
        public List<string> Ingresos = new List<string>();
        public List<int?> Salidas = new List<int?>();

        public bool FallarAlInsertarMovimiento;
        public int InsercionesHastaFallar = int.MaxValue;
        public int EjecucionesDeUnidad;
        public int LecturasDeCuenta;
        public int Bloqueos;
        private int insertados;

        public static BaseEnMemoria ConSemilla()
        {
            BaseEnMemoria b = new BaseEnMemoria();
            b.Usuarios["cromero"] = new DatosUsuario("0001", ec.edu.monster.utilidades.HashClave.Sha256("Cromero2026"), "ACTIVO", "Carlos Romero Pachas", "Procesos");
            b.Usuarios["inactivo"] = new DatosUsuario("0008", ec.edu.monster.utilidades.HashClave.Sha256("Sarmiento2026"), "ANULADO", "Cesar Sarmiento Lopez", "Procesos");
            b.Usuarios["sinrol"] = new DatosUsuario("0009", ec.edu.monster.utilidades.HashClave.Sha256("Pachas2026"), "ACTIVO", "Luis Pachas Rios", null);
            b.Clientes["06914897"] = new DatosCliente("00001", "Ricardo Gomez Torres");
            b.Clientes["00000000"] = new DatosCliente("00099", "Cliente Sin Cuentas");
            b.Tipos["003"] = new DatosTipoMovimiento("003", "Deposito", "INGRESO");
            b.Tipos["004"] = new DatosTipoMovimiento("004", "Retiro", "SALIDA");
            b.Tipos["007"] = new DatosTipoMovimiento("007", "ITF", "SALIDA");
            b.Tipos["008"] = new DatosTipoMovimiento("008", "Transferencia", "INGRESO");
            b.Tipos["009"] = new DatosTipoMovimiento("009", "Transferencia", "SALIDA");
            b.Tipos["010"] = new DatosTipoMovimiento("010", "Cargo por Movimiento", "SALIDA");
            b.Parametros["001"] = 0.08m;
            b.Parametros["002"] = 15m;
            b.MonedasSoportadas.Add("01");
            b.MonedasSoportadas.Add("02");
            b.Mantenimientos["01"] = new DatosCostoMantenimiento(3500m, 7m);
            b.Mantenimientos["02"] = new DatosCostoMantenimiento(1000m, 2.5m);
            b.CostosMovimiento["01"] = 2m;
            b.CostosMovimiento["02"] = 0.6m;
            b.Intereses["01"] = 0.7m;
            b.Intereses["02"] = 0.5m;
            b.AgregarCuenta("00200002", "01", "Soles", 5000m, "ACTIVO", 3);
            b.AgregarCuenta("00200001", "01", "Soles", 100m, "ACTIVO", 2);
            b.AgregarCuenta("00100002", "02", "Dolares", 800m, "ACTIVO", 1);
            b.AgregarCuenta("00300001", "01", "Soles", 50m, "CANCELADO", 0);
            return b;
        }

        public CuentaDTO AgregarCuenta(string codigo, string moneda, string descripcion, decimal saldo, string estado,
            int operaciones)
        {
            CuentaDTO cuenta = new CuentaDTO(null, codigo, "00001", "001", descripcion, estado, "2024-01-15", moneda,
                "Ricardo Gomez Torres", operaciones, saldo);
            Cuentas[codigo] = cuenta;
            Movimientos[codigo] = new List<MovimientoDTO>();
            return cuenta;
        }

        public decimal Saldo(string cuenta)
        {
            return Cuentas[cuenta].Saldo;
        }

        public int Contador(string cuenta)
        {
            return Cuentas[cuenta].OperacionesRealizadas;
        }

        public List<MovimientoDTO> De(string cuenta)
        {
            return Movimientos[cuenta];
        }

        public DatosCliente BuscarPorDni(string dni)
        {
            DatosCliente cliente;
            return Clientes.TryGetValue(dni, out cliente) ? cliente : null;
        }

        public List<CuentaDTO> ListarPorCliente(string codigoCliente)
        {
            lock (cerrojo)
            {
                return Cuentas.Values.Where(c => c.CodigoCliente == codigoCliente).Select(Copiar).ToList();
            }
        }

        public CuentaDTO BuscarPorCodigo(string codigo)
        {
            lock (cerrojo)
            {
                LecturasDeCuenta++;
                CuentaDTO cuenta;
                return Cuentas.TryGetValue(codigo, out cuenta) ? Copiar(cuenta) : null;
            }
        }

        public CuentaDTO BloquearPorCodigo(ContextoTransaccional contexto, string codigo)
        {
            object candado;
            lock (cerrojo)
            {
                if (!Cuentas.ContainsKey(codigo))
                {
                    return null;
                }
                if (!candados.TryGetValue(codigo, out candado))
                {
                    candado = new object();
                    candados[codigo] = candado;
                }
                Bloqueos++;
            }
            if (!retenidos.Value.Contains(candado))
            {
                Monitor.Enter(candado);
                retenidos.Value.Add(candado);
            }
            lock (cerrojo)
            {
                return Copiar(Cuentas[codigo]);
            }
        }

        public void ActualizarSaldoYContador(ContextoTransaccional contexto, string codigo, decimal saldo, int contador)
        {
            lock (cerrojo)
            {
                CuentaDTO actual = Cuentas[codigo];
                actual.Saldo = saldo;
                actual.OperacionesRealizadas = contador;
            }
        }

        public List<MovimientoDTO> ListarPorCuenta(string codigoCuenta)
        {
            lock (cerrojo)
            {
                return Movimientos.ContainsKey(codigoCuenta) ? Movimientos[codigoCuenta].ToList() : new List<MovimientoDTO>();
            }
        }

        public void Insertar(ContextoTransaccional contexto, string codigoCuenta, MovimientoDTO movimiento)
        {
            lock (cerrojo)
            {
                insertados++;
                if (FallarAlInsertarMovimiento && insertados >= InsercionesHastaFallar)
                {
                    throw new InvalidOperationException("Fallo simulado al insertar el movimiento");
                }
                Movimientos[codigoCuenta].Add(movimiento);
            }
        }

        public DatosUsuario BuscarPorNombre(string nombreUsuario)
        {
            DatosUsuario usuario;
            return Usuarios.TryGetValue(nombreUsuario, out usuario) ? usuario : null;
        }

        public int? RegistrarIngreso(string empleado, string ip, string hostname)
        {
            lock (cerrojo)
            {
                Ingresos.Add(empleado);
                return Ingresos.Count;
            }
        }

        public void RegistrarSalida(int? idSesion)
        {
            lock (cerrojo)
            {
                Salidas.Add(idSesion);
            }
        }

        public decimal? Valor(string codigo)
        {
            decimal valor;
            return Parametros.TryGetValue(codigo, out valor) ? valor : (decimal?)null;
        }

        public DatosTipoMovimiento BuscarPorCodigo(string codigo, bool tipo)
        {
            DatosTipoMovimiento t;
            return Tipos.TryGetValue(codigo, out t) ? t : null;
        }

        DatosTipoMovimiento ITipoMovimientoDAO.BuscarPorCodigo(string codigo)
        {
            return BuscarPorCodigo(codigo, true);
        }

        public bool EstaSoportada(string codigoMoneda)
        {
            return MonedasSoportadas.Contains(codigoMoneda);
        }

        public DatosCostoMantenimiento CostoMantenimiento(string moneda)
        {
            DatosCostoMantenimiento c;
            return Mantenimientos.TryGetValue(moneda, out c) ? c : null;
        }

        public decimal? CostoMovimiento(string moneda)
        {
            decimal c;
            return CostosMovimiento.TryGetValue(moneda, out c) ? c : (decimal?)null;
        }

        public decimal? InteresMensual(string moneda)
        {
            decimal c;
            return Intereses.TryGetValue(moneda, out c) ? c : (decimal?)null;
        }

        public T Ejecutar<T>(Func<ContextoTransaccional, T> cuerpo)
        {
            Interlocked.Increment(ref EjecucionesDeUnidad);
            Dictionary<string, CuentaDTO> copiaCuentas;
            Dictionary<string, List<MovimientoDTO>> copiaMovimientos;
            lock (cerrojo)
            {
                copiaCuentas = Cuentas.ToDictionary(p => p.Key, p => Copiar(p.Value));
                copiaMovimientos = Movimientos.ToDictionary(p => p.Key, p => p.Value.ToList());
            }
            try
            {
                return cuerpo(new ContextoTransaccional(null, null));
            }
            catch
            {
                lock (cerrojo)
                {
                    Cuentas = copiaCuentas;
                    Movimientos = copiaMovimientos;
                }
                throw;
            }
            finally
            {
                foreach (object candado in retenidos.Value)
                {
                    Monitor.Exit(candado);
                }
                retenidos.Value.Clear();
            }
        }

        private static CuentaDTO Copiar(CuentaDTO c)
        {
            return new CuentaDTO(c.Clave, c.Codigo, c.CodigoCliente, c.CodigoSucursal, c.DescripcionMoneda, c.Estado,
                c.FechaCreacion, c.Moneda, c.NombreCliente, c.OperacionesRealizadas, c.Saldo);
        }
    }
}
