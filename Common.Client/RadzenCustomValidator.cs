using Common.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Radzen;
using Radzen.Blazor;

namespace Common.Client;

public class CommonCustomValidator : ValidatorBase
{
    [Inject]
    private L L { get; set; } = default!;
    public override string Text { get; set; } = S.Invalid;
    public Message Message { get; set; } = new(S.Invalid);

    private List<Message> SubMessages { get; set; } = new();

    [Parameter]
    [EditorRequired]
    public Func<IRadzenFormComponent, ValidationResult> Validator { get; set; } = default!;

    protected override bool Validate(IRadzenFormComponent component)
    {
        var res = Validator(component);
        Message = res.Message;
        SubMessages = res.SubMessages;
        return res.Valid;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (!Visible || IsValid)
            return;
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "style", Style);
        builder.AddAttribute(2, "class", GetCssClass());
        builder.AddMultipleAttributes(3, Attributes);
        if (!Text.IsNullOrWhiteSpace())
            builder.AddContent(4, L.S(Message.Key, Message.Model));
        if (SubMessages.Any())
        {
            builder.OpenElement(5, "ul");
            builder.AddAttribute(6, "class", "p-l-1h m-y-0");
            foreach (var subRule in SubMessages)
            {
                builder.OpenElement(7, "li");
                builder.AddContent(8, L.S(subRule.Key, subRule.Model));
                builder.CloseElement();
            }

            builder.CloseElement();
        }

        builder.CloseElement();
    }
}
