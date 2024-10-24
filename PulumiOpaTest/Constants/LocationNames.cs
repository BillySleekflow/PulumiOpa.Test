namespace Sleekflow.Infras.Constants;

public static class LocationNames
{
    public const string EastUs = "eastus";
    public const string EastAsia = "eastasia";
    public const string UaeNorth = "uaenorth";
    public const string SouthEastAsia = "southeastasia";

    public static string GetShortName(string locationName)
    {
        return locationName switch
        {
            EastUs => "eus",
            EastAsia => "eas",
            UaeNorth => "uaen",
            SouthEastAsia => "seas",
            _ => throw new Exception("LocationNames - GetShortName")
        };
    }

    public static string GetAzureLocation(string locationName)
    {
        return locationName switch
        {
            EastUs => "eastus",
            EastAsia => "eastasia",
            UaeNorth => "uaenorth",
            SouthEastAsia => "southeastasia",
            _ => throw new Exception("LocationNames - GetAzureLocation")
        };
    }
}