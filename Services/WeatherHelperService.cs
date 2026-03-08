namespace Zero.Services // Ajuste o namespace para o do seu projeto
{
    public static class WeatherHelper
    {
        public static string TraduzirWeatherCode(int code)
        {
            return code switch
            {
                0 => "Céu limpo, o sol tá trincando!",
                1 or 2 or 3 => "Nublado, mas o visual tá massa.",
                45 or 48 => "Neblina total, cuidado pra não perder o rumo no mar.",
                51 or 53 or 55 => "Garoa chata, mas não molha o que já tá na água.",
                61 or 63 or 65 => "Chuva caindo, lava a alma!",
                71 or 73 or 75 => "Neve (se isso acontecer em Navega, corre que é o fim do mundo!)",
                95 or 96 or 99 => "Trovoada! Sai da água que o bicho vai pegar.",
                _ => "O tempo tá meio estranho, fica de olho no céu!"
            };
        }

        public static string ConverterDirecaoVento(double graus)
        {
            if (graus >= 337.5 || graus < 22.5) return "Norte (N)";
            if (graus >= 22.5 && graus < 67.5) return "Nordeste (NE)";
            if (graus >= 67.5 && graus < 112.5) return "Leste (E)";
            if (graus >= 112.5 && graus < 157.5) return "Sudeste (SE)";
            if (graus >= 157.5 && graus < 202.5) return "Sul (S)";
            if (graus >= 202.5 && graus < 247.5) return "Sudoeste (SW)";
            if (graus >= 247.5 && graus < 292.5) return "Oeste (W)";
            if (graus >= 292.5 && graus < 337.5) return "Noroeste (NW)";
            return "Indefinido";
        }
    }
}