using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ec.edu.monster.escritorio.modelo;
using ec.edu.monster.escritorio.vista.controles;

namespace ec.edu.monster.escritorio.vista
{
    // Movimientos de una cuenta: número, fecha, tipo, acción e importe.
    public sealed class VistaMovimientos : VistaModulo
    {
        private readonly CampoTexto _cuenta = new CampoTexto { MaximoCaracteres = 30 };
        private readonly BotonRedondeado _consultar = Tema.BotonPrimario("Consultar movimientos");
        private readonly Label _estado = Disenio.Estado();
        private readonly DataGridView _tabla = Disenio.Tabla("Número", "Fecha", "Tipo", "Acción", "Importe");

        public VistaMovimientos(IMediadorEscritorio mediador) : base(mediador)
        {
            var busqueda = Disenio.Filas(18, 44, 0);
            busqueda.Controls.Add(Tema.EtiquetaCampo("Código de cuenta"), 0, 0);
            busqueda.Controls.Add(Disenio.Lado(230, _cuenta, _consultar), 0, 1);

            var contenido = Disenio.Filas(118, 0, 24);
            contenido.Controls.Add(Disenio.Tarjeta(busqueda), 0, 0);
            contenido.Controls.Add(Disenio.Tarjeta(_tabla, 1), 0, 1);
            contenido.Controls.Add(_estado, 0, 2);
            Controls.Add(contenido);

            _consultar.Click += (s, e) => Consultar();
            _cuenta.EnterPresionado += (s, e) => Consultar();
        }

        public override void Limpiar()
        {
            _cuenta.Valor = string.Empty;
            Disenio.PintarEstado(_estado, string.Empty, false);
            Disenio.Llenar(_tabla, Enumerable.Empty<string[]>());
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
                () => Mediador.Movimientos.Consultar(cuenta),
                AlRecibir,
                ex => Fallo(ex, "Movimientos.Consultar", _estado));
        }

        private void AlRecibir(RespuestaMovimientos respuesta)
        {
            Ocupar(false);
            if (Mediador.RevisarRespuesta(respuesta))
            {
                return;
            }
            if (!Presentacion.EsExitosa(respuesta))
            {
                Disenio.Llenar(_tabla, Enumerable.Empty<string[]>());
                Disenio.PintarEstado(_estado, Presentacion.Mensaje(respuesta), true);
                return;
            }

            List<string[]> filas = (respuesta.Movimientos ?? new List<MovimientoDTO>())
                .Select(Presentacion.FilaMovimiento)
                .ToList();
            Disenio.Llenar(_tabla, filas);
            Disenio.PintarEstado(_estado, filas.Count == 0 ? Presentacion.SinResultados : string.Empty, false);
        }
    }
}
