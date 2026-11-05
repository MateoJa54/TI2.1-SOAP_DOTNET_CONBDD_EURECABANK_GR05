using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Web.SessionState;
using System.Web.Mvc;
using ec.edu.monster.web.controlador;
using ec.edu.monster.web.modelo;
using ec.edu.monster.web.util;

namespace ec.edu.monster.web.pruebas
{
    // Sesión HTTP simulada: guarda pares en memoria y cambia de identificador al abandonarse.
    public sealed class SesionFalsa : HttpSessionStateBase
    {
        private readonly Dictionary<string, object> datos = new Dictionary<string, object>();
        private string id = Guid.NewGuid().ToString("N");

        public int Abandonos { get; private set; }
        public int Limpiezas { get; private set; }

        public override object this[string nombre]
        {
            get { object valor; return datos.TryGetValue(nombre, out valor) ? valor : null; }
            set { datos[nombre] = value; }
        }

        public override object this[int indice]
        {
            get { return datos.Values.ElementAt(indice); }
            set { datos[datos.Keys.ElementAt(indice)] = value; }
        }

        public override int Count
        {
            get { return datos.Count; }
        }

        public override string SessionID
        {
            get { return id; }
        }

        public override bool IsNewSession
        {
            get { return false; }
        }

        public override bool IsReadOnly
        {
            get { return false; }
        }

        public override bool IsSynchronized
        {
            get { return false; }
        }

        public override object SyncRoot
        {
            get { return this; }
        }

        public override SessionStateMode Mode
        {
            get { return SessionStateMode.InProc; }
        }

        public override int Timeout
        {
            get { return 30; }
            set { }
        }

        public override void Abandon()
        {
            Abandonos++;
            datos.Clear();
            id = Guid.NewGuid().ToString("N");
        }

        public override void Clear()
        {
            Limpiezas++;
            datos.Clear();
        }

        public override void Add(string nombre, object valor)
        {
            datos[nombre] = valor;
        }

        public override void Remove(string nombre)
        {
            datos.Remove(nombre);
        }

        public override void RemoveAll()
        {
            datos.Clear();
        }

        public bool Contiene(string nombre)
        {
            return datos.ContainsKey(nombre);
        }
    }

    // Petición simulada con la ruta y el verbo indicados.
    public sealed class PeticionFalsa : HttpRequestBase
    {
        private readonly string ruta;
        private readonly string metodo;

        public PeticionFalsa(string ruta, string metodo)
        {
            this.ruta = ruta;
            this.metodo = metodo;
        }

        public override string AppRelativeCurrentExecutionFilePath
        {
            get { return ruta; }
        }

        public override string HttpMethod
        {
            get { return metodo; }
        }
    }

    // Contexto HTTP simulado: solo expone sesión y petición, que son lo que usan los controladores y el filtro.
    public sealed class ContextoFalso : HttpContextBase
    {
        private readonly SesionFalsa sesion;
        private readonly PeticionFalsa peticion;

        public ContextoFalso(SesionFalsa sesion, string ruta = "~/", string metodo = "GET")
        {
            this.sesion = sesion ?? new SesionFalsa();
            peticion = new PeticionFalsa(ruta, metodo);
        }

        public override HttpSessionStateBase Session
        {
            get { return sesion; }
        }

        public override HttpRequestBase Request
        {
            get { return peticion; }
        }
    }

    public static class PreparacionMvc
    {
        // Deja una sesión de negocio vigente en la sesión HTTP simulada, como lo hace el login.
        public static SesionUsuario IniciarSesion(SesionFalsa sesion, string token)
        {
            SesionUsuario usuario = new SesionUsuario();
            usuario.Iniciar(new RespuestaAutenticacion
            {
                Codigo = "200",
                Token = token,
                Empleado = "E-001",
                NombreEmpleado = "Ana Pérez",
                Rol = "Cajero",
                Expiracion = "2026-11-04T23:00:00"
            });
            sesion[SesionWeb.Clave] = usuario;
            return usuario;
        }

        // Asocia un controlador a un contexto y a una ruta controller/action, como lo hace MVC antes de ejecutar la acción.
        public static void Asociar(ControllerBase controlador, HttpContextBase contexto, string controller, string action)
        {
            RouteData datosRuta = new RouteData();
            datosRuta.Values["controller"] = controller;
            datosRuta.Values["action"] = action;
            controlador.ControllerContext = new ControllerContext(contexto, datosRuta, controlador);
        }

        public static void RegistrarRutas()
        {
            if (RouteTable.Routes.Count == 0)
            {
                ec.edu.monster.web.RouteConfig.RegisterRoutes(RouteTable.Routes);
            }
        }
    }
}
