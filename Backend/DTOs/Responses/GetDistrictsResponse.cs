public record GetDistrictsResponse(
    int City_Id,
    Dictionary<int, string> Districts // int districtID --> string districtName
);