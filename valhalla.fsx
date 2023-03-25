#r "nuget: FSharp.Data"
open System.Diagnostics
open System.Text.Json
open System
open System.IO
open FSharp.Data

//Heimdall (God of Light and Protection) process the data used for the API
module heimdall =

    //Defines areas
    let areas = [
        "Eagle River"
        "Kincaid Park"
        "Far North Bicentennial Park"
        "Bear Valley"
        "Fire Island"
    ]

    //Url and token for API
    let urlA = "https://incommodities.io/a?area=" //Husk og tilføje "+ "var navn" for area (of den repræsentatne forecast) hvor man gerne vil finde info. se https://www.youtube.com/watch?v=WZNG8UomjSI&ab_channel=JonahLawrence%E2%80%A2DevProTips"
    let urlB = "https://incommodities.io/b"
    let token = "6b0fb5dad1564780a6bb83a5491e9bc5"
   
    //Defines time zone
    let anchorageTimeZone = "Alaskan Standard Time"
//Heimdal stops his watch


//Urd (God of time and fate) coverts the Anchorage local time to Utc time
module urd = 
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


//Kvasir (God of knowledge) takes care of the language barrier with his knowledge and translate the data from a to Json
module kvasir =

    //type contanning forecast info
    type ForecastData = {
        time: string
        temperature: float
        humidity: float
        wind: float
        pressure: float
    }

    //type contanning loaction info
    type Forecast = {
       area: string
       forecast: ForecastData[]
    }

    let toJson (response: string, area: string) =
        let newLineSplit = Seq.toList(response.Split "\n")
        if newLineSplit.Length = 1 then 
            raise (System.Exception "Funny reply")

        let mutable forecasts = Array.empty
        let updated = (newLineSplit.[1].Split ",").[1]
        for i in 1.. newLineSplit.Length-1 do
            let dataSplit = newLineSplit.[i].Split ","

            let localTime = dataSplit.[0]
            let temperature = dataSplit.[2]
            let humidity = dataSplit.[3]
            let wind = dataSplit.[4]
            let pressure = dataSplit.[5]

            let originalDateTime = urd.parse localTime
            let utcDateTime = urd.localToUtc (originalDateTime, heimdall.anchorageTimeZone)
            let utcTime = urd.toIsoString utcDateTime

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


module mimir =
    let run (area: string, f: string -> bool * string) =
        async {
            let mutable attemptCount = 0
            while true do
                printfn "%A" DateTime.Now
                try
                    attemptCount <- 0

                    let (changed, updated) = f area     
                    if changed then
                        printfn $"updated: {updated}"
                        let updatedDateTime = urd.localToUtc ((urd.parse updated), heimdall.anchorageTimeZone)
                        let now = DateTime.UtcNow 
                        let delay = Convert.ToInt32 (System.Math.Floor (now.Subtract(updatedDateTime).TotalMilliseconds))
                        let expectedInterval = 60 * 1000 - 200
                        let wait = expectedInterval - delay
                        printfn $"Waiting {wait} milliseconds"
                        do! Async.Sleep (wait)
                    else
                        printfn "not changed"
                        attemptCount <- attemptCount + 1
                        if attemptCount <= 20 then
                            printfn "Cooling down"
                            do! Async.Sleep (1000)
                with
                    | ex -> 
                        printfn $"Failed: {ex}"
        }

    let runAll (areas: List<string>, f: string -> bool * string) = 
        areas 
        |> List.map (fun area -> run (area, f)) 
        |> Async.Parallel 
        |> Async.RunSynchronously
