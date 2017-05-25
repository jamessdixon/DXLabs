open System
open System.Text
open Newtonsoft.Json
open System.Threading
open System.Runtime.Serialization
open Microsoft.ServiceBus.Messaging

type Transaction = {TransactionId: int; TransactionTime: String; DeviceId: int; CardNumber:int; Amount:float}

[<EntryPoint>]
let main argv = 
    let probability = 0.01
    let connectionString = @"Endpoint=sb://chickensoftwarestreamanalytics.servicebus.windows.net/;SharedAccessKeyName=SendPolicy;SharedAccessKey=G7U3+lWZ6p3ajhwMT5oWGAVih4BwN6z7YL02eXglppU=;EntityPath=inputhub"
    let random = new Random()
    let client = EventHubClient.CreateFromConnectionString(connectionString)

    let generateTransaction transactionid =
        let transactionTime = DateTime.UtcNow.ToString()
        let deviceId = 12345 + random.Next(0, 88888)
        let testCardNumber = 123456789 + random.Next(0,888888888)
        let cardNumber =
            match(random.NextDouble() < probability) with
            | true -> -1
            | false -> testCardNumber
        let amount = random.Next(1, 20) * 20 |> float
        {TransactionId=transactionid;TransactionTime=transactionTime;DeviceId=deviceId;CardNumber=cardNumber;Amount=amount}   

    [|0..100000|]
    |> Array.map(fun i -> Console.WriteLine("[{0}] Event transmitted",i); i)
    |> Array.map(fun i -> generateTransaction i)
    |> Array.map(fun t -> JsonConvert.SerializeObject(t))
    |> Array.map(fun st -> Encoding.UTF8.GetBytes(st))
    |> Array.map(fun b -> new EventData(b))
    |> Array.iter(fun ed -> client.Send(ed))
    0 
