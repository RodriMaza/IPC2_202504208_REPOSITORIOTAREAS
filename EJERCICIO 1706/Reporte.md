# Reporte de Investigación: Arquitectura Multi-Nivel y Patrón MVC

## Parte 1: Fundamentación Teórica y Análisis Crítico

### 1. El Tránsito hacia los Sistemas Distribuidos y Multi-Capa

**La Limitación del Monolito Local**
Cuando la interfaz, la lógica y el almacenamiento residen de forma exclusiva en una máquina física aislada, el sistema sufre de cuellos de botella severos. La escalabilidad es limitada porque todos los procesos compiten por la misma CPU, RAM y disco duro. En cuanto a la sincronización de datos, al tener todo centralizado, cualquier sobrecarga en la interfaz de usuario puede bloquear directamente el acceso a la base de datos, creando un único punto de fallo para todo el sistema.

**Distinción Crítica (Layers vs. Tiers)**
* **Capas Lógicas (Layers):** Representan la organización interna y la separación del código a nivel de software dentro de una misma aplicación o proyecto (ej. separar clases de acceso a datos de las clases de interfaz).
* **Niveles Físicos (Tiers):** Se refieren a la distribución física del hardware. Dictan en qué servidores, contenedores o máquinas distintas se ejecutan los componentes lógicos.

**Responsabilidades en la Arquitectura de 3 Niveles**
* **Nivel 1: Capa de Presentación (Presentation Tier):** Su misión exclusiva es interactuar con el usuario final, capturar sus entradas y mostrar la información procesada. *Tecnología común:* Navegadores web, HTML, CSS, JavaScript o aplicaciones móviles.
* **Nivel 2: Capa de Aplicación o Negocio (Application Tier):** Su misión es procesar las reglas de negocio, realizar cálculos y actuar como puente seguro entre la presentación y los datos. *Tecnología común:* Servidores web ejecutando C# (.NET), Java, Python o Node.js.
* **Nivel 3: Capa de Datos (Data Tier):** Su misión exclusiva es el almacenamiento, la persistencia y la recuperación segura de la información. *Tecnología común:* Motores de bases de datos relacionales (SQL Server, MySQL, PostgreSQL) o NoSQL.

**Seguridad Perimetral**
Desde la perspectiva de ingeniería, es un error crítico exponer públicamente el puerto de una base de datos a internet porque la hace vulnerable a ataques de fuerza bruta, escaneo de puertos e inyección de código, comprometiendo la integridad de toda la información de la empresa. La **buena práctica recomendada** es mantener la base de datos aislada en una red privada (VPC), permitiendo únicamente conexiones entrantes desde la IP autorizada del servidor que aloja la Capa de Aplicación.

---

### 2. Desacoplamiento Lógico con el Patrón MVC

**La Crisis del Código Espagueti**
Mezclar sentencias SQL, lógica matemática y etiquetas visuales (HTML) dentro de un mismo archivo físico hace que el mantenimiento del software sea una pesadilla. Cualquier cambio visual puede romper la lógica de la base de datos por error. Además, imposibilita el diseño de pruebas unitarias automatizadas, ya que no se puede evaluar una función matemática o de negocio sin tener que renderizar simultáneamente la interfaz gráfica y conectar a la base de datos.

**Separación de Preocupaciones (SoC)**
* **Modelo:** Representa las estructuras de datos y las reglas del dominio de negocio. *Aislamiento:* No debe conocer en absoluto cómo se muestran los datos al usuario ni qué interfaz lo está consumiendo.
* **Vista:** Es una entidad pasiva encargada de la interfaz de usuario. *Aislamiento:* Tiene estrictamente prohibido contener lógica de negocio, reglas matemáticas, o hacer consultas directas a la base de datos. Solo recibe datos y los dibuja.
* **Controlador:** Es el intermediario táctico y "director de orquesta". *Aislamiento:* Recibe las peticiones HTTP del usuario, invoca las operaciones en el Modelo y decide qué Vista debe mostrarse, inyectándole los datos correspondientes.

**Métricas de Ingeniería de Software**
El patrón MVC fomenta una **Alta Cohesión** porque cada archivo tiene un propósito único y altamente enfocado (la vista solo hace diseño, el modelo solo datos). Promueve un **Bajo Acoplamiento** porque los componentes interactúan mediante interfaces y reglas claras; puedes cambiar completamente el diseño de la Vista sin necesidad de tocar ni una sola línea de código del Modelo o de la base de datos.

---

## Parte 2: Modelado del Ciclo de Vida y Enrutamiento Semántico

### 1. Mapeo Analítico de URLs

| URL Entrante del Cliente | Clase Controladora Buscada por el Framework | Método (Acción) Ejecutado | Parámetro Inyectado `id` |
| :--- | :--- | :--- | :--- |
| `https://ingenieria.usac.edu.gt/ControlAcademico/Login` | `ControlAcademicoController` | `Login` | *(Ninguno / Opcional)* |
| `https://ingenieria.usac.edu.gt/Estudiante/Historial/20260123` | `EstudianteController` | `Historial` | `20260123` |
| `https://ingenieria.usac.edu.gt/Asignacion/Detalle/10` | `AsignacionController` | `Detalle` | `10` |
| `https://ingenieria.usac.edu.gt/Home` | `HomeController` | `Index` | *(Ninguno / Opcional)* |

### 2. Diagramación del Flujo Interactivo

1. **Interacción del Usuario:** El usuario hace clic en un enlace o botón en su navegador web, generando una petición HTTP que viaja por la red hacia el servidor.
2. **Enrutamiento:** El framework web (ASP.NET Core) recibe la petición HTTP, analiza la URL entrante mediante su motor de enrutamiento y determina a qué **Controlador** y Acción (método) debe delegar el trabajo.
3. **Procesamiento y Modelo:** El **Controlador** recibe la orden, valida los datos de entrada y se comunica con el **Modelo** para consultar o guardar la información necesaria en la base de datos.
4. **Preparación de la Respuesta:** El **Modelo** retorna los datos de negocio limpios al **Controlador**. Este último selecciona la **Vista** apropiada y le inyecta (pasa) la información obtenida.
5. **Renderizado (HTML Final):** La **Vista**, que es pasiva, toma esos datos y los utiliza para generar dinámicamente el código HTML final, el cual es devuelto al navegador del usuario como una respuesta HTTP (200 OK) para ser visualizado en pantalla.

---

## Parte 4: Auditoría y Control de Calidad

1. **Prueba de Cohesión (GET):** Se verificó que el método `Listar` de `EstudianteController` retorna una respuesta limpia. El controlador cumplió su propósito al inyectar directamente `_baseDatosMemoria` hacia la Vista, absteniéndose de mezclar sentencias SQL o realizar cálculos lógicos.
2. **Evaluación de Antipatrones (Skinny Controllers):** El método `Registrar` fue diseñado aplicando el principio de validación perimetral rápida en menos de 10 líneas lógicas de código, evitando exitosamente la construcción de "Controladores Gordos" (Fat Controllers).

---
