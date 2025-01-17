﻿using ECommerce.Application.ViewModels;
using Microsoft.Extensions.Options;

namespace ECommerce.Services.Services;

public class UserService(IHttpService http, ICookieService cookieService, IOptions<SmsIrSettings> options)
    : EntityService<User>(http), IUserService
{
    private const string Url = "api/Users";
    private readonly SmsIrSettings _smsSettings = options.Value;

    public async Task<bool> GetVerificationByNationalId(string nationalId)
    {
        if (nationalId == null || nationalId.Length != 10) return false;
        var nationalIdArray = new int[10];
        var sum = 0;
        for (var i = 0; i < nationalId.Length; i++)
        {
            nationalIdArray[i] = int.Parse(nationalId[i].ToString());
            if (i < 9) sum += nationalIdArray[i] * (10 - i);
        }

        var remainder = sum % 11;
        if ((remainder < 2 && nationalIdArray[9] == remainder) ||
            (remainder >= 2 && nationalIdArray[9] == 11 - remainder)) return true;

        return false;
    }

    public async Task<ServiceResult> Logout()
    {
        var result = await http.GetAsync<bool>(Url, "LogoutUser");
        if (result.Code == ResultCode.Success)
            return new ServiceResult
            {
                Code = ServiceCode.Success,
                Message = result.Messages?.FirstOrDefault()
            };
        return new ServiceResult
        {
            Code = ServiceCode.Error,
            Message = result.GetBody()
        };
    }

    public async Task<ServiceResult<LoginViewModel>> Login(LoginViewModel loginViewModel)
    {
        var result = await http.PostAsync<LoginViewModel, string>(Url, loginViewModel, "Login");
        if (result.Code == 0)
        {
            await cookieService.SetToken(result.ReturnData);

            var resultCurrentUser = cookieService.GetCurrentUser();


            return new ServiceResult<LoginViewModel>
            {
                Code = ServiceCode.Success,
                Message = "با موفقیت وارد شدید",
                ReturnData = resultCurrentUser
            };
        }

        if (result.Code == ResultCode.NotFound)
            return new ServiceResult<LoginViewModel>
            {
                Code = ServiceCode.Warning,
                Message = "نام کاربر یا کلمه عبور اشتباه می باشد"
            };
        return new ServiceResult<LoginViewModel>
        {
            Code = ServiceCode.Error,
            Message = "مشکل در ارتباط با سرور"
        };
    }

    public async Task<ServiceResult> Register(RegisterViewModel registerViewModel)
    {
        var result = await http.PostAsync(Url, registerViewModel, "Register");

        if (result.Code != 0 || result.Status != 200)
            return new ServiceResult
            {
                Code = ServiceCode.Error,
                Message = result.GetBody()
            };

        var loginViewModel = new LoginViewModel
        {
            Username = registerViewModel.Username,
            Password = registerViewModel.Password,
            RememberMe = true
        };
        await Login(loginViewModel);

        return new ServiceResult
        {
            Code = ServiceCode.Success,
            Message = "ثبت نام با موفقیت انجام شد"
        };
    }

    public async Task<ServiceResult> Update(User user)
    {
        var result = await http.PutAsync(Url, user);

        return new ServiceResult
        {
            Code = (ServiceCode)result.Code,
            Message = result.GetBody()
        };
    }

    public async Task<ServiceResult> ChangePassword(string oldPass, string newPass, string newConPass)
    {
        if (!newPass.Equals(newConPass))
            return new ServiceResult
            {
                Code = ServiceCode.Success,
                Message = "پسوردها مطابقت ندارند"
            };
        var user = cookieService.GetCurrentUser();
        var resetPasswordViewModel = new ResetPasswordViewModel
        {
            Password = newPass,
            OldPassword = oldPass,
            Username = user.Username
        };
        var result = await http.PostAsync(Url, resetPasswordViewModel, "ResetPassword");

        return new ServiceResult
        {
            Code = ServiceCode.Success,
            Message = result.GetBody()
        };
    }

    public async Task<ServiceResult> ChangeForgotPassword(ResetForgotPasswordViewModel resetForgotPasswordViewModel)
    {
        if (!resetForgotPasswordViewModel.Password.Equals(resetForgotPasswordViewModel.ConPass))
            return new ServiceResult
            {
                Code = ServiceCode.Success,
                Message = "پسوردها مطابقت ندارند"
            };
        var result = await http.PostAsync(Url, resetForgotPasswordViewModel, "ResetForgotPassword");

        return new ServiceResult
        {
            Code = ServiceCode.Success,
            Message = result.GetBody()
        };
    }

    public async Task<ServiceResult> ForgotPassword(string email)
    {
        var forgotPasswordViewModel = new ForgotPasswordViewModel { EmailOrPhoneNumber = email };
        var result = await http.PostAsync(Url, forgotPasswordViewModel, "ForgotPassword");

        return new ServiceResult
        {
            Code = ServiceCode.Success,
            Message = result.GetBody()
        };
    }

    public async Task<ServiceResult<List<UserListViewModel>>> UserList(string search = "",
        int pageNumber = 0, int pageSize = 10, int userSort = 1, bool? isActive = null, bool? isColleague = null,
        bool? HasBuying = null)
    {
        //var result = await _http.GetAsync<List<ProductIndexPageViewModel>>(Url, $"NewProducts?count={count}");
        //return Return<List<ProductIndexPageViewModel>>(result);
        var command = "Get?" +
                      $"PaginationParameters.PageNumber={pageNumber}&" +
                      $"PaginationParameters.PageSize={pageSize}&";
        if (!string.IsNullOrEmpty(search)) command += $"PaginationParameters.Search={search}&";
        if (isActive != null) command += $"IsActive={isActive}&";
        if (isColleague != null) command += $"IsColleauge={isColleague}&";
        if (HasBuying != null) command += $"HasBuying={HasBuying}&";
        command += $"UserSort={userSort}";
        var result = await http.GetAsync<List<UserListViewModel>>(Url, command);
        return Return(result);
    }

    public async Task<ServiceResult<User>> GetUser()
    {
        var resultCurrentUser = cookieService.GetCurrentUser();
        var command = "Get/" + resultCurrentUser.Id;
        var result = await http.GetAsync<User>(Url, command);
        return Return(result);
    }

    public async Task<ServiceResult<User>> GetById(int id)
    {
        var result = await http.GetAsync<User>(Url, $"GetById?id={id}");
        return Return(result);
    }

    public async Task<ServiceResult> Delete(int id)
    {
        //var result = await Delete(Url, id);
        //return Return(result);
        var result = await http.DeleteAsync(Url, id);
        if (result.Code == ResultCode.Success)
            return new ServiceResult
            {
                Code = ServiceCode.Success,
                Message = "با موفقیت حذف شد"
            };
        return new ServiceResult
            { Code = ServiceCode.Error, Message = "به علت وابستگی با عناصر دیگر امکان حذف وجود ندارد" };
    }

    public async Task<ServiceResult> Edit(User user)
    {
        var result = await http.PutAsync(Url, user);
        return Return(result);
    }

    public async Task<List<ResponseVerifySmsIrViewModel>> SendInvoiceSms(string invoice, string[] mobile, string persianDate)
    {
        var result = new List<ResponseVerifySmsIrViewModel>();
        var apiKey = _smsSettings.apikey;
        var apiName = _smsSettings.apiName;
        var url = _smsSettings.url;
        var invoiceParameter = new RequestVerifySmsIrParameters
        {
            Name = "INVOICE",
            Value = invoice
        };
        var dateParameter = new RequestVerifySmsIrParameters
        {
            Name = "DATE",
            Value = persianDate
        };
        foreach (var item in mobile)
        {
            var requestSmsIrViewModel = new RequestVerifySmsIrViewModel
            {
                Parameters = new[] { invoiceParameter, dateParameter },
                TemplateId = _smsSettings.invoiceTemplateId,
                Mobile = item
            };
            var response =
                await http.PostAsyncWithApiKeyByRequestModel<RequestVerifySmsIrViewModel, ResponseVerifySmsIrViewModel>(
                    apiName, apiKey, requestSmsIrViewModel, url);
            result.Add(response);
        }
        return result;
    }

    public async Task<ResponseVerifySmsIrViewModel> SendAuthenticationSms(string? mobile, string code)
    {
        var apiKey = _smsSettings.apikey;
        var apiName = _smsSettings.apiName;
        var url = _smsSettings.url;
        var RequestSMSIrViewModel = new RequestVerifySmsIrViewModel();
        var RequestVerifySmsIrParameter = new RequestVerifySmsIrParameters();
        RequestVerifySmsIrParameter.Name = "CODE";
        RequestVerifySmsIrParameter.Value = code;
        RequestSMSIrViewModel.Parameters = new[] { RequestVerifySmsIrParameter };
        RequestSMSIrViewModel.TemplateId = _smsSettings.authenticationTemplateId;
        RequestSMSIrViewModel.Mobile = mobile;
        var result =
            await http.PostAsyncWithApiKeyByRequestModel<RequestVerifySmsIrViewModel, ResponseVerifySmsIrViewModel>(
                apiName, apiKey, RequestSMSIrViewModel, url);
        return result;
    }

    public async Task<ServiceResult<bool>> SetConfirmCodeByUsername(string username, string confirmCode)
    {
        var result = await http.GetAsync<bool>(Url, $"SetConfirmCodeByUsername?username={username}" +
                                                     $"&confirmCode={confirmCode}");
        return Return(result);
    }

    public async Task<ServiceResult<int?>> GetSecondsLeftConfirmCodeExpire(string username)
    {
        var result = await http.GetAsync<int?>(Url, $"GetSecondsLeftConfirmCodeExpire?username={username}");
        return Return(result);
    }

    private ServiceResult<TResult> Return<TResult>(ApiResult<TResult> result)
    {
        if (result is { Code: ResultCode.Success })
            return new ServiceResult<TResult>
            {
                PaginationDetails = result.PaginationDetails,
                Code = ServiceCode.Success,
                ReturnData = result.ReturnData,
                Message = result.Messages?.FirstOrDefault()
            };
        var typeOfTResult = Activator.CreateInstance(typeof(TResult));
        return new ServiceResult<TResult>
        {
            Code = ServiceCode.Error,
            Message = result?.GetBody(),
            ReturnData = (TResult)typeOfTResult
        };
    }
}