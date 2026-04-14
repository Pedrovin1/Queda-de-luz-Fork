public class BusinessAccount : BaseAccount
{
    public string CNPJ;

    //to insert into the Database
    public BusinessAccount( string Cnpj, 
                            string Username, string Hashed_password, string Email, int District_id) 
                            : base( Username,  Hashed_password,  Email,  District_id)
    {
        this.CNPJ = Cnpj;
    }


    //to recover from the Database
    public BusinessAccount( string Cnpj, 
                            string Username, string Email, long UTC_datetime_creation, long Advertisement_slots_amount, long District_id) 
                            : base( Username,  Email,  UTC_datetime_creation,  Advertisement_slots_amount, District_id)
    {
        this.CNPJ = Cnpj;
    }

    //to recover from the Database
    //Get Account Data endpoint
    public BusinessAccount( string Cnpj, 
                            string Username, string Email, string? Description, long Advertisement_slots_amount, long District_id, string? Profile_picture_link) 
                            : base(Username, Email, Description, Advertisement_slots_amount, District_id, Profile_picture_link)
    {
        this.CNPJ = Cnpj;
    }
}