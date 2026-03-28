using Microsoft.AspNetCore.Authorization;

namespace RetailNexus.Api.Authorization;

public class RequirePermissionAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "Permission:";

    public RequirePermissionAttribute(string permission)
        : base($"{PolicyPrefix}{permission}")
    {
    }
}
