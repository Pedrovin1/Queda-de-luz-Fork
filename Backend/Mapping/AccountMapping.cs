
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

    public static PostAccountResponse ToPostAccountResponse(this BaseAccount account)
    {
        PersonAccountDetails? pDetails = null;
        BusinessAccountDetails? bDetails = null;

        if(account is PersonAccount personAccount){
            pDetails = new(personAccount.Birthday);
        }
        else{
            bDetails = new(((BusinessAccount)account).CNPJ);
        }

        return new PostAccountResponse(
            Username: account.Username, 
            Email: account.Email, 
            utc_Datetime_Creation: account.UTC_DatetimeCreation, 
            Advertisement_Slots_Amount: account.AdvertisementSlotsAmount, 
            District_Id: account.DistrictId,

            Person_Details: pDetails,
            Business_Details: bDetails 
        );
    }
}