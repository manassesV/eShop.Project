namespace Identity.API.Models.AccountViewModels
{
    public class LoggedOutViewModel
    {
        public string PostLogoutRedirectUri { get; init; }
        public string ClientName { get; init; }
        public string SignOutFrameUri { get; init; }
    }
}
