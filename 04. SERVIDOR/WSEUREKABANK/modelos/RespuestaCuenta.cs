using System.Collections.Generic;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace ec.edu.monster.modelos
{
    [XmlType("respuestaCuenta", Namespace = "http://ws.monster.edu.ec/")]
    public class RespuestaCuenta : RespuestaBase
    {
        [XmlElement("cuenta", Order = 4, Form = XmlSchemaForm.Unqualified)]
        public CuentaDTO Cuenta { get; set; }

        public RespuestaCuenta()
        {
        }

        public RespuestaCuenta(string estado, string codigo, string mensaje, CuentaDTO cuenta) : base(estado, codigo, mensaje)
        {
            Cuenta = cuenta;
        }
    }
}
