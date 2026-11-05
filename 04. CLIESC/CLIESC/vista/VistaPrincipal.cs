using System;
using System.Drawing;
using System.Windows.Forms;
using ec.edu.monster.escritorio.util;
using ec.edu.monster.escritorio.vista.controles;

namespace ec.edu.monster.escritorio.vista
{
    // Pantalla principal tras el acceso: barra azul marino con el empleado y "Cerrar sesión" (siempre visible),
    // pestañas Cuentas, Movimientos, Operación y Costos, y el contenido de la pestaña activa.
    public sealed class VistaPrincipal : UserControl
    {
        private readonly IMediadorEscritorio _mediador;
        private readonly Panel _contenido;
        private readonly BotonPestana[] _pestanas;
        private readonly VistaModulo[] _modulos;
        private readonly Label _empleado;
        private readonly BotonRedondeado _cerrar;

        public VistaPrincipal(IMediadorEscritorio mediador)
        {
            _mediador = mediador ?? throw new ArgumentNullException(nameof(mediador));
            Dock = DockStyle.Fill;
            BackColor = Tema.Fondo;

            _modulos = new VistaModulo[]
            {
                new VistaCuentas(mediador),
                new VistaMovimientos(mediador),
                new VistaOperacion(mediador),
                new VistaCostos(mediador)
            };
            string[] nombres = { "Cuentas", "Movimientos", "Operación", "Costos" };

            _contenido = new Panel { Dock = DockStyle.Fill, BackColor = Tema.Fondo };
            _pestanas = new BotonPestana[nombres.Length];
            var fila = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Tema.Fondo, Padding = new Padding(24, 2, 24, 0) };
            for (int i = 0; i < nombres.Length; i++)
            {
                int indice = i;
                _pestanas[i] = new BotonPestana
                {
                    Text = nombres[i],
                    Location = new Point(24 + i * 156, 2),
                    Size = new Size(150, 46)
                };
                _pestanas[i].Click += (s, e) => Activar(indice);
                fila.Controls.Add(_pestanas[i]);

                _modulos[i].Visible = false;
                _contenido.Controls.Add(_modulos[i]);
            }

            _empleado = Tema.Etiqueta(string.Empty, Tema.Base, Color.White);
            _empleado.Dock = DockStyle.Fill;
            _empleado.TextAlign = ContentAlignment.MiddleRight;
            _empleado.AutoSize = false;

            _cerrar = Tema.BotonSobreNavy("Cerrar sesión");
            _cerrar.Dock = DockStyle.Fill;
            _cerrar.Margin = new Padding(0);

            var titulo = Tema.Etiqueta("EURECABANK", Tema.Marca18, Color.White);
            titulo.Dock = DockStyle.Fill;
            titulo.AutoSize = false;
            titulo.TextAlign = ContentAlignment.MiddleLeft;

            var barra = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 68,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Tema.Navy,
                Padding = new Padding(24, 14, 24, 14)
            };
            barra.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));
            barra.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));
            barra.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170f));
            barra.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            barra.Controls.Add(titulo, 0, 0);
            barra.Controls.Add(_empleado, 1, 0);
            barra.Controls.Add(_cerrar, 2, 0);

            // El orden de Controls.Add define el orden de acoplamiento: el último agregado queda arriba.
            Controls.Add(_contenido);
            Controls.Add(fila);
            Controls.Add(barra);

            _cerrar.Click += (s, e) => CerrarSesion();
            Activar(0);
        }

        // Se llama al entrar a la pantalla principal: muestra el empleado y empieza en la primera pestaña.
        public void AlMostrar()
        {
            _empleado.Text = "Empleado: " + Formato.Texto(_mediador.Sesion.NombreEmpleado);
            Activar(0);
        }

        // Vacía los datos de todas las pestañas al salir de la sesión.
        public void Limpiar()
        {
            foreach (VistaModulo modulo in _modulos)
            {
                modulo.Limpiar();
            }
        }

        private void Activar(int indice)
        {
            for (int i = 0; i < _modulos.Length; i++)
            {
                _pestanas[i].Activa = i == indice;
                _modulos[i].Visible = i == indice;
            }
        }

        private void CerrarSesion()
        {
            _cerrar.Enabled = false;
            TareaAsincrona.Ejecutar(
                () => _mediador.Autenticacion.Cerrar(),
                respuesta => Terminar(),
                ex => Terminar());
        }

        private void Terminar()
        {
            _cerrar.Enabled = true;
            Limpiar();
            _mediador.SesionCerrada();
        }
    }
}
