﻿using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace ECommerce.Infrastructure.DataContext.Utilities;

public static class CookieHelper
{
    public static void SetCookie(this HttpContext context, string key, object value, double timeDifference,
        TimeSpan? duration = null)
    {
        try
        {
            var option = new CookieOptions();

            if (duration.HasValue)
                option.Expires = DateTime.Now.AddMinutes(timeDifference).Add(duration.Value);
            else
                option.Expires = DateTime.Now.AddMinutes(timeDifference).AddDays(30);

            var val = JsonSerializer.Serialize(value);
            // var val = JsonConvert.SerializeObject(value);
            context.Response.Cookies.Append(key, val, option);
        }
        catch
        {
        }
    }

    public static T GetCookie<T>(this HttpContext context, string key)
    {
        context.Request.Cookies.TryGetValue(key, out var value);
        if (value == null) return default;

        var val = JsonSerializer.Deserialize<T>(value);
        // var val = JsonConvert.DeserializeObject<T>(value);
        return val;
    }

    public static void RemoveCookie(this HttpContext context, string key)
    {
        foreach (var cookie in context.Request.Cookies)
            if (cookie.Key == key)
            {
                context.Response.Cookies.Append(key, "", new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(-1)
                });
                context.Response.Cookies.Delete(cookie.Key);
            }
    }
}