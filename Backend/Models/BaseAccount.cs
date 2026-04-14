using System.Runtime.Serialization;

public class BaseAccount{
    public int  Id {get; set;}
    public string Username {get; set;}
    public string HashedPassword {get; set;} = string.Empty;
    public string Email {get; set;} = string.Empty;
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

    //to recover from the database
    public BaseAccount(string Username, string Email, long UTC_datetime_creation, long Advertisement_slots_amount, long District_id)
    {
        this.Username = Username;
        this.Email = Email;
        this.UTC_DatetimeCreation = UTC_datetime_creation;
        this.AdvertisementSlotsAmount = (int)Advertisement_slots_amount;
        this.DistrictId = (int)District_id;
    }

    //to recover from the database
    public BaseAccount(long Base_Account_id, string Username, string? Profile_picture_link)
    {
        this.Id = (int)Base_Account_id;
        this.Username = Username;
        this.ProfilePictureLink = Profile_picture_link;
    }

    public BaseAccount(string Username, string Email, string? Description, long Advertisement_slots_amount, long District_id, string? Profile_picture_link)
    {
        this.Username = Username; 
        this.Email = Email; 
        this.Description = Description ?? string.Empty; 
        this.AdvertisementSlotsAmount = (int)Advertisement_slots_amount; 
        this.DistrictId = (int)District_id; 
        this.ProfilePictureLink = Profile_picture_link;
    }
}

