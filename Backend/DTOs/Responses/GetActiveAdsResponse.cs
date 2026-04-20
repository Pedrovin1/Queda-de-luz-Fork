public record GetActiveAdsResponse(
    List<ActiveAds> active_Ads
);

public record ActiveAds(
    long    ad_Id,
    string  ad_Text,
    string? ad_Image_Link,
    string? ad_Redirect_Link,
    long    account_Owner_Id,
    string  account_Owner_Username
);