public  class AccountSignInOutValidator
{
    private IBlackoutMapConnectionFactory _connectionFactory;

    public AccountSignInOutValidator(IBlackoutMapConnectionFactory connectionFactory)
    {
        this._connectionFactory = connectionFactory;
    }

    public bool IsValid(PostAccountRequest request)
    {
        if(request.Person_Details is not null && request.Business_Details is not null ||
           request.Person_Details is null     && request.Business_Details is null)
        {
            return false;
        }

        //<<TODO: validate Email>>
        //<<TODO: validate District Id Existence>>
        //<<TODO: validate duplicate Username>>

        return true;
    }
}