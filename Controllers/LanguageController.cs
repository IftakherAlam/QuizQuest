using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System;

public class LanguageController : Controller
{
    [HttpPost]
    public IActionResult SetLanguage(string culture)
    {
        if (string.IsNullOrEmpty(culture))
        {
            culture = "en"; // Default to English
        }

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTime.UtcNow.AddYears(1) }
        );

        return Redirect(Request.Headers["Referer"].ToString()); // Redirect to previous page
    }
}
