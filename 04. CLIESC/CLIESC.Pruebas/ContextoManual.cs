using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace ec.edu.monster.escritorio.pruebas
{
    // Contexto de sincronización de prueba: Post encola el trabajo y solo se ejecuta cuando la prueba lo bombea
    // desde su propio hilo. Así se puede comprobar en qué hilo corre cada callback, sin abrir ventanas.
    public sealed class ContextoManual : SynchronizationContext
    {
        private readonly object _cerrojo = new object();
        private readonly Queue<Action> _cola = new Queue<Action>();

        public int Publicaciones { get; private set; }

        public override void Post(SendOrPostCallback callback, object state)
        {
            lock (_cerrojo)
            {
                Publicaciones++;
                _cola.Enqueue(() => callback(state));
            }
        }

        public int Pendientes
        {
            get { lock (_cerrojo) { return _cola.Count; } }
        }

        // Ejecuta en el hilo que llama todo lo encolado hasta ahora.
        public void Bombear()
        {
            while (true)
            {
                Action accion;
                lock (_cerrojo)
                {
                    if (_cola.Count == 0)
                    {
                        return;
                    }
                    accion = _cola.Dequeue();
                }
                accion();
            }
        }

        // Bombea hasta que la condición se cumple o vence el tiempo.
        public bool Esperar(Func<bool> condicion, int milisegundos = 5000)
        {
            var cronometro = Stopwatch.StartNew();
            while (cronometro.ElapsedMilliseconds < milisegundos)
            {
                Bombear();
                if (condicion())
                {
                    return true;
                }
                Thread.Sleep(5);
            }
            Bombear();
            return condicion();
        }
    }
}
