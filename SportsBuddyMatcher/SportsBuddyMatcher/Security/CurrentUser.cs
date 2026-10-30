using System.Security.Claims;
using SportsBuddyMatcher.BusinessLogic.Interfaces;
namespace SportsBuddyMatcher.Security;
public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;
    public CurrentUser(IHttpContextAccessor accessor) { _accessor = accessor; }
    public int Id => int.Parse(_accessor.HttpContext!.User.FindFirstValue("sub")!);
}
