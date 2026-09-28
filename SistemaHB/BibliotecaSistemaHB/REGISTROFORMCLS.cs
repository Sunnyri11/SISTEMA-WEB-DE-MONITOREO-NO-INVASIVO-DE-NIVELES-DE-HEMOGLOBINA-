using System;

namespace SistemaHBAPI.Models;

public class REGISTROFORMCLS
{
    public string Nombre { get; set; } = null!;
    public string ApellidoPaterno { get; set; } = null!;
    public string ApellidoMaterno { get; set; } = null!;
    public string Genero { get; set; } = null!; 
    public DateOnly FechaNacimiento { get; set; }
    public string Departamento { get; set; } = null!;
    public string Provincia { get; set; } = null!;
    public decimal Altitud { get; set; }
    public string Correo { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Rol {  get; set; } = null!;
    public int idEspecialidad {  get; set; }
}
