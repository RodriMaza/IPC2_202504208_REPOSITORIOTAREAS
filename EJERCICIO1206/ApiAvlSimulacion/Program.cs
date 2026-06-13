using ApiAvlSimulacion;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Simulación del estado del árbol en memoria (Slide 5)
var estadoArbol = new List<NodoAVL>
{
    new NodoAVL { Id = 30, Etiqueta = "Nodo Raiz (Abuelo) - FE: -2" },
    new NodoAVL { Id = 10, Etiqueta = "Hijo Izquierdo - FE: +1" }
};

// Respuesta para la ruta raíz (redirige al endpoint principal para evitar el error 404)
app.MapGet("/", () => Results.Redirect("/api/arbol"));

// 1. ENDPOINT GET: Recupera la estructura física del árbol actual
app.MapGet("/api/arbol", () => Results.Ok(estadoArbol));

// 2. ENDPOINT POST: Simula la inserción que gatilla el balanceo compuesto
app.MapPost("/api/arbol/insertar", (NodoAVL nuevoNodo) =>
{
    // Validación básica de la llave
    if (nuevoNodo.Id <= 0) return Results.BadRequest("ID de nodo inválido.");

    // Simulación de la lógica del motor de inserción (Slide 8 y 10)
    // Al insertar el 20, se detecta el caso cruzado Izquierda-Derecha
    if (nuevoNodo.Id == 20)
    {
        // Limpiamos el árbol viejo
        estadoArbol.Clear();

        // El resultado de la rotación RID balancea perfectamente el árbol (Slide 9)
        estadoArbol.Add(new NodoAVL { Id = 20, Etiqueta = "Nueva Raiz Balanceada (RID) - FE: 0" });
        estadoArbol.Add(new NodoAVL { Id = 10, Etiqueta = "Hijo Izquierdo - FE: 0" });
        estadoArbol.Add(new NodoAVL { Id = 30, Etiqueta = "Hijo Derecho - FE: 0" });

        return Results.Created("/api/arbol", new 
        { 
            Mensaje = "Rotación RID ejecutada con éxito. Estabilidad total lograda.", 
            Estructura = estadoArbol 
        });
    }

    // Inserción tradicional sin rotación compuesta
    estadoArbol.Add(nuevoNodo);
    return Results.Created($"/api/arbol/{nuevoNodo.Id}", nuevoNodo);
});

app.Run();