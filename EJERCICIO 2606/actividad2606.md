

## Parte 1: 

### Formatos de Intercambio

| Formato | Ventajas | Desventajas |
| --- | --- | --- |
| **CSV** | Formato extremadamente ligero y de procesamiento rápido. Es ideal para representar grandes volúmenes de datos tabulares simples y requiere muy poco consumo de recursos para su lectura. | No soporta de forma nativa estructuras de datos complejas, jerárquicas o anidadas. Además, carece de un estándar para tipos de datos o metadatos, siendo todo texto plano. |
| **XML** | Soporta estructuras de datos complejas, profundamente anidadas y jerárquicas. Es autodescriptivo gracias a su fuerte enfoque en el uso de etiquetas. | Es sumamente "verboso" debido a las etiquetas de apertura y cierre, lo que genera archivos de gran peso y hace que el costo computacional de su parseo sea mucho más lento y exigente. |

### 1. Diferenciación de Procesos

Explicación de los procesos utilizando la librería nativa `System.Text.Json`:

* **Serialización:** Es el proceso mediante el cual se toma un objeto que está cargado en la memoria de la aplicación (una instancia en C#) y se convierte en una cadena de texto en formato JSON. Esto sirve para poder transmitir esos datos por la red o guardarlos de forma persistente.
* **Deserialización:** Es el proceso opuesto. Consiste en recibir o leer una cadena de texto estructurada en formato JSON y convertirla (mapearla) directamente en un objeto fuertemente tipado en C# para poder manipular su información internamente en el código.

### 2. El Antipatrón del Rendimiento (N+1 y Batching)

* **El error N+1 en lecturas masivas:** Consiste en realizar una operación individual de inserción a la base de datos por cada registro encontrado (es decir, una línea leída = un query a la BD). Esto es perjudicial porque multiplica el tiempo de I/O y el uso de transacciones de red, colapsando el rendimiento al enviar "N" consultas independientes.

* **Estrategia de Optimización (Batching):** La solución radica en leer múltiples registros, almacenarlos en memoria temporalmente (en una colección o lista), y enviar la colección completa a la base de datos en un solo viaje o bloque (lote/batch). Esto transforma "N" transacciones costosas en 1 sola, agilizando drásticamente la carga masiva de datos.

---

## Parte 2:

### Desafío 1: 
Consumo de Endpoints y Deserialización

El siguiente código implementa el consumo del endpoint https://api.usac.edu/v1/alumnos, manejando excepciones e implementando la propiedad de insensibilidad a mayúsculas y minúsculas al deserializar.

```csharp
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

public class IntegracionUsacService
{
    private readonly HttpClient _httpClient;

    public IntegracionUsacService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Alumno>> ObtenerAlumnosAsync()
    {
        try
        {
            // Petición GET a la URL solicitada
            HttpResponseMessage response = await _httpClient.GetAsync("https://api.usac.edu/v1/alumnos");
            
            // Validar el código de estado HTTP
            response.EnsureSuccessStatusCode();

            string jsonPayload = await response.Content.ReadAsStringAsync();

            // Configurar propiedades de deserialización para ignorar case sensitivity
            var opcionesJson = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // Deserializar el payload JSON a una lista de tipo Alumno
            var alumnos = JsonSerializer.Deserialize<List<Alumno>>(jsonPayload, opcionesJson);

            return alumnos;
        }
        catch (HttpRequestException httpEx)
        {
            Console.WriteLine($"Error al consumir el endpoint: {httpEx.Message}");
            throw;
        }
        catch (JsonException jsonEx)
        {
            Console.WriteLine($"Error de formato al deserializar el JSON: {jsonEx.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ocurrió un error inesperado: {ex.Message}");
            throw;
        }
    }
}
```

### Desafío 2: Endpoint para Carga Masiva CSV

Controlador que recibe el archivo mediante IFormFile, lo lee asíncronamente con StreamReader para no saturar la RAM e inserta los datos por lotes mediante AddRange() y un único SaveChangesAsync().

```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class CargaMasivaController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CargaMasivaController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("csv")]
    public async Task<IActionResult> CargarAlumnosMasivo(IFormFile archivo)
    {
        if (archivo == null || archivo.Length == 0)
        {
            return BadRequest("Debe enviar un archivo CSV válido.");
        }

        // Lista intermedia para almacenar los registros mapeados (Batching)
        var listaAlumnos = new List<Alumno>();

        // Uso de StreamReader para evitar saturación de memoria RAM
        using (var stream = archivo.OpenReadStream())
        using (var reader = new StreamReader(stream))
        {
            // Leer línea por línea de forma asíncrona
            while (!reader.EndOfStream)
            {
                var linea = await reader.ReadLineAsync();

                if (!string.IsNullOrWhiteSpace(linea))
                {
                    // Lógica de mapeo separando por comas (formato CSV)
                    var datos = linea.Split(',');

                    if (datos.Length >= 2)
                    {
                        var alumno = new Alumno
                        {
                            Carnet = datos[0].Trim(),
                            Nombre = datos[1].Trim()
                        };
                        
                        listaAlumnos.Add(alumno);
                    }
                }
            }
        }

        if (listaAlumnos.Count > 0)
        {
            // Inserción en un solo bloque por lotes y una única llamada de guardado
            _context.Alumnos.AddRange(listaAlumnos);
            await _context.SaveChangesAsync();
        }

        return Ok($"Carga finalizada con éxito. Registros procesados: {listaAlumnos.Count}");
    }
}
```

---

## Parte 3: Referencias Bibliográficas

* Facultad de Ingeniería, USAC. (2026). Sesión 20: Integración de Datos. Consumo de APIs Externas y Carga Masiva (CSV/XML). Laboratorio del curso Introducción a la Programación y Computación 2. Guatemala.