
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using detector_incidente.Models;
using detector_incidente.Services;

namespace detector_incidente
{
    public class PruebaDetector
    {
        public static void Ejecutar()
        {
            try
            {
                // Ruta del archivo de métricas de Kaiku.
                string ruta = @"C:\Zikit\zikitmetricas.csv.csv";

                if (!File.Exists(ruta))
                {
                    Console.WriteLine(
                        "No se encontró el archivo: " + ruta);
                    return;
                }

                CsvPruebaReader lector =
                    new CsvPruebaReader();

                List<MetricRecord> datos = lector.Leer(ruta);

                Console.WriteLine(
                    "Registros leídos: " + datos.Count);

                Console.WriteLine(
                    "CPU mínima: " +
                    datos.Where(m => m.Cpu.HasValue)
                         .Min(m => m.Cpu.Value));

                Console.WriteLine(
                    "CPU máxima: " +
                    datos.Where(m => m.Cpu.HasValue)
                         .Max(m => m.Cpu.Value));

                Console.WriteLine(
                    "RAM máxima: " +
                    datos.Where(m => m.Ram.HasValue)
                         .Max(m => m.Ram.Value));

                Console.WriteLine(
                    "Latencia máxima: " +
                    datos.Where(m => m.LatenciaMs.HasValue)
                         .Max(m => m.LatenciaMs.Value));

                Console.WriteLine(
                    "Errores máximos: " +
                    datos.Where(m => m.Errores.HasValue)
                         .Max(m => m.Errores.Value));


                if (datos.Count == 0)
                {
                    Console.WriteLine(
                        "No se encontraron registros válidos.");
                    return;
                }

                IncidentDetector detector =
                    new IncidentDetector();

                List<AlertRecord> alertas =
                    detector.Detectar(datos);

                string rutaLogs = @"C:\Zikit\zikitlogs.md";

                if (File.Exists(rutaLogs))
                {
                    LogPruebaReader lectorLogs =
                        new LogPruebaReader();

                    List<LogPruebaReader.EventoLog> eventos =
                        lectorLogs.Leer(rutaLogs);

                    Console.WriteLine(
                        "Eventos de log leidos: " + eventos.Count);

                    foreach (AlertRecord alerta in alertas)
                    {
                        Console.WriteLine("");
                        Console.WriteLine("================================");
                        Console.WriteLine(
                            "ANALISIS DE ALERTA: " +
                            alerta.Timestamp.ToString("yyyy-MM-dd HH:mm"));

                        // Ventana para investigar el contexto:
                        // 30 minutos antes y 30 minutos despues.
                        DateTime desde = alerta.Timestamp.AddMinutes(-30);
                        DateTime hasta = alerta.Timestamp.AddMinutes(30);

                        List<LogPruebaReader.EventoLog> cercanos =
                            eventos.Where(e =>
                                e.Timestamp >= desde &&
                                e.Timestamp <= hasta)
                            .OrderBy(e => e.Timestamp)
                            .ToList();

                        if (cercanos.Count == 0)
                        {
                            Console.WriteLine(
                                "No se encontraron eventos en ese intervalo.");
                        }
                        else
                        {
                            foreach (LogPruebaReader.EventoLog evento in cercanos)
                            {
                                string marca =
                                    evento.Timestamp <= alerta.Timestamp
                                    ? "ANTES/EN ALERTA"
                                    : "DESPUES DE ALERTA";

                                Console.WriteLine(
                                    evento.Timestamp.ToString("yyyy-MM-dd HH:mm:ss") +
                                    " | " + evento.Nivel +
                                    " | " + marca +
                                    " | " + evento.Mensaje);
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine(
                        "No se encontro el archivo de logs: " + rutaLogs);
                }


                Console.WriteLine(
                    "Cantidad de alertas: " + alertas.Count);


                foreach (AlertRecord alerta in alertas)
                {
                    Console.WriteLine("--------------------------------");

                    Console.WriteLine(
                        "Empresa: " + alerta.Tenant);

                    Console.WriteLine(
                        "Fecha y hora: " +
                        alerta.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"));

                    Console.WriteLine(
                        "Tipo: " + alerta.TipoIncidente);

                    Console.WriteLine(
                        "Puntuacion heuristica: " +
                        alerta.Confianza.ToString("0.00"));

                    Console.WriteLine(
                        "Motivo: " + alerta.Motivo);

                    Console.WriteLine(
                        "CPU: " + Formatear(alerta.Cpu) + "%");

                    Console.WriteLine(
                        "RAM: " + Formatear(alerta.Ram) + "%");

                    Console.WriteLine(
                        "Disco: " + Formatear(alerta.Disco) + "%");

                    Console.WriteLine(
                        "Latencia: " + Formatear(alerta.LatenciaMs) + " ms");

                    Console.WriteLine(
                        "Errores: " + Formatear(alerta.Errores));

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "Error al ejecutar la prueba: " +
                    ex.Message);
            }
        }

        private static string Formatear(double? valor)
        {
            if (!valor.HasValue)
                return "Sin dato";

            return valor.Value.ToString("0.##");
        }

    }
}

