using Common.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Radzen;
using Radzen.Blazor;
using S = Common.Shared.I18n.S;

namespace Common.Client;

public class CommonCustomValidator : ValidatorBase
{
    [Inject]
    private L L { get; set; } = default!;
    public override string Text { get; set; } = S.Invalid;
    public ValidationResult Result { get; set; } = ValidationResult.New();

    [Parameter]
    [EditorRequired]
    public Func<IRadzenFormComponent, ValidationResult> Validator { get; set; } = default!;

    protected override bool Validate(IRadzenFormComponent component)
    {
        Result = Validator(component);
        return Result.Valid;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (!Visible || IsValid)
            return;
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "style", Style);
        builder.AddAttribute(2, "class", GetCssClass());
        builder.AddMultipleAttributes(3, Attributes);
        RecursiveBuildRenderTree(builder, Result, 4);
        builder.CloseElement();
    }

    private void RecursiveBuildRenderTree(
        RenderTreeBuilder builder,
        ValidationResult res,
        int sequence
    )
    {
        builder.AddContent(sequence++, L.S(res.Message.Key, res.Message.Model));
        var subs = res.SubResults.Where(x => !x.Valid).ToList();
        if (subs.Any())
        {
            builder.OpenElement(sequence++, "ul");
            builder.AddAttribute(sequence++, "class", "p-l-1h m-y-0");
            foreach (var subRes in subs)
            {
                builder.OpenElement(sequence++, "li");
                RecursiveBuildRenderTree(builder, subRes, sequence);
                builder.CloseElement();
            }

            builder.CloseElement();
        }
    }
}
