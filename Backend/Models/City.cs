public class City
{
    public int Id {get; private set;} = -1 ;
    public string Name {get; private set;} = "unnamed";
    public string StateAbbreviation {get; private set;} = "unnamed";

    //public City(){}

    public City(long City_id, string City_Name, string State_Abbreviation)
    {
        this.Id = (int)City_id;
        this.Name = City_Name;
        this.StateAbbreviation = State_Abbreviation;
    }

    public City(string City_Name, string State_Abbreviation)
    {
        this.Name = City_Name;
        this.StateAbbreviation = State_Abbreviation;
    }

    public City(long City_id, string City_Name)
    {
        this.Id = (int)City_id;
        this.Name = City_Name;
    }
}