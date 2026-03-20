public record GetCitiesResponse(
    string State_Abbreviation,
    List<GetCitiesResponse_City> Cities
    //List< (int City_Id, string City_Name) > Cities
);

public record GetCitiesResponse_City (
    int City_Id, 
    string City_Name
);
