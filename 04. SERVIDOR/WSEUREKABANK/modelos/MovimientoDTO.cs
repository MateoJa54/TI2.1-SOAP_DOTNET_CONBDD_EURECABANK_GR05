using System.Collections.Generic;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace ec.edu.monster.modelos
{
    [XmlType("movimientoDTO", Namespace = "http://ws.monster.edu.ec/")]
    public class MovimientoDTO
    {
        [XmlElement("empleado", Order = 1, Form = XmlSchemaForm.Unqualified)]
        public string Empleado { get; set; }

        [XmlElement("fecha", Order = 2, Form = XmlSchemaForm.Unqualified)]
        public string Fecha { get; set; }

        [XmlElement("importe", Order = 3, Form = XmlSchemaForm.Unqualified)]
        public decimal Importe { get; set; }

        [XmlElement("numero", Order = 4, Form = XmlSchemaForm.Unqualified)]
        public int Numero { get; set; }

        [XmlElement("referencia", Order = 5, Form = XmlSchemaForm.Unqualified)]
        public string Referencia { get; set; }

        [XmlElement("tipoAccion", Order = 6, Form = XmlSchemaForm.Unqualified)]
        public string TipoAccion { get; set; }

        [XmlElement("tipoCodigo", Order = 7, Form = XmlSchemaForm.Unqualified)]
        public string TipoCodigo { get; set; }

        [XmlElement("tipoDescripcion", Order = 8, Form = XmlSchemaForm.Unqualified)]
        public string TipoDescripcion { get; set; }

        public MovimientoDTO()
        {
        }

        public MovimientoDTO(string empleado, string fecha, decimal importe, int numero, string referencia, string tipoAccion, string tipoCodigo, string tipoDescripcion)
        {
            Empleado = empleado;
            Fecha = fecha;
            Importe = importe;
            Numero = numero;
            Referencia = referencia;
            TipoAccion = tipoAccion;
            TipoCodigo = tipoCodigo;
            TipoDescripcion = tipoDescripcion;
        }
    }
}
