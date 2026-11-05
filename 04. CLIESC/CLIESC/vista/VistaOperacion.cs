using System;
using System.Drawing;
using System.Windows.Forms;
using ec.edu.monster.escritorio.modelo;
using ec.edu.monster.escritorio.util;
using ec.edu.monster.escritorio.vista.controles;

namespace ec.edu.monster.escritorio.vista
{
    // Depósito, retiro y transferencia. El monto solo se valida como número; el resto de reglas lo decide el servidor.
    public sealed class VistaOperacion : VistaModulo
    {
        private const int Deposito = 0;
        private const int Retiro = 1;
        private const int Transferencia = 2;

        private static readonly string[] Ayudas =
        {
            "Suma dinero a una cuenta. Escriba la cuenta que recibe el dinero.",
            "Saca dinero de una cuenta. Escriba la cuenta de la que sale el dinero.",
            "Mueve dinero de una cuenta a otra de la misma moneda."
        };

        private int _tipo = Deposito;
        private readonly BotonPestana[] _pestanas =
        {
            new BotonPestana { Text = "Depósito", Dock = DockStyle.Fill },
            new BotonPestana { Text = "Retiro", Dock = DockStyle.Fill },
            new BotonPestana { Text = "Transferencia", Dock = DockStyle.Fill }
        };
        private readonly Label _ayuda = new Label
        {
            Dock = DockStyle.Fill,
            Font = Tema.Base,
            ForeColor = Tema.TextoSecundario,
            TextAlign = ContentAlignment.MiddleLeft
        };
        private readonly Label _etiquetaCuenta = Tema.EtiquetaCampo("Cuenta");
        private readonly CampoTexto _cuenta = new CampoTexto { MaximoCaracteres = 30 };
        private readonly Label _etiquetaDestino = Tema.EtiquetaCampo("Cuenta destino");
        private readonly CampoTexto _destino = new CampoTexto { MaximoCaracteres = 30 };
        private readonly CampoTexto _monto = new CampoTexto { MaximoCaracteres = 24 };
        private readonly BotonRedondeado _ejecutar = Tema.BotonPrimario("Ejecutar operación");
        private readonly Label _estado = Disenio.Estado();
        private readonly Label _resultadoTitulo = new Label
        {
            Dock = DockStyle.Top,
            AutoSize = false,
            Height = 26,
            Font = Tema.Fuente(Tema.TamanoBase + 1f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };
        private readonly Label _resultadoDetalle = new Label
        {
            Dock = DockStyle.Fill,
            AutoSize = false,
            Font = Tema.Base,
            TextAlign = ContentAlignment.TopLeft
        };
        private readonly Panel _resultado = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(18, 14, 18, 14),
            Visible = false
        };

        public VistaOperacion(IMediadorEscritorio mediador) : base(mediador)
        {
            for (int i = 0; i < _pestanas.Length; i++)
            {
                int indice = i;
                _pestanas[i].Click += (s, e) => Seleccionar(indice);
            }

            var pestanas = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, BackColor = Color.Transparent };
            for (int i = 0; i < 3; i++)
            {
                pestanas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
                pestanas.Controls.Add(_pestanas[i], i, 0);
            }
            pestanas.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            var columnaCuenta = Disenio.Filas(22, 44);
            columnaCuenta.Controls.Add(_etiquetaCuenta, 0, 0);
            columnaCuenta.Controls.Add(_cuenta, 0, 1);
            var columnaDestino = Disenio.Filas(22, 44);
            columnaDestino.Controls.Add(_etiquetaDestino, 0, 0);
            columnaDestino.Controls.Add(_destino, 0, 1);
            var campos = Disenio.Lado(0, columnaCuenta, columnaDestino);
            campos.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, 50f);
            campos.ColumnStyles[1] = new ColumnStyle(SizeType.Percent, 50f);
            columnaCuenta.Margin = new Padding(0, 0, 12, 0);
            columnaDestino.Margin = new Padding(12, 0, 0, 0);

            // Alturas: etiqueta, pestañas, ayuda, campos, espacio, etiqueta, monto, espacio, botón, espacio, estado, resultado
            var formulario = Disenio.Filas(22, 46, 36, 66, 16, 22, 44, 16, 44, 14, 26, 0);
            formulario.Controls.Add(Tema.EtiquetaCampo("Tipo de operación"), 0, 0);
            formulario.Controls.Add(pestanas, 0, 1);
            formulario.Controls.Add(_ayuda, 0, 2);
            formulario.Controls.Add(campos, 0, 3);
            formulario.Controls.Add(Tema.EtiquetaCampo("Monto"), 0, 5);
            formulario.Controls.Add(_monto, 0, 6);
            formulario.Controls.Add(_ejecutar, 0, 8);
            formulario.Controls.Add(_estado, 0, 10);
            _resultado.Controls.Add(_resultadoDetalle);
            _resultado.Controls.Add(_resultadoTitulo);
            formulario.Controls.Add(_resultado, 0, 11);

