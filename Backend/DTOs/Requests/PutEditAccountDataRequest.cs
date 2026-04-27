public record PutEditAccountDataRequest(
    PutPersonDataSchema? person_Data,
    PutBusinessDataSchema? business_Data
);

public record PutPersonDataSchema(
    string  username,
    string  email,
    string? description,
    int     district_Id,

    string? informal_Work
);

public record PutBusinessDataSchema(
    string  username,
    string  email,
    string? description,
    int     district_Id
);