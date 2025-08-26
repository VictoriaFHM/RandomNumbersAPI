namespace RandomNumbersApi.Models;

public class CustomRandomRequest
{
    // "number" | "decimal" | "string"
    public string? Type { get; set; }

    // Solo aplica a type = "number"
    public int? Min { get; set; }
    public int? Max { get; set; }

    // Solo aplica a type = "decimal" (por defecto 2)
    public int? Decimals { get; set; }

    // Solo aplica a type = "string" (por defecto 8)
    public int? Length { get; set; }
}