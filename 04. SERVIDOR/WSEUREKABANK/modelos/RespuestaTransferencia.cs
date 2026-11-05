using System.Collections.Generic;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace ec.edu.monster.modelos
{
    [XmlType("respuestaTransferencia", Namespace = "http://ws.monster.edu.ec/")]
    public class RespuestaTransferencia : RespuestaBase
    {
        [XmlElement("itfAplicado", Order = 4, Form = XmlSchemaForm.Unqualified)]
        public decimal ItfAplicado { get; set; }

        [XmlElement("numeroDestino", Order = 5, Form = XmlSchemaForm.Unqualified)]
        public int NumeroDestino { get; set; }

        [XmlElement("numeroOrigen", Order = 6, Form = XmlSchemaForm.Unqualified)]
        public int NumeroOrigen { get; set; }

        [XmlElement("saldoDestino", Order = 7, Form = XmlSchemaForm.Unqualified)]
        public decimal SaldoDestino { get; set; }

        [XmlElement("saldoOrigen", Order = 8, Form = XmlSchemaForm.Unqualified)]
        public decimal SaldoOrigen { get; set; }

        public RespuestaTransferencia()
        {
        }

        public RespuestaTransferencia(string estado, string codigo, string mensaje, decimal itfAplicado, int numeroDestino, int numeroOrigen, decimal saldoDestino, decimal saldoOrigen) : base(estado, codigo, mensaje)
        {
            ItfAplicado = itfAplicado;
            NumeroDestino = numeroDestino;
            NumeroOrigen = numeroOrigen;
            SaldoDestino = saldoDestino;
            SaldoOrigen = saldoOrigen;
        }
    }
}
