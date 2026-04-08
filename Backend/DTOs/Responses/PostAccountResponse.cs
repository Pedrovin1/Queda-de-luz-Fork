public record PostAccountResponse(
    string Username, 
    string Email, 
    long utc_Datetime_Creation, 
    int Advertisement_Slots_Amount, 
    int District_Id,
    PersonAccountDetails? Person_Details = null,
    BusinessAccountDetails? Business_Details = null 
);

//<< PostAccountRequest.cs >>

// public record PersonAccountDetails(
//     DateOnly Birthday // "YYYY-MM-DD"
// );

// public record BusinessAccountDetails(
//     string CNPJ
// );