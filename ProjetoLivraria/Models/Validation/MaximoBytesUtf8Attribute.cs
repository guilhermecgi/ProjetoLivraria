using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ProjetoLivraria.Models.Validation;

[AttributeUsage(AttributeTargets.Property)]
public sealed class MaximoBytesUtf8Attribute : ValidationAttribute
{
    private readonly int _maximoBytes;

    public MaximoBytesUtf8Attribute(int maximoBytes)
    {
        _maximoBytes = maximoBytes;
    }

    public override bool IsValid(object? value)
    {
        return value is null || value is string texto && Encoding.UTF8.GetByteCount(texto) <= _maximoBytes;
    }
}
