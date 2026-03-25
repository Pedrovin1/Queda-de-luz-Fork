public class BaseAccount{
    public int  Id {get; set;}
    public string Username {get; set;}
    public string HashedPassword {get; set;}
    public string Email {get; set;}
    public string? Description {get; set;} = string.Empty;
    public string? ProfilePictureLink {get; set;} = null;

    public long  UTC_DatetimeCreation {get; set;}
    public int  AdvertisementSlotsAmount {get; set;} = 1;

    public int DistrictId {get; set;}

    //to insert new accounts into the database
    public BaseAccount(string Username, string Hashed_password, string Email, int District_id)
    {
        this.Username = Username;
        this.HashedPassword = Hashed_password;
        this.Email = Email;
        this.DistrictId = District_id;
    }
}

