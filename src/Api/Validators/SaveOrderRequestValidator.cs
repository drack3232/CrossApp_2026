using Api.Contracts;
using FluentValidation;

namespace Api.Validators;

public sealed class SaveOrderLineRequestValidator : AbstractValidator<SaveOrderLineRequest>
{
    public SaveOrderLineRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ID товару обов'язковий.")
            .MaximumLength(50).WithMessage("ID товару не може перевищувати 50 символів.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Назва товару обов'язкова.")
            .MaximumLength(200).WithMessage("Назва товару не може перевищувати 200 символів.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Ціна не може бути від'ємною.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Кількість має бути більшою за нуль.");
    }
}

public sealed class SaveOrderRequestValidator : AbstractValidator<SaveOrderRequest>
{
    public SaveOrderRequestValidator()
    {
        RuleFor(x => x.Customer)
            .NotEmpty().WithMessage("Ім'я клієнта обов'язкове.")
            .MaximumLength(200).WithMessage("Ім'я клієнта не може перевищувати 200 символів.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Адреса доставки обов'язкова.")
            .MaximumLength(300).WithMessage("Адреса доставки не може перевищувати 300 символів.");

        RuleFor(x => x.Comment)
            .MaximumLength(500).WithMessage("Коментар не може перевищувати 500 символів.");

        RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("Замовлення повинно містити хоча б одну позицію.");

        RuleForEach(x => x.Lines)
            .SetValidator(new SaveOrderLineRequestValidator());
    }
}