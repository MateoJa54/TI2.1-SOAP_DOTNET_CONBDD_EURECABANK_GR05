using System;

namespace ec.edu.monster.utilidades
{
    public static class FormatoDinero
    {
        public const int Escala = 2;

        public static readonly decimal Cero = 0.00m;

        public static decimal ADosDecimales(decimal valor)
        {
            // decimal.Round no completa la escala (1m queda como "1"); sumar 0.00m la fija en dos decimales.
            return decimal.Round(valor, Escala, MidpointRounding.AwayFromZero) + 0.00m;
        }

        public static decimal? ADosDecimales(decimal? valor)
        {
            return valor.HasValue ? ADosDecimales(valor.Value) : (decimal?)null;
        }

        public static decimal Sumar(decimal valor1, decimal valor2)
        {
            return ADosDecimales(valor1 + valor2);
        }

        public static decimal Restar(decimal valor1, decimal valor2)
        {
            return ADosDecimales(valor1 - valor2);
        }

        public static decimal Porcentaje(decimal baseImporte, decimal porcentaje)
        {
            return ADosDecimales(baseImporte * porcentaje / 100m);
        }

        public static bool EsMontoValido(decimal? valor)
        {
            if (!valor.HasValue || valor.Value <= 0m)
            {
                return false;
            }
            return decimal.Round(valor.Value, Escala, MidpointRounding.AwayFromZero) == valor.Value;
        }
    }
}
