using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Xml;

namespace ec.edu.monster.controladores
{
    // Los clientes del contrato envían SOAPAction vacío (como JAX-WS): la operación se elige por el
    // elemento raíz del cuerpo, que en document/literal wrapped lleva el nombre de la operación.
    [AttributeUsage(AttributeTargets.Interface)]
    public sealed class DespachoPorElementoCuerpoAttribute : Attribute, IContractBehavior
    {
        public void AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint,
            BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint,
            ClientRuntime clientRuntime)
        {
        }

        public void ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint,
            DispatchRuntime dispatchRuntime)
        {
            Dictionary<string, string> operaciones = new Dictionary<string, string>();
            foreach (OperationDescription operacion in contractDescription.Operations)
            {
                operaciones[operacion.Name] = operacion.Name;
            }
            dispatchRuntime.OperationSelector = new SelectorPorElementoCuerpo(operaciones);
            dispatchRuntime.EndpointDispatcher.ContractFilter = new MatchAllMessageFilter();
        }

        public void Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
        {
        }
    }

    public sealed class SelectorPorElementoCuerpo : IDispatchOperationSelector
    {
        private readonly Dictionary<string, string> operaciones;

        public SelectorPorElementoCuerpo(Dictionary<string, string> operaciones)
        {
            this.operaciones = operaciones;
        }

        public string SelectOperation(ref Message message)
        {
            MessageBuffer buffer = message.CreateBufferedCopy(int.MaxValue);
            message = buffer.CreateMessage();
            string nombre = null;
            using (Message copia = buffer.CreateMessage())
            {
                if (!copia.IsEmpty)
                {
                    using (XmlDictionaryReader lector = copia.GetReaderAtBodyContents())
                    {
                        if (lector.NodeType == XmlNodeType.Element)
                        {
                            nombre = lector.LocalName;
                        }
                    }
                }
            }
            string operacion;
            return nombre != null && operaciones.TryGetValue(nombre, out operacion) ? operacion : "*";
        }
    }
}
