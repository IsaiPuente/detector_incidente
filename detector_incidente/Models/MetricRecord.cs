
using System;

namespace detector_incidente.Models
{
    public class MetricRecord
    {
        public string Tenant { get; set; }
        public DateTime Timestamp { get; set; }

        public double? Cpu { get; set; }
        public double? Ram { get; set; }
        public double? Disco { get; set; }
        public double? LatenciaMs { get; set; }
        public double? Errores { get; set; }
    }
}
