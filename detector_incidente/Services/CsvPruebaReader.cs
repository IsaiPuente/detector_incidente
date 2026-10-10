
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using detector_incidente.Models;

namespace detector_incidente.Services
{
    public class CsvPruebaReader
    {
        public List<MetricRecord> Leer(string ruta)
        {
            List<MetricRecord> registros =
                new List<MetricRecord>();

            using (StreamReader lector =
                new StreamReader(ruta))
            {
                string encabezado = lector.ReadLine();

                if (encabezado == null)
                    throw new Exception("El CSV está vacío.");

                string linea;

                while ((linea = lector.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(linea))
                        continue;

                    string[] c = linea.Split(',');

                    if (c.Length < 6)
                        continue;

                    DateTime fecha;

                    if (!DateTime.TryParse(
                        c[0],
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out fecha))
                    {
                        continue;
                    }

                    MetricRecord m = new MetricRecord();

                    m.Tenant = "Kaiku";
                    m.Timestamp = fecha;
                    m.Cpu = LeerNumero(c[1]);
                    m.Ram = LeerNumero(c[2]);
                    m.Disco = LeerNumero(c[3]);
                    m.LatenciaMs = LeerNumero(c[4]);
                    m.Errores = LeerNumero(c[5]);

                    registros.Add(m);
                }
            }

            return registros;
        }

        private double? LeerNumero(string texto)
        {
            double valor;

            if (string.IsNullOrWhiteSpace(texto))
                return null;

            if (double.TryParse(
                texto,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out valor))
            {
                return valor;
            }

            return null;
        }
    }
}
