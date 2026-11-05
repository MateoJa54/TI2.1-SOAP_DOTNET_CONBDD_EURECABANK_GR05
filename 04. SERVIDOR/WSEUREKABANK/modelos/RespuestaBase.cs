using System.Xml.Schema;
using System.Xml.Serialization;

namespace ec.edu.monster.modelos
{
    [XmlType("respuestaBase", Namespace = "http://ws.monster.edu.ec/")]
    [XmlInclude(typeof(RespuestaAutenticacion))]
    [XmlInclude(typeof(RespuestaCuentas))]
    [XmlInclude(typeof(RespuestaCuenta))]
    [XmlInclude(typeof(RespuestaMovimientos))]
    [XmlInclude(typeof(RespuestaMovimiento))]
    [XmlInclude(typeof(RespuestaTransferencia))]
    [XmlInclude(typeof(RespuestaCostos))]
    public class RespuestaBase
    {
        [XmlElement("codigo", Order = 1, Form = XmlSchemaForm.Unqualified)]
        public string Codigo { get; set; }

        [XmlElement("estado", Order = 2, Form = XmlSchemaForm.Unqualified)]
        public string Estado { get; set; }

        [XmlElement("mensaje", Order = 3, Form = XmlSchemaForm.Unqualified)]
        public string Mensaje { get; set; }

        public RespuestaBase()
        {
        }

        public RespuestaBase(string estado, string codigo, string mensaje)
        {
            Estado = estado;
            Codigo = codigo;
            Mensaje = mensaje;
        }
    }
}
