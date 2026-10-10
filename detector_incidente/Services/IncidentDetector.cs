
using System;
using System.Collections.Generic;
using System.Linq;
using detector_incidente.Models;

namespace detector_incidente.Services
{
    public class IncidentDetector
    {
        public List<AlertRecord> Detectar(
            List<MetricRecord> metricas)
        {
            List<AlertRecord> alertas =
                new List<AlertRecord>();

            if (metricas == null || metricas.Count == 0)
                return alertas;

            var grupos = metricas
                .Where(m => m != null)
                .GroupBy(m => m.Tenant);

            foreach (var grupo in grupos)
            {
                List<MetricRecord> registros = grupo
                    .OrderBy(m => m.Timestamp)
                    .ToList();

                for (int i = 10; i < registros.Count; i++)
                {
                    MetricRecord actual = registros[i];

                    // Solo se usan los 10 registros anteriores.
                    List<MetricRecord> anteriores = registros
                        .Skip(i - 10)
                        .Take(10)
                        .ToList();

                    int senales = 0;
                    List<string> motivos = new List<string>();

                    if (SubioMucho(
                        actual.Cpu,
                        Promedio(anteriores, m => m.Cpu),
                        10))
                    {
                        senales++;
                        motivos.Add("Aumento anormal de CPU");
                    }

                    if (SubioMucho(
                        actual.Ram,
                        Promedio(anteriores, m => m.Ram),
                        8))
                    {
                        senales++;
                        motivos.Add("Aumento anormal de RAM");
                    }

                    if (SubioMucho(
                        actual.Disco,
                        Promedio(anteriores, m => m.Disco),
                        8))
                    {
                        senales++;
                        motivos.Add("Aumento anormal de disco");
                    }

                    if (SubioMucho(
                        actual.LatenciaMs,
                        Promedio(anteriores, m => m.LatenciaMs),
                        60))
                    {
                        senales++;
                        motivos.Add("Aumento anormal de latencia");
                    }

                    if (SubioMucho(
                        actual.Errores,
                        Promedio(anteriores, m => m.Errores),
                        2))
                    {
                        senales++;
                        motivos.Add("Aumento anormal de errores");
                    }

                    // Una señal sola no basta para alertar.
                    if (senales >= 2)
                    {
                        bool alertaReciente = alertas.Any(a =>
                            a.Tenant == grupo.Key &&
                            (actual.Timestamp - a.Timestamp)
                                .TotalMinutes < 10);

                        if (!alertaReciente)
                        {
                            AlertRecord alerta = new AlertRecord();

                            alerta.Tenant = grupo.Key;
                            alerta.Timestamp = actual.Timestamp;

                            alerta.Cpu = actual.Cpu;
                            alerta.Ram = actual.Ram;
                            alerta.Disco = actual.Disco;
                            alerta.LatenciaMs = actual.LatenciaMs;
                            alerta.Errores = actual.Errores;


                            // Puntuación heurística, no probabilidad.
                            alerta.Confianza = Math.Min(
                                0.50 + senales * 0.10, 0.99);

                            alerta.TipoIncidente =
                                "Anomalia de infraestructura";

                            alerta.Motivo = string.Join(
                                ", ", motivos);

                            alertas.Add(alerta);
                        }
                    }
                }
            }

            return alertas;
        }

        private double? Promedio(
            List<MetricRecord> registros,
            Func<MetricRecord, double?> selector)
        {
            List<double> valores = registros
                .Select(selector)
                .Where(v => v.HasValue &&
                    !double.IsNaN(v.Value) &&
                    !double.IsInfinity(v.Value))
                .Select(v => v.Value)
                .ToList();

            if (valores.Count == 0)
                return null;

            return valores.Average();
        }

        private bool SubioMucho(
            double? actual,
            double? promedioAnterior,
            double aumentoMinimo)
        {
            if (!actual.HasValue || !promedioAnterior.HasValue)
                return false;

            return actual.Value - promedioAnterior.Value
                >= aumentoMinimo;
        }
    }
}
