﻿using FluentValidation;
using CaseStudy.Application.Models.User;

namespace CaseStudy.Application.Models.Validators.User;

public class ConfirmEmailModelValidator : AbstractValidator<ConfirmEmailModel>
{
    public ConfirmEmailModelValidator()
    {
        RuleFor(ce => ce.Token)
            .NotEmpty()
            .WithMessage("Your verification link is not valid");

        RuleFor(ce => ce.UserName)
            .NotEmpty()
            .WithMessage("Your verification link is not valid");
    }
}