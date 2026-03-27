public class PersonAccount : BaseAccount
{
    public DateOnly Birthday {get; set;}
    public string? InformalWork {get; set;} = string.Empty;

    public PersonAccount(   DateOnly Birthday, 
                            string Username, string Hashed_password, string Email, int District_id) 
                            : base( Username,  Hashed_password,  Email,  District_id)
    {
        this.Birthday = Birthday;
    }
}