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

        private readonly ComboBox _tipo = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = Tema.Base,
            FlatStyle = FlatStyle.Flat,
            BackColor = Tema.Superficie
        };
        private readonly CampoTexto _cuenta = new CampoTexto { MaximoCaracteres = 30 };
        private readonly CampoTexto _destino = new CampoTexto { MaximoCaracteres = 30, Enabled = false };
        private readonly CampoTexto _monto = new CampoTexto { MaximoCaracteres = 24 };
        private readonly BotonRedondeado _ejecutar = Tema.BotonPrimario("Ejecutar");
        private readonly Label _estado = Disenio.Estado();

        public VistaOperacion(IMediadorEscritorio mediador) : base(mediador)
        {
            _tipo.Items.AddRange(new object[] { "Depósito", "Retiro", "Transferencia" });
            _tipo.SelectedIndex = Deposito;
            _tipo.SelectedIndexChanged += (s, e) => ActualizarDestino();

            var formulario = Disenio.Filas(18, 40, 12, 18, 44, 12, 18, 44, 12, 18, 44, 16, 42, 12, 22);
            formulario.Controls.Add(Tema.EtiquetaCampo("Operación"), 0, 0);
            formulario.Controls.Add(_tipo, 0, 1);
            formulario.Controls.Add(Tema.EtiquetaCampo("Cuenta"), 0, 3);
            formulario.Controls.Add(_cuenta, 0, 4);
            formulario.Controls.Add(Tema.EtiquetaCampo("Cuenta destino"), 0, 6);
            formulario.Controls.Add(_destino, 0, 7);
            formulario.Controls.Add(Tema.EtiquetaCampo("Monto"), 0, 9);
            formulario.Controls.Add(_monto, 0, 10);
            formulario.Controls.Add(_ejecutar, 0, 12);
            formulario.Controls.Add(_estado, 0, 14);

            var columnas = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, BackColor = Tema.Fondo };
            columnas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22f));
            columnas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 56f));
            columnas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22f));
            columnas.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            columnas.Controls.Add(Disenio.Tarjeta(formulario, 28), 1, 0);
            Controls.Add(columnas);

            _ejecutar.Click += (s, e) => Ejecutar();
            _monto.EnterPresionado += (s, e) => Ejecutar();
            ActualizarDestino();
        }

        public override void Limpiar()
        {
            _cuenta.Valor = string.Empty;
            _destino.Valor = string.Empty;
            _monto.Valor = string.Empty;
            _tipo.SelectedIndex = Deposito;
            Disenio.PintarEstado(_estado, string.Empty, false);
        }

        protected override void Ocupar(bool ocupado)
        {
            _ejecutar.Enabled = !ocupado;
        }

        private void ActualizarDestino()
        {
            bool transferencia = _tipo.SelectedIndex == Transferencia;
            _destino.Enabled = transferencia;
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
            Disenio.PintarEstado(_estado, Presentacion.Consultando, false);

            switch (_tipo.SelectedIndex)
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
            string texto = Presentacion.Mensaje(respuesta);
            if (exito)
            {
                texto += " Saldo nuevo: " + Formato.Monto(respuesta.SaldoNuevo);
            }
            Disenio.PintarEstado(_estado, texto, !exito);
        }

        private void AlRecibirTransferencia(RespuestaTransferencia respuesta)
        {
            Ocupar(false);
            if (Mediador.RevisarRespuesta(respuesta))
            {
                return;
            }
            bool exito = Presentacion.EsExitosa(respuesta);
            string texto = Presentacion.Mensaje(respuesta);
            if (exito)
            {
                texto += " Saldo origen: " + Formato.Monto(respuesta.SaldoOrigen)
                    + ". Saldo destino: " + Formato.Monto(respuesta.SaldoDestino) + ".";
            }
            Disenio.PintarEstado(_estado, texto, !exito);
        }
    }
}
