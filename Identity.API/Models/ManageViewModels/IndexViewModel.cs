using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Identity.API.Models.ManageViewModels
{
    public class IndexViewModel
    {
        public bool HasPassword { get; init; }
        public IList<UserLoginInfo> userLogins { get; init; }  

        public string PhoneNumber {  get; init; }
        public bool TwoFactor { get; init; }
        public bool BrowserRemembered { get; init; }
    }
}
