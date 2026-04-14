public static  class AccountDataMapping
{
    public static PersonAccountData ToPersonAccountData(this PersonAccount account, string districtName, bool includePrivateData)
    {
        Public_PersonAccountData publicData = new Public_PersonAccountData(
            username: account.Username,
            email: account.Email,
            description: account.Description ?? string.Empty,
            profile_Picture_Link: account.ProfilePictureLink,
            district_Id: account.DistrictId,
            district_Name: districtName,
            visible_Ads:  new(),

            Birthday: account.Birthday
        );

        Private_PersonAccountData? privateData = null;
        if(includePrivateData == true)
        {
            privateData = new Private_PersonAccountData(
                advertisement_Slots_Amount: account.AdvertisementSlotsAmount,
                hidden_Ads: new()
            );
        }

        return new PersonAccountData(
            publicData,
            privateData
        );
    }

    public static BusinessAccountData ToBusinessAccountData(this BusinessAccount account, string districtName, bool includePrivateData)
    {
        Public_BusinessAccountData publicData = new Public_BusinessAccountData(
            username: account.Username,
            email: account.Email,
            description: account.Description ?? string.Empty,
            profile_Picture_Link: account.ProfilePictureLink,
            district_Id: account.DistrictId,
            district_Name: districtName,
            visible_Ads:  new(),

            cnpj: account.CNPJ
        );

        Private_BusinessAccountData? privateData = null;
        if(includePrivateData == true)
        {
            privateData = new Private_BusinessAccountData(
                advertisement_Slots_Amount: account.AdvertisementSlotsAmount,
                hidden_Ads: new()
            );
        }

        return new BusinessAccountData(
            publicData,
            privateData
        );
    }
}