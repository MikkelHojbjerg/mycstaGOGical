open System;

let toUtc (localTimeStamp: string, localTimeZone: string) =
    let dLocal = DateTime.SpecifyKind (
        DateTime.Parse (localTimeStamp, null, System.Globalization.DateTimeStyles.RoundtripKind), 
        System.DateTimeKind.Unspecified
    )
    let dUtc = System.TimeZoneInfo.ConvertTimeToUtc (
        dLocal, 
        System.TimeZoneInfo.FindSystemTimeZoneById localTimeZone
    )
    dUtc.ToString "yyyy-MM-ddTHH:mm:ssZ"


// Example:
// printfn "%s" (toUtc ("2023-03-24T11:25:01", "China Standard Time"))