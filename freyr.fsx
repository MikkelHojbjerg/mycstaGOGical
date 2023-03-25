#r "nuget: Newtonsoft.Json, 13.0.3"
#r "nuget: FSharp.Data"
open System.Diagnostics
open Newtonsoft.Json
open System
open System.IO
open FSharp
open FSharp.Data

let areas = [|
    "Eagle River"
    "Kincaid Park"
    "Far North Bicentennial Park"
    "Bear Valley"
    "Fire Island"
|]

let anchorageTimeZone = "Alaskan Standard Time"

let urlA = "https://incommodities.io/a?area=" //Husk og tilføje "+ "var navn" for area (of den repræsentatne forecast) hvor man gerne vil finde info. se https://www.youtube.com/watch?v=WZNG8UomjSI&ab_channel=JonahLawrence%E2%80%A2DevProTips"
let urlB = "https://incommodities.io/b"
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
            time = convTime.toUtc (localTime, anchorageTimeZone)
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
    Http.RequestString($"{url}", httpMethod = "POST", headers = [ "Authorization", $"Bearer {token}" ])

let sendToB (data: string) =
    let response = Http.RequestString($"{urlB}", httpMethod = "POST", body = TextRequest data, headers = [ "Authorization", $"Bearer {token}" ])
    printfn $"Response from B: {response}"


let mutable lastJson = ""
let run () =
    for area in areas do
        let response = fetchFromA area
        let json = (toJson (response, area))
        if json <> lastJson then
            sendToB json
            lastJson <- json
            printfn "changed"
            printfn "%s\n\n%s" json lastJson
        else
            printfn "not changed"



let rec attempt () = 
    try
        printfn "%A" DateTime.Now
        run ()
    with
        | ex -> attempt ()


let timer = new Timers.Timer(5000.)
let event = Async.AwaitEvent (timer.Elapsed) |> Async.Ignore
timer.Start()
while true do
    attempt ()
    Async.RunSynchronously event


// for z in TimeZoneInfo.GetSystemTimeZones() do
//     printfn "%s" z.Id




// module requestsData =
    
    //Mulig læsestof til http post
    //https://github.com/fsprojects/FSharp.Data/issues/1392

//Read CSV in F#
//https://www.youtube.com/watch?v=wBCbdWHzfng&ab_channel=HAMYLABS