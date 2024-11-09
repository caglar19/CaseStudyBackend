﻿using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using CaseStudy.Application.Common.Email;
using CaseStudy.Application.Exceptions;
using CaseStudy.Application.Helpers;
using CaseStudy.Application.Models;
using CaseStudy.Application.Models.User;
using CaseStudy.Application.Templates;
using CaseStudy.Core.Entities;

namespace CaseStudy.Application.Services.Impl;

public class UserService : IUserService
{
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;
    private readonly SignInManager<User> _signInManager;
    private readonly ITemplateService _templateService;
    private readonly UserManager<User> _userManager;

    public UserService(IMapper mapper,
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IConfiguration configuration,
        ITemplateService templateService,
        IEmailService emailService)
    {
        _mapper = mapper;
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _templateService = templateService;
        _emailService = emailService;
    }

    public async Task<CreateUserResponseModel> CreateAsync(CreateUserModel createUserModel)
    {
        var user = _mapper.Map<User>(createUserModel);

        var result = await _userManager.CreateAsync(user, createUserModel.Password);

        if (!result.Succeeded) throw new BadRequestException(result.Errors.FirstOrDefault()?.Description);

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        var emailTemplate = await _templateService.GetTemplateAsync(TemplateConstants.ConfirmationEmail);

        var emailBody = _templateService.ReplaceInTemplate(emailTemplate,
            new Dictionary<string, string> { { "{UserId}", user.RefId.ToString() }, { "{Token}", token } });

        await _emailService.SendEmailAsync(EmailMessage.Create(user.Email, emailBody, "[N-Tier]Confirm your email"));

        return _mapper.Map<CreateUserResponseModel>(user);
    }

    public async Task<LoginResponseModel> LoginAsync(LoginUserModel loginUserModel)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == loginUserModel.Email);

        if (user == null)
            throw new NotFoundException("Username or password is incorrect");

        var signInResult = await _signInManager.PasswordSignInAsync(user, loginUserModel.Password, false, false);

        if (!signInResult.Succeeded)
            throw new BadRequestException("Username or password is incorrect");

        var token = JwtHelper.GenerateToken(user, _configuration);

        return new LoginResponseModel
        {
            Token = token
        };
    }

    public async Task<ConfirmEmailResponseModel> ConfirmEmailAsync(ConfirmEmailModel confirmEmailModel)
    {
        var user = await _userManager.FindByNameAsync(confirmEmailModel.UserName);

        if (user == null)
            throw new UnprocessableRequestException("Your verification link is incorrect");

        var result = await _userManager.ConfirmEmailAsync(user, confirmEmailModel.Token);

        if (!result.Succeeded)
            throw new UnprocessableRequestException("Your verification link has expired");

        return new ConfirmEmailResponseModel
        {
            Confirmed = true
        };
    }

    public async Task<BaseResponseModel> ChangePasswordAsync(Guid userId, ChangePasswordModel changePasswordModel)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null)
            throw new NotFoundException("User does not exist anymore");

        var result =
            await _userManager.ChangePasswordAsync(user, changePasswordModel.OldPassword,
                changePasswordModel.NewPassword);

        if (!result.Succeeded)
            throw new BadRequestException(result.Errors.FirstOrDefault()?.Description);

        return new BaseResponseModel
        {
            //Id = Guid.Parse(user.Id)
        };
    }
}