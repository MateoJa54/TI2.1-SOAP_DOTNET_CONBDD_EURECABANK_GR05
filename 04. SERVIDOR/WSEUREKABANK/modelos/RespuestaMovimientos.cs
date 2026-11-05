using System.Collections.Generic;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace ec.edu.monster.modelos
{
    [XmlType("respuestaMovimientos", Namespace = "http://ws.monster.edu.ec/")]
    public class RespuestaMovimientos : RespuestaBase
    {
        [XmlElement("cuenta", Order = 4, Form = XmlSchemaForm.Unqualified)]
        public string Cuenta { get; set; }

        [XmlElement("movimientos", Order = 5, Form = XmlSchemaForm.Unqualified)]
        public List<MovimientoDTO> Movimientos { get; set; }

        public RespuestaMovimientos()
        {
        }

        public RespuestaMovimientos(string estado, string codigo, string mensaje, string cuenta, List<MovimientoDTO> movimientos) : base(estado, codigo, mensaje)
        {
            Cuenta = cuenta;
            Movimientos = movimientos;
        }
    }
}
