using System.Collections.Generic;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace ec.edu.monster.modelos
{
    [XmlType("respuestaCostos", Namespace = "http://ws.monster.edu.ec/")]
    public class RespuestaCostos : RespuestaBase
    {
        [XmlElement("costoMantenimiento", Order = 4, Form = XmlSchemaForm.Unqualified)]
        public decimal CostoMantenimiento { get; set; }

        [XmlElement("costoMovimiento", Order = 5, Form = XmlSchemaForm.Unqualified)]
        public decimal CostoMovimiento { get; set; }

        [XmlElement("interesProyectado", Order = 6, Form = XmlSchemaForm.Unqualified)]
        public decimal InteresProyectado { get; set; }

        [XmlElement("limiteMantenimiento", Order = 7, Form = XmlSchemaForm.Unqualified)]
        public decimal LimiteMantenimiento { get; set; }

        [XmlElement("operacionesSinCosto", Order = 8, Form = XmlSchemaForm.Unqualified)]
        public int OperacionesSinCosto { get; set; }

        [XmlElement("saldoActual", Order = 9, Form = XmlSchemaForm.Unqualified)]
        public decimal SaldoActual { get; set; }

        public RespuestaCostos()
        {
        }

        public RespuestaCostos(string estado, string codigo, string mensaje, decimal costoMantenimiento, decimal costoMovimiento, decimal interesProyectado, decimal limiteMantenimiento, int operacionesSinCosto, decimal saldoActual) : base(estado, codigo, mensaje)
        {
            CostoMantenimiento = costoMantenimiento;
            CostoMovimiento = costoMovimiento;
            InteresProyectado = interesProyectado;
            LimiteMantenimiento = limiteMantenimiento;
            OperacionesSinCosto = operacionesSinCosto;
            SaldoActual = saldoActual;
        }
    }
}
