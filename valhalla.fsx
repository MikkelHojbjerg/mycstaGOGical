#r "nuget: FSharp.Data"
open System.Diagnostics
open System.Text.Json
open System
open System.IO
open FSharp.Data

//Heimdall (God of Light and Protection) process the data used for the API
module heimdall =

    //Defines areas
    let areas = [|
        "Eagle River"
        "Kincaid Park"
        "Far North Bicentennial Park"
        "Bear Valley"
        "Fire Island"
    |]

    //Url and token for API
    let url_A = "https://incommodities.io/a?area=" //Husk og tilføje "+ "var navn" for area (of den repræsentatne forecast) hvor man gerne vil finde info. se https://www.youtube.com/watch?v=WZNG8UomjSI&ab_channel=JonahLawrence%E2%80%A2DevProTips"
    let url_B = "https://incommodities.io/b"
    let token = "6b0fb5dad1564780a6bb83a5491e9bc5"

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
   
    //Defines time zone
    let anchorageTimeZone = "Alaskan Standard Time"
//Heimdal stops his watch


//Urd (God of time and fate) coverts the Anchorage local time to Utc time
module urd = 
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


//Kvasir (God of knowledge) takes care of the language barrier with his knowledge and translate the data from a to Json
module kvasir =
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
                heimdall.time = urd.toUtc (localTime, heimdall.anchorageTimeZone)
                heimdall.temperature = float temperature
                heimdall.humidity = float humidity
                heimdall.wind = float wind
                heimdall.pressure = float pressure
            } 
            forecasts <- Array.append forecasts [| somecastData |]

        let forecast = { 
            heimdall.area = area
            heimdall.forecast = forecasts
        }

        JsonSerializer.Serialize(forecast)
