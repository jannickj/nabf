namespace NabfAgentLogic
    open NabfAgentLogic.AgentInterfaces;
    open JSLibrary.Data.GenericEvents;
    open JSLibrary.IiLang;
    open JSLibrary;
    open JSLibrary.IiLang.DataContainers;
    open System;
    open NabfAgentLogic.AgentLogic;
    open System.Threading;
    open System.Linq;
    open AgentTypes

    type public AgentLogicClient(decisionTreeGenerator) = class 
        
        
        [<DefaultValue>] val mutable private BeliefData : State
        [<DefaultValue>] val mutable private KnownJobs : List<Job>
        //[<DefaultValue>] val mutable private DesiredJobs : List<Job*Desirability>
        //[<DefaultValue>] val mutable private possibleActions : List<Action>
        //[<DefaultValue>] val mutable private undecidedDecisions : (DecisionRank*Decision<(State -> (bool*Option<Action>))>) list
        [<DefaultValue>] val mutable private awaitingPercepts : List<Percept>

        let decisionTree = decisionTreeGenerator()
        let mutable simEnded = false
        let mutable runningCalc = 0
        let mutable decisionId = 0
        //let mutable decidedAction = false
        let mutable decidedAction = (Int32.MaxValue,Action.Recharge)
        //let mutable lastHighestDesire:Desirability = 0
        

        new() = AgentLogicClient(fun () -> generateDecisionTree)

        //Parallel helpers
        let stopDeciders = new CancellationTokenSource()

        let stateLock = new Object()
        let decisionLock = new Object()
        let runningCalcLock = new Object()
        let knownJobsLock = new Object()
        let awaitingPerceptsLock = new Object()
        


        let SendAgentServerEvent = new Event<UnaryValueHandler<IilAction>, UnaryValueEvent<IilAction>>()
        let SendMarsServerEvent = new Event<UnaryValueHandler<IilAction>, UnaryValueEvent<IilAction>>()
        let EvaluationCompletedEvent = new Event<UnaryValueHandler<IilAction>, UnaryValueEvent<IilAction>>()
        let EvaluationStartedEvent = new Event<UnaryValueHandler<IilAction>, UnaryValueEvent<IilAction>>()
        let SimulationEndedEvent = new Event<EventHandler, EventArgs>()

        member public this.DecidedAction = decidedAction

        member private this.asyncCalculation stopToken func = this.asyncCalculationAF stopToken (async { func() })

        member private this.asyncCalculationAF stopToken func =
            let changeCals value = 
                lock runningCalcLock (fun () -> 
                if runningCalc = 0 then
                    EvaluationStartedEvent.Trigger(this, new UnaryValueEvent<IilAction>(buildEvaluationStarted))
                runningCalc <- runningCalc + value)
                if runningCalc = 0 then
                    EvaluationCompletedEvent.Trigger(this, new UnaryValueEvent<IilAction>(buildEvaluationEnded))
                    ()
            let asyncF f = 
                async
                    {
                        try
                            Async.RunSynchronously f
                        with _ -> ()   

                        changeCals(-1)
                        ()
                    }
            
            changeCals(1)
            Async.Start ((asyncF func), stopToken)
            () 

        member private this.EvaluateDecision    (rankCur:DecisionRank) 
                                                (stopToken:CancellationToken) 
                                                (stopSource:CancellationTokenSource) 
                                                (s:State,d:Decision<(State -> (bool*Option<Action>))>) =
            match d with
            | Choice f -> 
                
                this.asyncCalculationAF stopToken (
                    async
                        { 
                            
                            let dId = lock decisionLock (fun () -> decisionId)
                           
                            let cancelFunc() =
                                stopSource.Cancel()                            
                            use! handler = Async.OnCancel(cancelFunc)
                            let output = ref (false,Option.None)
                            let timer:float = 10.0
                            let success = Parallel.TryExecute<(bool*Option<Action>)>((fun () -> f s),timer,(fun () -> stopToken.IsCancellationRequested),output);
                            if success then
                                lock decisionLock (fun () ->
                                    if dId = decisionId then
                                        let (b,a) = output.Value
                                        let (cR,cA) = decidedAction
                                        if b && a.IsSome && cR > rankCur then
                                            decidedAction <- (rankCur,a.Value)
                                            stopSource.Cancel() 
                                    else
                                        stopSource.Cancel()
                                    )
                            else
                                stopSource.Cancel() 
                        }
                    )
            | Options ds -> 
                ignore <| List.fold ( fun (r,c) t ->
                                        let iteStopSource = new CancellationTokenSource()
                                        this.EvaluateDecision r c iteStopSource (s,t)
                                        (r+1,iteStopSource.Token)
                                        ) (rankCur,stopToken) ds
            | Condition (c,d) -> 
                this.asyncCalculationAF stopToken (
                            async
                                {
                                    use! handler = Async.OnCancel(fun () -> stopSource.Cancel())
                                    let (b,_) = c s
                                    let source = new CancellationTokenSource()
                                    this.EvaluateDecision rankCur stopSource.Token source (s,d)
                                })
                    




            

        member public this.EvaluteState() =
            let s = lock stateLock (fun () -> this.BeliefData)
            this.EvaluateDecision 0 stopDeciders.Token (new CancellationTokenSource()) (s,decisionTree)
            

        member private this.asyncCalculationMany func values stopToken =
            List.iter (fun v -> (this.asyncCalculation stopToken (fun () -> func v))) values

        member private this.ReEvaluate percepts =
            stopDeciders.Cancel()
            let sharedPercepts = lock awaitingPerceptsLock (fun () -> 
                                        let percepts = this.awaitingPercepts
                                        this.awaitingPercepts <- []
                                        percepts)
            lock stateLock (fun () -> this.BeliefData <- updateState this.BeliefData (percepts@sharedPercepts))
            runningCalc <- 0
            lock decisionLock ( fun () ->
                                    decidedAction <- (Int32.MaxValue,Action.Recharge)
                                    decisionId <- (decisionId + 1)
                                )
            this.EvaluteState()

        let stopLogic () =
            stopDeciders.Cancel()
            simEnded <- true
        
        member private this.CalculateAcceptedJob id =
            let foundJob = List.tryFind (fun ((jid,_,_),_) -> jid = id) this.KnownJobs
            if foundJob.IsSome then
                let update = 
                    async
                        {
                            let ajob = foundJob.Value
                            lock stateLock (fun () ->   this.BeliefData <- updateStateWhenGivenJob this.BeliefData ajob)
                            this.asyncCalculation stopDeciders.Token (fun () -> this.EvaluteState())
                            ()
                        }
                Async.Start update

            

        member private this.generateNewJobs() = 
            let jobTypes = Enum.GetValues(typeof<JobType>)
            let rec jobGenFunc jobtype state knownJobs =
                let jobopt = generateJob jobtype state knownJobs
                match jobopt with
                | Some job ->
                    SendAgentServerEvent.Trigger (this, new UnaryValueEvent<IilAction>(buildJob job)) 
                    jobGenFunc jobtype state (job::knownJobs)                            
                | None -> ()

            let jobTypeList = List.ofSeq (jobTypes.Cast<JobType>())
            let knownjobs jobtype =List.filter (fun ((_, _, jt), _) -> jt = jobtype) this.KnownJobs
            let stateData = lock stateLock (fun () -> this.BeliefData)
            this.asyncCalculationMany (fun jobType -> jobGenFunc jobType stateData (knownjobs jobType) ) jobTypeList stopDeciders.Token

        member private this.evaluateJobs jobs =
            let update job = 
                let stateData = lock stateLock (fun () -> this.BeliefData)
                let (desire,wantJob) = decideJob stateData job
                if wantJob then
                    SendAgentServerEvent.Trigger (this, new UnaryValueEvent<IilAction>(buildJobAccept (desire,job)))  
            lock knownJobsLock (fun () -> this.KnownJobs <- jobs@this.KnownJobs)
            this.asyncCalculationMany update jobs stopDeciders.Token


        interface IAgentLogic with
            member this.Close() = 
                stopLogic()
                ()
            member this.CurrentDecision = 
                let (_,bestAction) = lock decisionLock (fun () -> decidedAction)
                buildIilAction bestAction

            member this.HandlePercepts(iilpercepts) = 
                if simEnded then
                    ()
                let ServerMessage = (parseIilPercepts iilpercepts)
                match ServerMessage with
                | AgentServerMsg msg ->
                    match msg with
                    | NewJobs jobs ->
                        this.evaluateJobs jobs
                    | AcceptedJob id ->
                        this.CalculateAcceptedJob id  
                    | SharedPercepts percepts ->
                        ignore <| lock awaitingPerceptsLock (fun () -> this.awaitingPercepts <- percepts@this.awaitingPercepts)
                |  MarsServerMsg msg ->
                    match msg with
                    | SimulationEnd _ -> 
                        SimulationEndedEvent.Trigger(this, new EventArgs())
                        stopLogic()
                        ()
                    | SimulationStart ->
                        ()
                    | ActionRequest (deadline,actionTime,id, percepts) ->
                        let action = buildSharePerceptsAction (sharedPercepts percepts)
                        SendAgentServerEvent.Trigger(this, new UnaryValueEvent<IilAction>(action))
                        this.ReEvaluate percepts
                        let knownJobs = lock knownJobsLock (fun () -> this.KnownJobs)
                        this.evaluateJobs knownJobs
                        this.generateNewJobs()
                        let totalTime = deadline - actionTime
                        let forceDecision start totaltime =
                            async
                                {
                                    let awaitingDecision = ref true
                                    do! Async.Sleep(800)
                                    while awaitingDecision.Value do 
                                        do! Async.Sleep(200)
                                        let expired = (System.DateTime.Now.Ticks - start)/(int64(10000))
                                        let runningCalcs = lock runningCalcLock (fun () -> runningCalc)
                                        if (expired+int64(400)) > int64(totaltime) || runningCalcs = 0 then
                                            SendAgentServerEvent.Trigger(this,new UnaryValueEvent<IilAction>((this:>IAgentLogic).CurrentDecision))
                                            awaitingDecision:=false
                                }
                        Async.Start (forceDecision System.DateTime.Now.Ticks totalTime)
                        ()
                    | ServerClosed -> ()
           
            

            [<CLIEvent>]
            member this.SendAgentServerAction = SendAgentServerEvent.Publish
            [<CLIEvent>]
            member this.SendMarsServerAction = SendMarsServerEvent.Publish

            [<CLIEvent>]
            member this.EvaluationCompleted = EvaluationCompletedEvent.Publish
            [<CLIEvent>]
            member this.EvaluationStarted = EvaluationStartedEvent.Publish
            [<CLIEvent>]
            member this.SimulationEnded  = SimulationEndedEvent.Publish
    end