public class BusinessAccount : BaseAccount
{
    public string CNPJ;

    public BusinessAccount( string Cnpj, 
                            string Username, string Hashed_password, string Email, int District_id) 
                            : base( Username,  Hashed_password,  Email,  District_id)
    {
        this.CNPJ = Cnpj;
    }
}