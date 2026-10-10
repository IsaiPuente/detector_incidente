
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace detector_incidente.Services
{
    public class LogPruebaReader
    {
        public class EventoLog
        {
            public DateTime Timestamp { get; set; }
            public string Nivel { get; set; }
            public string Mensaje { get; set; }
        }

        public List<EventoLog> Leer(string ruta)
        {
            List<EventoLog> eventos =
                new List<EventoLog>();

            using (StreamReader lector =
                new StreamReader(ruta))
            {
                string linea;

                while ((linea = lector.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(linea))
                        continue;

                    // Formato esperado:
                    // 2026-10-11T04:22:36 ERROR mensaje...

                    if (linea.Length < 20)
                        continue;

                    DateTime fecha;

                    string fechaTexto = linea.Substring(0, 19);

                    if (!DateTime.TryParseExact(
                        fechaTexto,
                        "yyyy-MM-dd'T'HH:mm:ss",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out fecha))
                    {
                        continue;
                    }

                    string resto = linea.Substring(19).Trim();

                    string nivel = "DESCONOCIDO";
                    string mensaje = resto;

                    string[] niveles = {
                        "INFO", "WARN", "ERROR", "DEBUG"
                    };

                    foreach (string candidato in niveles)
                    {
                        if (resto.StartsWith(
                            candidato + " ",
                            StringComparison.OrdinalIgnoreCase))
                        {
                            nivel = candidato;
                            mensaje = resto.Substring(
                                candidato.Length).Trim();
                            break;
                        }
                    }

                    EventoLog evento = new EventoLog();

                    evento.Timestamp = fecha;
                    evento.Nivel = nivel;
                    evento.Mensaje = mensaje;

                    eventos.Add(evento);
                }
            }

            eventos.Sort((a, b) =>
                a.Timestamp.CompareTo(b.Timestamp));

            return eventos;
        }
    }
}
