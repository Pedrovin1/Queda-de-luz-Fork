public class PersonAccount : BaseAccount
{
    public DateOnly Birthday {get; set;}
    public string? InformalWork {get; set;} = string.Empty;

    //to insert into the database
    public PersonAccount(   DateOnly Birthday, 
                            string Username, string Hashed_password, string Email, int District_id) 
                            : base( Username,  Hashed_password,  Email,  District_id)
    {
        this.Birthday = Birthday;
    }

    //to recover from the database
    public PersonAccount(   string Birthday, 
                            string Username, string Email, long UTC_datetime_creation, long Advertisement_slots_amount, long District_id) 
                            : base( Username,  Email,  UTC_datetime_creation,  Advertisement_slots_amount, District_id)
    {
        this.Birthday = DateOnly.Parse(Birthday);
    }
}