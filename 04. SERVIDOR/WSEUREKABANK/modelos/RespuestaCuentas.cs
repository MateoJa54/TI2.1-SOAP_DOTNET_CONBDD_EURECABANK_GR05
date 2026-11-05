using System.Collections.Generic;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace ec.edu.monster.modelos
{
    [XmlType("respuestaCuentas", Namespace = "http://ws.monster.edu.ec/")]
    public class RespuestaCuentas : RespuestaBase
    {
        [XmlElement("cedula", Order = 4, Form = XmlSchemaForm.Unqualified)]
        public string Cedula { get; set; }

        [XmlElement("cuentas", Order = 5, Form = XmlSchemaForm.Unqualified)]
        public List<CuentaDTO> Cuentas { get; set; }

        [XmlElement("nombreCliente", Order = 6, Form = XmlSchemaForm.Unqualified)]
        public string NombreCliente { get; set; }

        public RespuestaCuentas()
        {
        }

        public RespuestaCuentas(string estado, string codigo, string mensaje, string cedula, List<CuentaDTO> cuentas, string nombreCliente) : base(estado, codigo, mensaje)
        {
            Cedula = cedula;
            Cuentas = cuentas;
            NombreCliente = nombreCliente;
        }
    }
}
