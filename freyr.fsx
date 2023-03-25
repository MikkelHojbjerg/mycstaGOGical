#r "nuget: Newtonsoft.Json, 13.0.3"
#r "nuget: FSharp.Data"
open System.Diagnostics
open Newtonsoft.Json
open System
open System.IO
open FSharp
open FSharp.Data

let areas = [
    "Eagle River"
    "Kincaid Park"
    "Far North Bicentennial Park"
    "Bear Valley"
    "Fire Island"
]

let anchorageTimeZone = "Alaskan Standard Time"

let urlA = "https://incommodities.io/a?area=" //Husk og tilføje "+ "var navn" for area (of den repræsentatne forecast) hvor man gerne vil finde info. se https://www.youtube.com/watch?v=WZNG8UomjSI&ab_channel=JonahLawrence%E2%80%A2DevProTips"
let urlB = "https://incommodities.io/b"
let token = "6b0fb5dad1564780a6bb83a5491e9bc5"

module convTime = 
    let parse (localTimeStamp: string) =
        DateTime.Parse (localTimeStamp, null, System.Globalization.DateTimeStyles.RoundtripKind)

    let localToUtc (localTimeStamp: DateTime, localTimeZone: string) =
        let dLocal = DateTime.SpecifyKind (
            localTimeStamp, 
            System.DateTimeKind.Unspecified
        )
        System.TimeZoneInfo.ConvertTimeToUtc (
            dLocal, 
            System.TimeZoneInfo.FindSystemTimeZoneById localTimeZone
        )

    let toIsoString (timeStamp: DateTime) =
        timeStamp.ToString "yyyy-MM-ddTHH:mm:ssZ"


// Type containing forecast info
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
    if newLineSplit.Length = 1 then 
        raise (System.Exception "Funny reply")

    let mutable forecasts = Array.empty
    let mutable updated = ""
    for i in 1.. newLineSplit.Length-1 do
        let dataSplit = newLineSplit.[i].Split ","

        let localTime = dataSplit.[0]
        updated <- dataSplit.[1]
        let temperature = dataSplit.[2]
        let humidity = dataSplit.[3]
        let wind = dataSplit.[4]
        let pressure = dataSplit.[5]

        let originalDateTime = convTime.parse localTime
        let utcDateTime = convTime.localToUtc (originalDateTime, anchorageTimeZone)
        let utcTime = convTime.toIsoString utcDateTime

        let somecastData = {
            time = utcTime
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

    (JsonConvert.SerializeObject(forecast), updated)


let fetchFromA (area: string) =
    let url = urlA + (area.Replace (" ", "%20"))
    let response = Http.RequestString($"{url}", httpMethod = "POST", headers = [ "Authorization", $"Bearer {token}" ])
    printfn $"Response from A: {response}"
    response


let sendToB (data: string) =
    let response = Http.RequestString($"{urlB}", httpMethod = "POST", body = TextRequest data, headers = [ "Authorization", $"Bearer {token}" ])
    printfn $"Response from B: {response}"


let run (area: string) =
    async {
        let mutable lastUpadated = ""
        while true do
            printfn "%A" DateTime.Now
            try
                let response = fetchFromA area
                try 
                    let (json, updated) = (toJson (response, area))
                    let changed = updated <> lastUpadated

                    if changed then
                        sendToB json
                        lastUpadated <- updated
                        printfn "changed"
                        
                        printfn $"updated: {updated}"
                        let updatedDateTime = convTime.localToUtc ((convTime.parse updated), anchorageTimeZone)
                        let now = DateTime.UtcNow 
                        let delay = Convert.ToInt32 (System.Math.Floor (now.Subtract(updatedDateTime).TotalMilliseconds))
                        let twentyNineMins = 29 * 60 * 1000
                        let wait = twentyNineMins - delay
                        printfn $"Waiting {wait} milliseconds"
                        do! Async.Sleep (wait)
                    else
                        printfn "not changed"
                        do! Async.Sleep (10000)
                with
                    | ex -> 
                        printfn $"Failed: {ex}"
                
            with
                | ex ->
                    printfn $"Failed contacting A - Trying again in 2 minutes. \n{ex}"
                    do! Async.Sleep (2 * 60 * 1000)
    }

areas 
    |> List.map run 
    |> Async.Parallel 
    |> Async.RunSynchronously

