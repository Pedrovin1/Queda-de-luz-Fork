public record PostAccountRequest(
    string Username,
    string Unhashed_Password,
    string Email,
    int District_Id,
    PersonAccountDetails? Person_Details = null,
    BusinessAccountDetails? Business_Details = null
);

public record PersonAccountDetails(
    DateOnly Birthday // "YYYY-MM-DD"
);

public record BusinessAccountDetails(
    string CNPJ
);