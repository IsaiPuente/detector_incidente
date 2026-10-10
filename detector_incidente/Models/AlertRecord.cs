
using System;

namespace detector_incidente.Models
{
    public class AlertRecord
    {
        public string Tenant { get; set; }
        public DateTime Timestamp { get; set; }
        public double Confianza { get; set; }
        public string TipoIncidente { get; set; }
        public string Motivo { get; set; }

        // Valores de las metricas al generar la alerta.
        public double? Cpu { get; set; }
        public double? Ram { get; set; }
        public double? Disco { get; set; }
        public double? LatenciaMs { get; set; }
        public double? Errores { get; set; }
    }
}

