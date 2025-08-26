using Microsoft.AspNetCore.Mvc;
using RandomNumbersApi.Models;
using System.Security.Cryptography;
using System.Text;

namespace RandomNumbersApi.Controllers;

[ApiController]
[Route("random")] // La ruta base queda /random/...
public class RandomNumbersAPI : ControllerBase   // 👈 el nombre de tu clase
{
    // GET /random/number  (entero no negativo)
    // GET /random/number?min=10&max=50  (en rango [min,max])
    [HttpGet("number")]
    public ActionResult<int> GetNumber([FromQuery] int? min, [FromQuery] int? max)
    {
        // Caso sin rango: devuelve un entero aleatorio estándar
        if (min is null && max is null)
            return Ok(Random.Shared.Next()); // 0 .. int.MaxValue-1

        // Validación de parámetros
        if (min is null || max is null)
            return BadRequest("Debes enviar ambos parámetros: min y max.");

        if (min > max)
            return BadRequest("min no puede ser mayor que max.");

        // Evitar overflow de max+1
        if (max == int.MaxValue)
            return BadRequest("max debe ser menor que int.MaxValue para incluir el extremo.");

        // En .NET Next(a,b) es [a, b) por eso sumamos 1 al límite superior.
        var value = Random.Shared.Next(min.Value, max.Value + 1);
        return Ok(value);
    }

    // GET /random/decimal  (decimal entre 0 y 1)
    [HttpGet("decimal")]
    public ActionResult<decimal> GetDecimal()
    {
        // NextDouble() devuelve [0,1)
        var value = (decimal)Random.Shared.NextDouble();
        return Ok(value);
    }

    // GET /random/string?length=8
    [HttpGet("string")]
    public ActionResult<string> GetString([FromQuery] int length = 8)
    {
        if (length < 1 || length > 1024)
            return BadRequest("length debe estar entre 1 y 1024.");

        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var sb = new StringBuilder(length);

        // Usamos RNG criptográfico para mejor aleatoriedad de caracteres
        for (int i = 0; i < length; i++)
        {
            int idx = RandomNumberGenerator.GetInt32(0, chars.Length);
            sb.Append(chars[idx]);
        }
        return Ok(sb.ToString());
    }

    // POST /random/custom
    // Body JSON:
    // {
    //   "type":"number" | "decimal" | "string",
    //   "min":100, "max":200,          // solo number
    //   "decimals":2,                  // solo decimal (default 2)
    //   "length":10                    // solo string (default 8)
    // }
    [HttpPost("custom")]
    public ActionResult<CustomRandomResponse> PostCustom([FromBody] CustomRandomRequest req)
    {
        if (req.Type is null)
            return BadRequest("Debes indicar 'type': 'number' | 'decimal' | 'string'.");

        switch (req.Type.Trim().ToLowerInvariant())
        {
            case "number":
                if (req.Min is null || req.Max is null)
                    return BadRequest("Para type='number' debes enviar 'min' y 'max'.");

                if (req.Min > req.Max)
                    return BadRequest("min no puede ser mayor que max.");

                if (req.Max == int.MaxValue)
                    return BadRequest("max debe ser menor que int.MaxValue para incluir el extremo.");

                var n = Random.Shared.Next(req.Min.Value, req.Max.Value + 1);
                return Ok(new CustomRandomResponse { Result = n });

            case "decimal":
            {
                int decimals = req.Decimals ?? 2;
                if (decimals < 0 || decimals > 10)
                    return BadRequest("decimals debe estar entre 0 y 10.");

                var d = (decimal)Random.Shared.NextDouble(); // [0,1)
                d = Math.Round(d, decimals);
                return Ok(new CustomRandomResponse { Result = d });
            }

            case "string":
            {
                int length = req.Length ?? 8;
                if (length < 1 || length > 1024)
                    return BadRequest("length debe estar entre 1 y 1024.");

                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                var sb = new StringBuilder(length);
                for (int i = 0; i < length; i++)
                {
                    int idx = RandomNumberGenerator.GetInt32(0, chars.Length);
                    sb.Append(chars[idx]);
                }
                return Ok(new CustomRandomResponse { Result = sb.ToString() });
            }

            default:
                return BadRequest("type inválido. Usa 'number', 'decimal' o 'string'.");
        }
    }
}