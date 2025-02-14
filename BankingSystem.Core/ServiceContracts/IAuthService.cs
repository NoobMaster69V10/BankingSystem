using BankingSystem.Core.Identity;

namespace BankingSystem.Core.ServiceContracts;

 public interface IAuthService
{
   Task<string> GenerateJwtToken(IdentityPerson user);
}
