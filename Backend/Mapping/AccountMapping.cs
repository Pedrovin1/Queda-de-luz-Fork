public static class AccountMapping
{
    public static PersonAccount ToPersonAccount(this PostAccountRequest request, string hashedPassword )
    {
          return new PersonAccount(
            Birthday: request.Person_Details!.Birthday,

            Username: request.Username, 
            Hashed_password: hashedPassword, 
            Email: request.Email, 
            District_id: request.District_Id 
        );
    }

    public static BusinessAccount ToBusinessAccount(this PostAccountRequest request, string hashedPassword)
    {
        return new BusinessAccount(
            Cnpj: request.Business_Details!.CNPJ,
            
            Username: request.Username, 
            Hashed_password: hashedPassword, 
            Email: request.Email, 
            District_id: request.District_Id 
        );
    }
}