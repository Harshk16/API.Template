namespace API.Template.WebApi.Models
{
    public sealed record ChangePasswordModel(string CurrentPassword, string NewPassword);
}
