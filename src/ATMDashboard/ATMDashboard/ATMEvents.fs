namespace ATMDashboard

open System
open System.Text
open Newtonsoft.Json
open System.Diagnostics
open System.Threading.Tasks
open System.Collections.Generic
open Microsoft.ServiceBus.Messaging

type ATMEvent = {
    CardNumber:string;
    ATM1:string;ATM2:string;
    TransactionTime1:string;
    TransactionTime2:string} 

type ATMEventAggregator() =
    let events = new List<ATMEvent>()
    member this.LogEvent(event) =
        match events.Count < 1024 with
        | true -> events.Add(event)
        | false -> ()
    member this.GetLoggedEvents() =
        let eventArray = events.ToArray()
        events.Clear()
        eventArray

type SimpleEventProcessor(aggregator:ATMEventAggregator) =
    let checkpointStopWatch = new Stopwatch()
    interface IEventProcessor with
        member this.CloseAsync(context, reason) = 
            Debug.WriteLine("Processor Shutting Down. Partition '{0}', Reason: '{1}'.", context.Lease.PartitionId, reason)
            match reason with 
            | CloseReason.Shutdown -> context.CheckpointAsync() 
            | _ -> Task.CompletedTask   
        member this.OpenAsync(context) =
            Debug.WriteLine("SimpleEventProcessor initialized.  Partition: '{0}', Offset: '{1}'", context.Lease.PartitionId, context.LeaseOffset)
            checkpointStopWatch.Start()
            Task.CompletedTask
        member this.ProcessEventsAsync(context, messages) =
            messages
            |> Seq.map(fun ed -> Encoding.UTF8.GetString(ed.GetBytes()))
            |> Seq.map(fun b -> Debug.WriteLine(String.Format("Message received.  Partition: '{0}', Data: '{1}'", context.Lease.PartitionId, data)); b)
            |> Seq.map(fun b -> JsonConvert.DeserializeObject<ATMEvent>(b))
            |> Seq.iter(fun e -> aggregator.LogEvent(e))
            
            match checkpointStopWatch.Elapsed > TimeSpan.FromMinutes(5.0) with
            | true -> checkpointStopWatch.Restart()
                      context.CheckpointAsync()
            | false -> Task.CompletedTask                 