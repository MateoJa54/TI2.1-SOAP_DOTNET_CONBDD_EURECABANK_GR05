using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ec.edu.monster.escritorio.modelo;
using ec.edu.monster.escritorio.util;
using ec.edu.monster.escritorio.vista.controles;

namespace ec.edu.monster.escritorio.vista
{
    // Cuentas por cédula (tabla) y consulta de una cuenta por código (detalle).
    public sealed class VistaCuentas : VistaModulo
    {
        private readonly CampoTexto _cedula = new CampoTexto { MaximoCaracteres = 20 };
        private readonly BotonRedondeado _buscar = Tema.BotonPrimario("Buscar cuentas");
        private readonly Label _titular = Disenio.Estado();
        private readonly Label _estadoLista = Disenio.Estado();
        private readonly DataGridView _tabla = Disenio.Tabla("Código", "Moneda", "Saldo", "Estado");

        private readonly CampoTexto _codigo = new CampoTexto { MaximoCaracteres = 30 };
        private readonly BotonRedondeado _consultar = Tema.BotonSecundario("Consultar cuenta");
        private readonly Label _estadoDetalle = Disenio.Estado();
        private readonly DataGridView _detalle = Disenio.Tabla("Campo", "Valor");

        public VistaCuentas(IMediadorEscritorio mediador) : base(mediador)
        {
            var busqueda = Disenio.Filas(18, 44, 10, 42, 20);
            busqueda.Controls.Add(Tema.EtiquetaCampo("Cédula del cliente"), 0, 0);
            busqueda.Controls.Add(_cedula, 0, 1);
            busqueda.Controls.Add(_buscar, 0, 3);
            busqueda.Controls.Add(_titular, 0, 4);

            var consulta = Disenio.Filas(18, 44, 10, 42, 20);
            consulta.Controls.Add(Tema.EtiquetaCampo("Código de cuenta"), 0, 0);
            consulta.Controls.Add(_codigo, 0, 1);
            consulta.Controls.Add(_consultar, 0, 3);
            consulta.Controls.Add(_estadoDetalle, 0, 4);

            var izquierda = Disenio.Filas(196, 0, 24);
            izquierda.Controls.Add(Disenio.Tarjeta(busqueda), 0, 0);
            izquierda.Controls.Add(Disenio.Tarjeta(_tabla, 1), 0, 1);
            izquierda.Controls.Add(_estadoLista, 0, 2);
            izquierda.Margin = new Padding(0, 0, 14, 0);

            var derecha = Disenio.Filas(196, 0);
            derecha.Controls.Add(Disenio.Tarjeta(consulta), 0, 0);
            derecha.Controls.Add(Disenio.Tarjeta(_detalle, 1), 0, 1);
            derecha.Margin = new Padding(14, 0, 0, 0);

            var raiz = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Tema.Fondo };
            raiz.ColumnStyles.Add(new ColumnStyle(System.Windows.Forms.SizeType.Percent, 56f));
            raiz.ColumnStyles.Add(new ColumnStyle(System.Windows.Forms.SizeType.Percent, 44f));
            raiz.RowStyles.Add(new RowStyle(System.Windows.Forms.SizeType.Percent, 100f));
            raiz.Controls.Add(izquierda, 0, 0);
            raiz.Controls.Add(derecha, 1, 0);
            Controls.Add(raiz);

            _buscar.Click += (s, e) => BuscarCuentas();
            _cedula.EnterPresionado += (s, e) => BuscarCuentas();
            _consultar.Click += (s, e) => ConsultarCuenta();
            _codigo.EnterPresionado += (s, e) => ConsultarCuenta();
        }

        public override void Limpiar()
        {
            _cedula.Valor = string.Empty;
            _codigo.Valor = string.Empty;
            _titular.Text = " ";
            Disenio.PintarEstado(_estadoLista, string.Empty, false);
            Disenio.PintarEstado(_estadoDetalle, string.Empty, false);
            Disenio.Llenar(_tabla, Enumerable.Empty<string[]>());
            Disenio.Llenar(_detalle, Enumerable.Empty<string[]>());
        }

        protected override void Ocupar(bool ocupado)
        {
            _buscar.Enabled = !ocupado;
            _consultar.Enabled = !ocupado;
        }

        private void BuscarCuentas()
        {
            string cedula = _cedula.Valor.Trim();
            Ocupar(true);
            Disenio.PintarEstado(_estadoLista, Presentacion.Consultando, false);
            TareaAsincrona.Ejecutar(
                () => Mediador.Cuentas.ListarPorCliente(cedula),
                AlRecibirCuentas,
                ex => Fallo(ex, "Cuentas.ListarPorCliente", _estadoLista));
        }

        private void AlRecibirCuentas(RespuestaCuentas respuesta)
        {
            Ocupar(false);
            if (Mediador.RevisarRespuesta(respuesta))
            {
                return;
            }
            if (!Presentacion.EsExitosa(respuesta))
            {
                _titular.Text = " ";
                Disenio.Llenar(_tabla, Enumerable.Empty<string[]>());
                Disenio.PintarEstado(_estadoLista, Presentacion.Mensaje(respuesta), true);
                return;
            }

            _titular.Text = "Titular: " + Formato.Texto(respuesta.NombreCliente);
            List<string[]> filas = (respuesta.Cuentas ?? new List<CuentaDTO>())
                .Select(Presentacion.FilaCuenta)
                .ToList();
            Disenio.Llenar(_tabla, filas);
            Disenio.PintarEstado(_estadoLista, filas.Count == 0 ? Presentacion.SinResultados : string.Empty, false);
        }

        private void ConsultarCuenta()
        {
            string codigo = _codigo.Valor.Trim();
            Ocupar(true);
            Disenio.PintarEstado(_estadoDetalle, Presentacion.Consultando, false);
            TareaAsincrona.Ejecutar(
                () => Mediador.Cuentas.Consultar(codigo),
                AlRecibirCuenta,
                ex => Fallo(ex, "Cuentas.Consultar", _estadoDetalle));
        }

        private void AlRecibirCuenta(RespuestaCuenta respuesta)
        {
            Ocupar(false);
            if (Mediador.RevisarRespuesta(respuesta))
            {
                return;
            }
            if (!Presentacion.EsExitosa(respuesta))
            {
                Disenio.Llenar(_detalle, Enumerable.Empty<string[]>());
                Disenio.PintarEstado(_estadoDetalle, Presentacion.Mensaje(respuesta), true);
                return;
            }
            if (respuesta.Cuenta == null)
            {
                Disenio.Llenar(_detalle, Enumerable.Empty<string[]>());
                Disenio.PintarEstado(_estadoDetalle, Presentacion.SinResultados, false);
                return;
            }

            Disenio.Llenar(_detalle, Presentacion.DetalleCuenta(respuesta.Cuenta)
                .Select(par => new[] { par.Key, par.Value }));
            Disenio.PintarEstado(_estadoDetalle, string.Empty, false);
        }
    }
}
