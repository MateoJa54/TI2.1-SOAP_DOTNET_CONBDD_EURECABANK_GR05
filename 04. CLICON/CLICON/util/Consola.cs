using System;
using System.IO;
using System.Text;

namespace ec.edu.monster.consola.util
{
    // Entrada y salida de la consola detrás de un objeto para poder probar las vistas sin teclado.
    public class Consola
    {
        private readonly TextReader entrada;
        private readonly TextWriter salida;
        private readonly bool interactiva;

        public Consola() : this(Console.In, Console.Out, true)
        {
        }

        public Consola(TextReader entrada, TextWriter salida, bool interactiva = false)
        {
            this.entrada = entrada;
            this.salida = salida;
            this.interactiva = interactiva;
        }

        public void Linea(string texto = "")
        {
            salida.WriteLine(texto);
        }

        public void Escribir(string texto)
        {
            salida.Write(texto);
        }

        // Devuelve null cuando se acaba la entrada.
        public string Leer(string etiqueta)
        {
            salida.Write(etiqueta + ": ");
            salida.Flush();
            return entrada.ReadLine();
        }

        public string LeerSecreto(string etiqueta)
        {
            if (!interactiva || Console.IsInputRedirected)
            {
                return Leer(etiqueta);
            }
            salida.Write(etiqueta + ": ");
            StringBuilder texto = new StringBuilder();
            ConsoleKeyInfo tecla;
            while ((tecla = Console.ReadKey(true)).Key != ConsoleKey.Enter)
            {
                if (tecla.Key == ConsoleKey.Backspace)
                {
                    if (texto.Length > 0)
                    {
                        texto.Length--;
                    }
                }
                else if (!char.IsControl(tecla.KeyChar))
                {
                    texto.Append(tecla.KeyChar);
                }
            }
            salida.WriteLine();
            return texto.ToString();
        }
    }
}
