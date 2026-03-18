public record GetDistrictsResponse(
    int City_id,
    Dictionary<int, string> Districts // int districtID --> string districtName
);