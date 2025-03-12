using System.Collections.Generic;

namespace Identity.API.Models.ConsentViewModels
{
    public class ConsentInputModel
    {
        public string Button {  get; set; }
        public IEnumerable<string> ScopesConseted { get; set; }

        public bool RememberUrl {  get; set; }
        public string ReturnUrl {  get; set; }
        public string Description { get; set; }
    }
}
