using System;
using ec.edu.monster.controladores;
using ec.edu.monster.seguridad;
using ec.edu.monster.servicios;

namespace ec.edu.monster.pruebas.Apoyo
{
    public class RelojManual
    {
        public DateTime Ahora = new DateTime(2026, 11, 3, 10, 0, 0, DateTimeKind.Utc);

        public DateTime Leer()
        {
            return Ahora;
        }

        public void Avanzar(TimeSpan tiempo)
        {
            Ahora = Ahora.Add(tiempo);
        }
    }

    public class Entorno
    {
        public readonly BaseEnMemoria Base;
        public readonly RelojManual Reloj = new RelojManual();
        public readonly TokenServicio Tokens;
        public readonly ServicioAutenticacion Autenticacion;
        public readonly ServicioCuentas Cuentas;
        public readonly ServicioMovimientos Movimientos;
        public readonly ServicioCostos Costos;
        public readonly WSEUREKABANK Fachada;

        public Entorno() : this(BaseEnMemoria.ConSemilla())
        {
        }

        public Entorno(BaseEnMemoria baseDatos)
        {
            Base = baseDatos;
            Tokens = new TokenServicio(TimeSpan.FromMinutes(30), Reloj.Leer);
            Autenticacion = new ServicioAutenticacion(Base, Base, Tokens);
            Cuentas = new ServicioCuentas(Base, Base, Tokens);
            Movimientos = new ServicioMovimientos(Base, Base, Base, Base, Base, Base, Base, Tokens,
                () => new DateTime(2026, 11, 3));
            Costos = new ServicioCostos(Base, Base, Base, Base, Tokens);
            Fachada = new WSEUREKABANK(Autenticacion, Cuentas, Movimientos, Costos);
        }

        public string Entrar()
        {
            return Autenticacion.Autenticar("cromero", "Cromero2026").Token;
        }
    }
}
