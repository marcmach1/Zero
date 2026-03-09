namespace Zero.Models
{
    public class PrevisaoPeriodoDTO
    {
        public string Titulo { get; set; } = string.Empty;
        public double Altura { get; set; }
        public double AlturaMin { get; set; }
        public int Periodo { get; set; }
        public int VentoVel { get; set; }
        
        // Esta é a STRING que recebe o texto (N, S, NE...)
        public string VentoDirecao { get; set; } = string.Empty; 
        
        // Este é o INT que recebe os graus (0 a 360) para o CSS girar a seta
        public int IconeVento { get; set; } 
    }
}