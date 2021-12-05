using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DidacticalEnigma.Mem.ViewModels.Authorization
{
    public class LogoutViewModel
    {
        [BindNever]
        public string RequestId { get; set; }
    }
}