namespace ApiAvlSimulacion
{
    public class NodoAVL
    {
        public int Id { get; set; } // Actúa como el Dato/Llave
        public string Etiqueta { get; set; } = string.Empty;
        public int Altura { get; set; } = 1;
    }
}