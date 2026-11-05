using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace ec.edu.monster.utilidades
{
    // El contenedor <return> del contrato lleva tipos distintos según la operación. XmlSerializer no admite
    // dos elementos "return" de tipos diferentes en el mismo contrato, así que el sobre se arma aparte:
    // este tipo es el cuerpo de <return> y sus hijos son los campos de la respuesta.
    public class Retorno
    {
        private static readonly XmlSerializerNamespaces SinPrefijos = CrearSinPrefijos();

        [XmlAnyElement]
        public XmlElement[] Campos { get; set; }

        public Retorno()
        {
        }

        public Retorno(XmlElement[] campos)
        {
            Campos = campos;
        }

        public static Retorno De<T>(T valor)
        {
            XmlDocument documento = new XmlDocument();
            using (StringWriter escritor = new StringWriter())
            {
                Cache<T>.Serializador.Serialize(escritor, valor, SinPrefijos);
                documento.LoadXml(escritor.ToString());
            }
            XmlElement[] campos = new XmlElement[documento.DocumentElement.ChildNodes.Count];
            for (int i = 0; i < campos.Length; i++)
            {
                campos[i] = (XmlElement)documento.DocumentElement.ChildNodes[i];
            }
            return new Retorno(campos);
        }

        // new XmlSerializer(tipo, raiz) no se cachea solo: se crea una vez por tipo.
        private static class Cache<T>
        {
            public static readonly XmlSerializer Serializador =
                new XmlSerializer(typeof(T), new XmlRootAttribute("return") { Namespace = "" });
        }

        private static XmlSerializerNamespaces CrearSinPrefijos()
        {
            XmlSerializerNamespaces espacios = new XmlSerializerNamespaces();
            espacios.Add("", "");
            return espacios;
        }
    }
}
