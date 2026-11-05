namespace ec.edu.monster.modelos
{
    public sealed class CodigoRespuesta
    {
        public static readonly CodigoRespuesta Ok200 = new CodigoRespuesta("OK", "200", "Operación realizada correctamente", true);
        public static readonly CodigoRespuesta Dat001 = new CodigoRespuesta("ERROR", "DAT-001", "Los datos de la petición están incompletos o son incorrectos", false);
        public static readonly CodigoRespuesta Auth001 = new CodigoRespuesta("ERROR", "AUTH-001", "El usuario o la contraseña son incorrectos", false);
        public static readonly CodigoRespuesta Auth002 = new CodigoRespuesta("ERROR", "AUTH-002", "El usuario se encuentra inactivo", false);
        public static readonly CodigoRespuesta Auth003 = new CodigoRespuesta("ERROR", "AUTH-003", "El token de sesión no es válido", false);
        public static readonly CodigoRespuesta Auth004 = new CodigoRespuesta("ERROR", "AUTH-004", "El token de sesión ha expirado, inicie sesión nuevamente", false);
        public static readonly CodigoRespuesta Cta001 = new CodigoRespuesta("ERROR", "CTA-001", "La cuenta solicitada no existe", false);
        public static readonly CodigoRespuesta Cta002 = new CodigoRespuesta("ERROR", "CTA-002", "La cuenta no se encuentra en estado ACTIVO", false);
        public static readonly CodigoRespuesta Cta003 = new CodigoRespuesta("ERROR", "CTA-003", "La cuenta origen y la cuenta destino no pueden ser iguales", false);
        public static readonly CodigoRespuesta Cta004 = new CodigoRespuesta("ERROR", "CTA-004", "La cuenta de destino pertenece a una moneda distinta", false);
        public static readonly CodigoRespuesta Cli001 = new CodigoRespuesta("ERROR", "CLI-001", "El cliente solicitado no existe", false);
        public static readonly CodigoRespuesta Sal001 = new CodigoRespuesta("ERROR", "SAL-001", "El saldo de la cuenta es insuficiente para la operación", false);
        public static readonly CodigoRespuesta Mon001 = new CodigoRespuesta("ERROR", "MON-001", "El monto debe ser mayor que cero y con máximo dos decimales", false);
        public static readonly CodigoRespuesta Mon002 = new CodigoRespuesta("ERROR", "MON-002", "La moneda de la cuenta no está soportada por el sistema", false);
        public static readonly CodigoRespuesta Lim001 = new CodigoRespuesta("ERROR", "LIM-001", "El monto supera el límite máximo de mantenimiento de la cuenta", false);
        public static readonly CodigoRespuesta Ser001 = new CodigoRespuesta("ERROR", "SER-001", "El servicio no está disponible en este momento", false);
        public static readonly CodigoRespuesta Err001 = new CodigoRespuesta("ERROR", "ERR-001", "Ocurrió un error interno, intente nuevamente", false);

        public static readonly CodigoRespuesta[] Todos =
        {
            Ok200, Dat001, Auth001, Auth002, Auth003, Auth004, Cta001, Cta002, Cta003, Cta004,
            Cli001, Sal001, Mon001, Mon002, Lim001, Ser001, Err001
        };

        public string Estado { get; private set; }
        public string Codigo { get; private set; }
        public string Mensaje { get; private set; }
        public bool Exitoso { get; private set; }

        private CodigoRespuesta(string estado, string codigo, string mensaje, bool exitoso)
        {
            Estado = estado;
            Codigo = codigo;
            Mensaje = mensaje;
            Exitoso = exitoso;
        }

        public override string ToString()
        {
            return Codigo;
        }
    }
}
