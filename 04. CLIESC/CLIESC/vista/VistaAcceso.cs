using System;
using System.Drawing;
using System.Windows.Forms;
using ec.edu.monster.escritorio.modelo;
using ec.edu.monster.escritorio.vista.controles;

namespace ec.edu.monster.escritorio.vista
{
    // Pantalla de acceso: marca arriba, tarjeta con usuario y clave (etiqueta encima de cada campo),
    // botón Ingresar, mensaje de error en línea y Monster.png debajo del formulario.
    public sealed class VistaAcceso : UserControl
    {
        private const int AnchoColumna = 420;
        private const int AltoColumna = 580;

        private readonly IMediadorEscritorio _mediador;
        private readonly Panel _columna;
        private readonly CampoTexto _usuario;
        private readonly CampoTexto _clave;
        private readonly BotonRedondeado _ingresar;
        private readonly Label _estado;

        public VistaAcceso(IMediadorEscritorio mediador)
        {
            _mediador = mediador ?? throw new ArgumentNullException(nameof(mediador));
            Dock = DockStyle.Fill;
            BackColor = Tema.Fondo;

            _usuario = new CampoTexto { MaximoCaracteres = 50 };
            _clave = new CampoTexto { EsPassword = true, MaximoCaracteres = 100 };
            _ingresar = Tema.BotonPrimario("Ingresar");
            _estado = Disenio.Estado();

            var marca = new Label
            {
                Text = "EURECABANK",
                Font = Tema.Titulo,
                ForeColor = Tema.Navy,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            var subtitulo = new Label
            {
                Text = "Acceso de empleados",
                Font = Tema.Base,
                ForeColor = Tema.TextoSecundario,
                TextAlign = ContentAlignment.TopCenter,
                Dock = DockStyle.Fill
            };
            var imagen = new PictureBox
            {
                Image = Recursos.CargarMarca(),
                SizeMode = PictureBoxSizeMode.Zoom,
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            var formulario = Disenio.Filas(18, 44, 14, 18, 44, 20, 42, 12, 22);
            formulario.Controls.Add(Tema.EtiquetaCampo("Usuario"), 0, 0);
            formulario.Controls.Add(_usuario, 0, 1);
            formulario.Controls.Add(Tema.EtiquetaCampo("Clave"), 0, 3);
            formulario.Controls.Add(_clave, 0, 4);
            formulario.Controls.Add(_ingresar, 0, 6);
            formulario.Controls.Add(_estado, 0, 8);

            _columna = new Panel { Size = new Size(AnchoColumna, AltoColumna), BackColor = Tema.Fondo };
            var disposicion = Disenio.Filas(44, 24, 16, 300, 18, 0);
            disposicion.Controls.Add(marca, 0, 0);
            disposicion.Controls.Add(subtitulo, 0, 1);
            disposicion.Controls.Add(Disenio.Tarjeta(formulario, 28), 0, 3);
            disposicion.Controls.Add(imagen, 0, 5);
            _columna.Controls.Add(disposicion);
            Controls.Add(_columna);

            Resize += (s, e) => AjustarColumna();
            _ingresar.Click += (s, e) => Autenticar();
            _clave.EnterPresionado += (s, e) => Autenticar();
            _usuario.EnterPresionado += (s, e) => _clave.Enfocar();
            AjustarColumna();
        }

        // Se llama cada vez que vuelve la pantalla de acceso; muestra el motivo si lo hay.
        public void AlMostrar(string mensaje, bool esError)
        {
            _usuario.Valor = string.Empty;
            _clave.Valor = string.Empty;
            Disenio.PintarEstado(_estado, mensaje, esError);
            _usuario.Enfocar();
        }

        private void Autenticar()
        {
            string usuario = _usuario.Valor.Trim();
            string clave = _clave.Valor;
            if (usuario.Length == 0 || clave.Length == 0)
            {
                Disenio.PintarEstado(_estado, "Ingrese el usuario y la clave.", true);
                return;
            }

            Ocupar(true);
            Disenio.PintarEstado(_estado, Presentacion.Consultando, false);
            TareaAsincrona.Ejecutar(
                () => _mediador.Autenticacion.Iniciar(usuario, clave),
                AlAutenticar,
                ex =>
                {
                    Ocupar(false);
                    Disenio.PintarEstado(_estado, Presentacion.FalloInesperado(ex, "Autenticacion.Iniciar"), true);
                });
        }

        private void AlAutenticar(RespuestaAutenticacion respuesta)
        {
            Ocupar(false);
            if (Presentacion.EsExitosa(respuesta) && _mediador.SesionIniciada())
            {
                return;
            }
            Disenio.PintarEstado(_estado, Presentacion.Mensaje(respuesta), true);
            _clave.Valor = string.Empty;
            _clave.Enfocar();
        }

        private void Ocupar(bool ocupado)
        {
            _ingresar.Enabled = !ocupado;
            _usuario.SoloLectura = ocupado;
            _clave.SoloLectura = ocupado;
        }

        private void AjustarColumna()
        {
            int x = Math.Max(12, (ClientSize.Width - AnchoColumna) / 2);
            int y = Math.Max(12, (ClientSize.Height - AltoColumna) / 2);
            _columna.Location = new Point(x, y);
        }
    }
}
