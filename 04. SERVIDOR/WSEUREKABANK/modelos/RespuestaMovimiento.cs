using System.Collections.Generic;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace ec.edu.monster.modelos
{
    [XmlType("respuestaMovimiento", Namespace = "http://ws.monster.edu.ec/")]
    public class RespuestaMovimiento : RespuestaBase
    {
        [XmlElement("costoAplicado", Order = 4, Form = XmlSchemaForm.Unqualified)]
        public decimal CostoAplicado { get; set; }

        [XmlElement("itfAplicado", Order = 5, Form = XmlSchemaForm.Unqualified)]
        public decimal ItfAplicado { get; set; }

        [XmlElement("movimiento", Order = 6, Form = XmlSchemaForm.Unqualified)]
        public MovimientoDTO Movimiento { get; set; }

        [XmlElement("saldoNuevo", Order = 7, Form = XmlSchemaForm.Unqualified)]
        public decimal SaldoNuevo { get; set; }

        public RespuestaMovimiento()
        {
        }

        public RespuestaMovimiento(string estado, string codigo, string mensaje, decimal costoAplicado, decimal itfAplicado, MovimientoDTO movimiento, decimal saldoNuevo) : base(estado, codigo, mensaje)
        {
            CostoAplicado = costoAplicado;
            ItfAplicado = itfAplicado;
            Movimiento = movimiento;
            SaldoNuevo = saldoNuevo;
        }
    }
}
