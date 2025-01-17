﻿using System.ComponentModel.DataAnnotations;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.ViewModels;

public class LoginViewModel
{
    public int Id { get; set; }

    [Display(Name = "نام کاربری")]
    [Required(ErrorMessage = @"{0} را وارد کنید")]
    [StringLength(30, MinimumLength = 4, ErrorMessage = @"حداقل 4 و حداکثر 30 کاراکتر")]
    public string Username { get; set; }

    public string? FullName { get; set; }

    [Display(Name = "کلمه عبور")]
    [Required(ErrorMessage = @"{0} را وارد کنید")]
    [StringLength(30, MinimumLength = 8, ErrorMessage = @"حداقل 8 و حداکثر 30 کاراکتر")]
    public string Password { get; set; }

    public string? Token { get; set; }
    public bool IsActive { get; set; }

    public bool IsColleague { get; set; }

    [Display(Name = "مرا به خاطر بسپار")] public bool RememberMe { get; set; }

    public string? AuthName { get; set; }
    public int? ConfirmCode { get; set; }
    public DateTime? ConfirmCodeExpirationDate { get; set; }


    public static implicit operator LoginViewModel(User user)
    {
        return new LoginViewModel
        {
            Username = user.UserName,
            IsColleague = user.IsColleague,
            AuthName = user.UserRole.Name,
            Id = user.Id,
            IsActive = user.IsActive
        };
    }

    public static implicit operator User(LoginViewModel u)
    {
        try
        {
            return new User
            {
                UserName = u.Username,
                ConfirmCode = u.ConfirmCode,
                ConfirmCodeExpirationDate = u.ConfirmCodeExpirationDate
            };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}