using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ProjetoLivraria.ModelBinders;

public sealed class ValorMonetarioModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var entrada = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, entrada);
        var texto = entrada.FirstValue?.Trim();

        if (string.IsNullOrEmpty(texto))
        {
            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Informe o valor do livro.");
        }
        else if (!decimal.TryParse(texto.Replace(',', '.'),
                     NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                     CultureInfo.InvariantCulture, out var valor) || decimal.Round(valor, 2) != valor)
        {
            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName,
                "Informe um valor válido com até duas casas decimais, sem separador de milhares.");
        }
        else
        {
            bindingContext.ModelState.SetModelValue(bindingContext.ModelName,
                new ValueProviderResult(valor.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture));
            bindingContext.Result = ModelBindingResult.Success(valor);
        }

        return Task.CompletedTask;
    }
}
