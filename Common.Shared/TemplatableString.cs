using Fluid;

namespace Common.Shared;

public record TemplatableString(string Raw)
{
    internal IFluidTemplate? Template { get; set; }
}
