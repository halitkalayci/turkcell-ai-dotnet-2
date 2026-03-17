using FluentValidation;

namespace OrderService.Application.Features.Orders.Commands.UpdateOrderStatus;

public sealed class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("OrderId boş olamaz.");

        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("Geçerli bir durum seçilmelidir.");
    }
}
