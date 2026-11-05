using System.Collections.Generic;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace ec.edu.monster.modelos
{
    [XmlType("respuestaAutenticacion", Namespace = "http://ws.monster.edu.ec/")]
    public class RespuestaAutenticacion : RespuestaBase
    {
        [XmlElement("empleado", Order = 4, Form = XmlSchemaForm.Unqualified)]
        public string Empleado { get; set; }

        [XmlElement("expiracion", Order = 5, Form = XmlSchemaForm.Unqualified)]
        public string Expiracion { get; set; }

        [XmlElement("nombreEmpleado", Order = 6, Form = XmlSchemaForm.Unqualified)]
        public string NombreEmpleado { get; set; }

        [XmlElement("rol", Order = 7, Form = XmlSchemaForm.Unqualified)]
        public string Rol { get; set; }

        [XmlElement("token", Order = 8, Form = XmlSchemaForm.Unqualified)]
        public string Token { get; set; }

        public RespuestaAutenticacion()
        {
        }

        public RespuestaAutenticacion(string estado, string codigo, string mensaje, string empleado, string expiracion, string nombreEmpleado, string rol, string token) : base(estado, codigo, mensaje)
        {
            Empleado = empleado;
            Expiracion = expiracion;
            NombreEmpleado = nombreEmpleado;
            Rol = rol;
            Token = token;
        }
    }
}
