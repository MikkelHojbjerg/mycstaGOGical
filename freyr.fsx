open System
open System.IO
open FSharp
open FSharp.Data


let urlA = "https://incommodities.io/a/" //Husk og tilføje "+ "var navn" for area (of den repræsentatne forecast) hvor man gerne vil finde info. se https://www.youtube.com/watch?v=WZNG8UomjSI&ab_channel=JonahLawrence%E2%80%A2DevProTips"
let urlB = "https://incommodities.io/b/"
let token = "Bearer 6b0fb5dad1564780a6bb83a5491e9bc5"

module convTime = 
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


//type contanning forecast info
type forecast = {
    time : string;
    temp : float;
    hum : float;
    wind : float;
    pressure : float;
}



module requestsData =
    
    //Mulig læsestof til http post
    //https://github.com/fsprojects/FSharp.Data/issues/1392

//Read CSV in F#
//https://www.youtube.com/watch?v=wBCbdWHzfng&ab_channel=HAMYLABS