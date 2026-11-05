using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ec.edu.monster.escritorio.modelo;
using ec.edu.monster.escritorio.vista.controles;

namespace ec.edu.monster.escritorio.vista
{
    // Costos de mantenimiento de una cuenta, mostrados como seis indicadores.
    public sealed class VistaCostos : VistaModulo
    {
        private const int Indicadores = 6;

        private readonly CampoTexto _cuenta = new CampoTexto { MaximoCaracteres = 30 };
        private readonly BotonRedondeado _consultar = Tema.BotonPrimario("Consultar costos");
        private readonly Label _estado = Disenio.Estado();
        private readonly Label[] _nombres = new Label[Indicadores];
        private readonly Label[] _valores = new Label[Indicadores];

        public VistaCostos(IMediadorEscritorio mediador) : base(mediador)
        {
            var busqueda = Disenio.Filas(18, 44, 0);
            busqueda.Controls.Add(Tema.EtiquetaCampo("Código de cuenta"), 0, 0);
            busqueda.Controls.Add(Disenio.Lado(230, _cuenta, _consultar), 0, 1);

            var rejilla = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 2,
                BackColor = Tema.Fondo,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            for (int c = 0; c < 3; c++)
            {
                rejilla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 3));
            }
            for (int f = 0; f < 2; f++)
            {
                rejilla.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            }

            List<KeyValuePair<string, string>> plantilla = Presentacion.Costos(new RespuestaCostos());
            for (int i = 0; i < Indicadores; i++)
            {
                _nombres[i] = Tema.Etiqueta(plantilla[i].Key, Tema.Pequena, Tema.TextoSecundario);
                _valores[i] = Tema.Etiqueta("-", Tema.Valor, Tema.Navy);
                var tarjeta = Disenio.Tarjeta(Disenio.Filas(22, 0, 40), 18);
                var contenido = (TableLayoutPanel)tarjeta.Controls[0];
                contenido.Controls.Add(_nombres[i], 0, 0);
                contenido.Controls.Add(_valores[i], 0, 2);
                tarjeta.Margin = new Padding(0, 0, 12, 12);
                rejilla.Controls.Add(tarjeta, i % 3, i / 3);
            }

            var contenedor = Disenio.Filas(118, 24, 0);
            contenedor.Controls.Add(Disenio.Tarjeta(busqueda), 0, 0);
            contenedor.Controls.Add(_estado, 0, 1);
            contenedor.Controls.Add(rejilla, 0, 2);
            Controls.Add(contenedor);

            _consultar.Click += (s, e) => Consultar();
            _cuenta.EnterPresionado += (s, e) => Consultar();
        }

        public override void Limpiar()
        {
            _cuenta.Valor = string.Empty;
            Disenio.PintarEstado(_estado, string.Empty, false);
            for (int i = 0; i < Indicadores; i++)
            {
                _valores[i].Text = "-";
            }
        }

        protected override void Ocupar(bool ocupado)
        {
            _consultar.Enabled = !ocupado;
        }

        private void Consultar()
        {
            string cuenta = _cuenta.Valor.Trim();
            Ocupar(true);
            Disenio.PintarEstado(_estado, Presentacion.Consultando, false);
            TareaAsincrona.Ejecutar(
                () => Mediador.Costos.Consultar(cuenta),
                AlRecibir,
                ex => Fallo(ex, "Costos.Consultar", _estado));
        }

        private void AlRecibir(RespuestaCostos respuesta)
        {
            Ocupar(false);
            if (Mediador.RevisarRespuesta(respuesta))
            {
                return;
            }
            if (!Presentacion.EsExitosa(respuesta))
            {
                Disenio.PintarEstado(_estado, Presentacion.Mensaje(respuesta), true);
                return;
            }

            List<KeyValuePair<string, string>> pares = Presentacion.Costos(respuesta);
            for (int i = 0; i < Indicadores; i++)
            {
                _valores[i].Text = pares[i].Value;
            }
            Disenio.PintarEstado(_estado, string.Empty, false);
        }
    }
}
