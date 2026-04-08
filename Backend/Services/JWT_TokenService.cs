


using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

public class JWT_TokenService
{
    private byte[] _secretKey ;

    public JWT_TokenService(string secret)
    {
        this._secretKey = System.Text.Encoding.UTF8.GetBytes(secret);
    }

    public string CreateToken(JWT_AccountData accountData)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(this._secretKey), 
            SecurityAlgorithms.HmacSha256Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {  
            //<<TODO: to define Expiration time>>
            Subject = this.GenerateClaimsIdentity(accountData),
            SigningCredentials = signingCredentials
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private ClaimsIdentity GenerateClaimsIdentity(JWT_AccountData accountData)
    {
        var claimsIdentity = new ClaimsIdentity();

        claimsIdentity.AddClaim( 
            new Claim(ClaimTypes.NameIdentifier, 
                      accountData.Id.ToString())
        );

        string accountType = accountData.AccountType == typeof(PersonAccount) ? nameof(PersonAccount) 
                                                      : nameof(BusinessAccount);
        claimsIdentity.AddClaim( 
            new Claim(ClaimTypes.Role, 
                      accountType)
        );

        return claimsIdentity;
    } 

}