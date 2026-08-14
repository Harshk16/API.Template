namespace API.Template.WebApi.Models
{
    public sealed record ResetPasswordModel(string Email, string Token, string NewPassword);
}
