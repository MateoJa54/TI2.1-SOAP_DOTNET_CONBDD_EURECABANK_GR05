using System;

namespace ec.edu.monster.datos
{
    public interface IUnidadTrabajo
    {
        T Ejecutar<T>(Func<ContextoTransaccional, T> cuerpo);
    }
}
