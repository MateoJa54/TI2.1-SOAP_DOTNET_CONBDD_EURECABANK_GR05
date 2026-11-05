namespace ec.edu.monster.datos
{
    public class DatosUsuario
    {
        public DatosUsuario(string codigo, string hash, string estado, string nombreEmpleado, string rol)
        {
            Codigo = codigo;
            Hash = hash;
            Estado = estado;
            NombreEmpleado = nombreEmpleado;
            Rol = rol;
        }

        public string Codigo { get; private set; }
        public string Hash { get; private set; }
        public string Estado { get; private set; }
        public string NombreEmpleado { get; private set; }
        public string Rol { get; private set; }
    }

    public class DatosCliente
    {
        public DatosCliente(string codigo, string nombre)
        {
            Codigo = codigo;
            Nombre = nombre;
        }

        public string Codigo { get; private set; }
        public string Nombre { get; private set; }
    }

    public class DatosTipoMovimiento
    {
        public DatosTipoMovimiento(string codigo, string descripcion, string accion)
        {
            Codigo = codigo;
            Descripcion = descripcion;
            Accion = accion;
        }

        public string Codigo { get; private set; }
        public string Descripcion { get; private set; }
        public string Accion { get; private set; }
    }

    public class DatosCostoMantenimiento
    {
        public DatosCostoMantenimiento(decimal montoMaximo, decimal importe)
        {
            MontoMaximo = montoMaximo;
            Importe = importe;
        }

        public decimal MontoMaximo { get; private set; }
        public decimal Importe { get; private set; }
    }
}
