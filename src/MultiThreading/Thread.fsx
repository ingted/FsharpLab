open System
open System.Threading

let random = new Random()
let threadQuantity = 100
let threadAttemptQuantity = 10000000L
let evtWaitHandle = []


let start = Environment.TickCount

ThreadPool.SetMaxThreads(30, 100)

let countPI attemptQuantity =
    let r = new Random(random.Next() &&& DateTime.Now.Millisecond)
    let mutable x = 0.0
    let mutable y = 0.0
    let mutable target = 0L
    for _i = 0 to attemptQuantity do
        x <- r.NextDouble()
        y <- r.NextDouble()
        if (x * x + y * y) < 1.0 then
            target <- target + 1L
        
    target

let runPi(param: obj) =
    try
        let index = param :?> System.Nullable<int>
        printfn "Start thread nr. %d, index: %A" Thread.CurrentThread.ManagedThreadId index
    with
    | :? ThreadAbortException as ex ->
        printfn "Abort error %s" ex.Message
    | exn -> 
        printfn "Ex %s" exn.Message