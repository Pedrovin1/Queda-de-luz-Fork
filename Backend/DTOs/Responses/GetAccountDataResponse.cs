public record GetAccountDataResponse(
    PersonAccountData? person_Account_Data,
    BusinessAccountData? business_Account_Data
);
//----------------------------------------------------------

public record AdvertisementSummary(
    long    ad_Id,
    string  ad_Text,
    string? ad_Image_Link,
    string? ad_Redirect_Link,
    bool    is_Hidden
);



//----------------------------------------------------------

public record PersonAccountData(
    Public_PersonAccountData   public_Data,
    Private_PersonAccountData? private_Data
);

public record Public_PersonAccountData(
    string username,
    string email,
    string description,
    string? profile_Picture_Link,
    long district_Id,
    string district_Name,

    List<AdvertisementSummary> visible_Ads,

    DateOnly Birthday
);
public record Private_PersonAccountData(
    long advertisement_Slots_Amount,
    List<AdvertisementSummary> hidden_Ads
);
//----------------------------------------------------------

public record BusinessAccountData(
    Public_BusinessAccountData   public_Data,
    Private_BusinessAccountData? private_Data
);

public record Public_BusinessAccountData(
    string username,
    string email,
    string description,
    string? profile_Picture_Link,
    long district_Id,
    string district_Name,

    List<AdvertisementSummary> visible_Ads,

    string cnpj
);

public record Private_BusinessAccountData(
    long advertisement_Slots_Amount,
    List<AdvertisementSummary> hidden_Ads
);
