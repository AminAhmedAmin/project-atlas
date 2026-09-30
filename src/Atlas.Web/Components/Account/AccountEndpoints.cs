using Atlas.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Web.Components.Account;

internal static class AccountEndpoints
{
    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // Binding a form value makes the endpoint validate the antiforgery token.
        endpoints.MapPost("/account/logout", async (
            [FromServices] SignInManager<ApplicationUser> signInManager,
            [FromForm] string? returnUrl) =>
        {
            await signInManager.SignOutAsync();
            var target = returnUrl is { Length: > 0 } && returnUrl.StartsWith('/') && !returnUrl.StartsWith("//", StringComparison.Ordinal)
                ? returnUrl
                : "/account/login";
            return TypedResults.LocalRedirect(target);
        });

        return endpoints;
    }
}
