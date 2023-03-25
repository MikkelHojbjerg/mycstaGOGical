#r "nuget: Newtonsoft.Json, 13.0.3"
open System.Diagnostics
open Newtonsoft.Json
open System
open System.IO
open FSharp
open FSharp.Data

let areasTimezones = Map [
    "Eagle River", "China Standard Time"
    "Kincaid Park", "China Standard Time"
    "Far North Bicentennial Park", "China Standard Time"
    "Bear Valley", "China Standard Time"
    "Fire Island", "China Standard Time"
]

let urlA = "https://incommodities.io/a?area=" //Husk og tilføje "+ "var navn" for area (of den repræsentatne forecast) hvor man gerne vil finde info. se https://www.youtube.com/watch?v=WZNG8UomjSI&ab_channel=JonahLawrence%E2%80%A2DevProTips"
let urlB = "https://incommodities.io/b/"
let token = "6b0fb5dad1564780a6bb83a5491e9bc5"

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
type ForecastData = {
    time: string
    temperature: float
    humidity: float
    wind: float
    pressure: float
}
type Forecast = {
    area: string
    forecast: ForecastData[]
}

let toJson (response: string, area: string) =
    let newLineSplit = Seq.toList(response.Split "\n")
    let mutable forecasts = Array.empty
    for i in 1.. newLineSplit.Length-1 do
        let dataSplit = newLineSplit.[i].Split ","

        let localTime = dataSplit.[0]
        let temperature = dataSplit.[2]
        let humidity = dataSplit.[3]
        let wind = dataSplit.[4]
        let pressure = dataSplit.[5]
        let somecastData = {
            time = convTime.toUtc (localTime, areasTimezones.[area])
            temperature = float temperature
            humidity = float humidity
            wind = float wind
            pressure = float pressure
        } 
        forecasts <- Array.append forecasts [| somecastData |]

    let forecast = { 
        area = area
        forecast = forecasts
    }

    JsonConvert.SerializeObject(forecast)


let fetchFromA (area: string) =
    let url = urlA + (area.Replace (" ", "%20"))
    let psi = new ProcessStartInfo("curl", sprintf "\"%s\" -X POST -H \"Authorization: Bearer %s\"" url token)
    psi.RedirectStandardOutput <- true
    let proces = Process.Start(psi)
    proces.StandardOutput.ReadToEnd()

let response = fetchFromA "Eagle River"
printfn "%s\n____________\n%s" response (toJson (response, "Eagle River"))



// module requestsData =
    
    //Mulig læsestof til http post
    //https://github.com/fsprojects/FSharp.Data/issues/1392

//Read CSV in F#
//https://www.youtube.com/watch?v=wBCbdWHzfng&ab_channel=HAMYLABS