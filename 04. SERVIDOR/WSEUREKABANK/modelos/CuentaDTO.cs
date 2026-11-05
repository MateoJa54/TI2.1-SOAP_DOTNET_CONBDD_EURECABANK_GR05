using System.Collections.Generic;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace ec.edu.monster.modelos
{
    [XmlType("cuentaDTO", Namespace = "http://ws.monster.edu.ec/")]
    public class CuentaDTO
    {
        [XmlElement("clave", Order = 1, Form = XmlSchemaForm.Unqualified)]
        public string Clave { get; set; }

        [XmlElement("codigo", Order = 2, Form = XmlSchemaForm.Unqualified)]
        public string Codigo { get; set; }

        [XmlElement("codigoCliente", Order = 3, Form = XmlSchemaForm.Unqualified)]
        public string CodigoCliente { get; set; }

        [XmlElement("codigoSucursal", Order = 4, Form = XmlSchemaForm.Unqualified)]
        public string CodigoSucursal { get; set; }

        [XmlElement("descripcionMoneda", Order = 5, Form = XmlSchemaForm.Unqualified)]
        public string DescripcionMoneda { get; set; }

        [XmlElement("estado", Order = 6, Form = XmlSchemaForm.Unqualified)]
        public string Estado { get; set; }

        [XmlElement("fechaCreacion", Order = 7, Form = XmlSchemaForm.Unqualified)]
        public string FechaCreacion { get; set; }

        [XmlElement("moneda", Order = 8, Form = XmlSchemaForm.Unqualified)]
        public string Moneda { get; set; }

        [XmlElement("nombreCliente", Order = 9, Form = XmlSchemaForm.Unqualified)]
        public string NombreCliente { get; set; }

        [XmlElement("operacionesRealizadas", Order = 10, Form = XmlSchemaForm.Unqualified)]
        public int OperacionesRealizadas { get; set; }

        [XmlElement("saldo", Order = 11, Form = XmlSchemaForm.Unqualified)]
        public decimal Saldo { get; set; }

        public CuentaDTO()
        {
        }

        public CuentaDTO(string clave, string codigo, string codigoCliente, string codigoSucursal, string descripcionMoneda, string estado, string fechaCreacion, string moneda, string nombreCliente, int operacionesRealizadas, decimal saldo)
        {
            Clave = clave;
            Codigo = codigo;
            CodigoCliente = codigoCliente;
            CodigoSucursal = codigoSucursal;
            DescripcionMoneda = descripcionMoneda;
            Estado = estado;
            FechaCreacion = fechaCreacion;
            Moneda = moneda;
            NombreCliente = nombreCliente;
            OperacionesRealizadas = operacionesRealizadas;
            Saldo = saldo;
        }
    }
}