            var columnas = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, BackColor = Tema.Fondo };
            columnas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12f));
            columnas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 76f));
            columnas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12f));
            columnas.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            columnas.Controls.Add(Disenio.Tarjeta(formulario, 28), 1, 0);
            Controls.Add(columnas);

            _ejecutar.Click += (s, e) => Ejecutar();
            _monto.EnterPresionado += (s, e) => Ejecutar();
            Seleccionar(Deposito);
        }

        public override void Limpiar()
        {
            _cuenta.Valor = string.Empty;
            _destino.Valor = string.Empty;
            _monto.Valor = string.Empty;
            _resultado.Visible = false;
            Disenio.PintarEstado(_estado, string.Empty, false);
            Seleccionar(Deposito);
        }

        protected override void Ocupar(bool ocupado)
        {
            _ejecutar.Enabled = !ocupado;
        }

        private void Seleccionar(int tipo)
        {
            _tipo = tipo;
            for (int i = 0; i < _pestanas.Length; i++)
            {
                _pestanas[i].Activa = i == tipo;
            }
            _ayuda.Text = Ayudas[tipo];
            bool transferencia = tipo == Transferencia;
            _etiquetaCuenta.Text = transferencia ? "Cuenta origen" : "Cuenta";
            _destino.Enabled = transferencia;
            _etiquetaDestino.ForeColor = transferencia ? Tema.Texto : Tema.TextoTenue;
            if (!transferencia)
            {
                _destino.Valor = string.Empty;
            }
        }

        private void Ejecutar()
        {
            decimal monto;
            string error;
            if (!Presentacion.TryMonto(_monto.Valor, out monto, out error))
            {
                Disenio.PintarEstado(_estado, error, true);
                return;
            }

            string cuenta = _cuenta.Valor.Trim();
            string destino = _destino.Valor.Trim();
            Ocupar(true);
            _resultado.Visible = false;
            Disenio.PintarEstado(_estado, Presentacion.Consultando, false);

            switch (_tipo)
            {
                case Retiro:
                    TareaAsincrona.Ejecutar(
                        () => Mediador.Movimientos.Retirar(cuenta, monto),
                        AlRecibirMovimiento,
                        ex => Fallo(ex, "Movimientos.Retirar", _estado));
                    break;
                case Transferencia:
                    TareaAsincrona.Ejecutar(
                        () => Mediador.Movimientos.Transferir(cuenta, destino, monto),
                        AlRecibirTransferencia,
                        ex => Fallo(ex, "Movimientos.Transferir", _estado));
                    break;
                default:
                    TareaAsincrona.Ejecutar(
                        () => Mediador.Movimientos.Depositar(cuenta, monto),
                        AlRecibirMovimiento,
                        ex => Fallo(ex, "Movimientos.Depositar", _estado));
                    break;
            }
        }

        private void AlRecibirMovimiento(RespuestaMovimiento respuesta)
        {
            Ocupar(false);
            if (Mediador.RevisarRespuesta(respuesta))
            {
                return;
            }
            bool exito = Presentacion.EsExitosa(respuesta);
            Disenio.PintarEstado(_estado, string.Empty, false);
            var lineas = new System.Collections.Generic.List<string>();
            if (exito)
            {
                lineas.Add("Movimiento número: " + (respuesta.Movimiento == null ? "" : respuesta.Movimiento.Numero.ToString()));
                lineas.Add("Saldo nuevo: " + Formato.Monto(respuesta.SaldoNuevo));
                lineas.Add("Cargo por movimiento: " + Formato.Monto(respuesta.CostoAplicado));
            }
            MostrarResultado(exito, Presentacion.Mensaje(respuesta), lineas);
        }

        private void AlRecibirTransferencia(RespuestaTransferencia respuesta)
        {
            Ocupar(false);
            if (Mediador.RevisarRespuesta(respuesta))
            {
                return;
            }
            bool exito = Presentacion.EsExitosa(respuesta);
            Disenio.PintarEstado(_estado, string.Empty, false);
            var lineas = new System.Collections.Generic.List<string>();
            if (exito)
            {
                lineas.Add("Movimientos: " + respuesta.NumeroOrigen + " (origen) y " + respuesta.NumeroDestino + " (destino)");
                lineas.Add("ITF cobrado: " + Formato.Monto(respuesta.ItfAplicado));
                lineas.Add("Saldo de origen: " + Formato.Monto(respuesta.SaldoOrigen));
                lineas.Add("Saldo de destino: " + Formato.Monto(respuesta.SaldoDestino));
            }
            MostrarResultado(exito, Presentacion.Mensaje(respuesta), lineas);
        }

        // Tarjeta de resultado: verde si el servidor aprobó la operación, rojo si la rechazó.
        private void MostrarResultado(bool exito, string mensaje, System.Collections.Generic.List<string> datos)
        {
            _resultado.BackColor = exito ? Tema.ExitoSuave : Tema.ErrorSuave;
            _resultadoTitulo.ForeColor = exito ? Tema.Exito : Tema.Error;
            _resultadoTitulo.Text = exito ? "Operación registrada" : "La operación no se realizó";
            _resultadoDetalle.ForeColor = Tema.Texto;
            var lineas = new System.Collections.Generic.List<string> { mensaje };
            lineas.AddRange(datos);
            _resultadoDetalle.Text = string.Join(Environment.NewLine, lineas);
            _resultado.Visible = true;
        }
    }
}
