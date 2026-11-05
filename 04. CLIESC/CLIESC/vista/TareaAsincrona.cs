using System;
using System.Threading;
using System.Threading.Tasks;

namespace ec.edu.monster.escritorio.vista
{
    // Toda llamada al servidor pasa por aquí: el trabajo corre en un hilo del pool y el resultado
    // vuelve al contexto de la interfaz (WinForms) para que las pantallas solo toquen controles ahí.
    public static class TareaAsincrona
    {
        // contexto nulo: se usa el contexto de sincronización actual; si tampoco hay, se entrega en el hilo de fondo.
        public static void Ejecutar<T>(Func<T> trabajo, Action<T> alExito, Action<Exception> alError = null,
            SynchronizationContext contexto = null)
        {
            if (trabajo == null) throw new ArgumentNullException(nameof(trabajo));
            if (alExito == null) throw new ArgumentNullException(nameof(alExito));

            SynchronizationContext destino = contexto ?? SynchronizationContext.Current;
            Task.Run(() =>
            {
                T resultado;
                try
                {
                    resultado = trabajo();
                }
                catch (Exception error)
                {
                    Volver(destino, () =>
                    {
                        if (alError != null) alError(error);
                    });
                    return;
                }
                Volver(destino, () => alExito(resultado));
            });
        }

        private static void Volver(SynchronizationContext destino, Action accion)
        {
            if (destino == null)
            {
                accion();
                return;
            }
            destino.Post(_ => accion(), null);
        }
    }
}
