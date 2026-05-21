namespace Application.DTOs.Personal;

public class CreatePersonalDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Cedula { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? TempPass { get; set; }
    public DateTime? Fecha { get; set; }
}