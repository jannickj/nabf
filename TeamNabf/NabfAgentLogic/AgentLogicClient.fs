namespace NabfAgentLogic
    open NabfAgentLogic.AgentInterfaces;
    open JSLibrary.Data.GenericEvents;
    open JSLibrary.IiLang;
    open JSLibrary.IiLang.DataContainers;
    open System;
    open NabfAgentLogic.AgentLogic;
    open System.Threading;
    open System.Linq;
    open AgentTypes


    type public AgentLogicClient() = class 
        
        
        [<DefaultValue>] val mutable private BeliefData : State
        [<DefaultValue>] val mutable private KnownJobs : List<Job>
        //[<DefaultValue>] val mutable private DesiredJobs : List<Job*Desirability>
        //[<DefaultValue>] val mutable private possibleActions : List<Action>
        [<DefaultValue>] val mutable private decidedAction : (DecisionRank*Action)
        //[<DefaultValue>] val mutable private undecidedDecisions : (DecisionRank*Decision<(State -> (bool*Option<Action>))>) list
        [<DefaultValue>] val mutable private awaitingPercepts : List<Percept>
        
        let decisionTree = generateDecisionTree
        let mutable simEnded = false
        let mutable runningCalc = 0
        //let mutable lastHighestDesire:Desirability = 0
        

        //Parallel helpers
        let stopDeciders = new CancellationTokenSource()

        let stateLock = new Object()
        let decisionLock = new Object()
        let actionDeciderLock = new Object()
        let runningCalcLock = new Object()
        let knonwJobsLock = new Object()
        let awaitingPerceptsLock = new Object()
        


        let SendAgentServerEvent = new Event<UnaryValueHandler<IilAction>, UnaryValueEvent<IilAction>>()
        let SendMarsServerEvent = new Event<UnaryValueHandler<IilAction>, UnaryValueEvent<IilAction>>()

        let JobCreatedEvent = new Event<UnaryValueHandler<IilAction>, UnaryValueEvent<IilAction>>()
        let JobDesiredEvent = new Event<UnaryValueHandler<IilAction>, UnaryValueEvent<IilAction>>()
        let EvaluationCompletedEvent = new Event<UnaryValueHandler<IilAction>, UnaryValueEvent<IilAction>>()
        let EvaluationStartedEvent = new Event<UnaryValueHandler<IilAction>, UnaryValueEvent<IilAction>>()
        let ActionRequestedEvent = new Event<UnaryValueHandler<IilAction>, UnaryValueEvent<IilAction>>()
        let SimulationEndedEvent = new Event<EventHandler, EventArgs>()

        

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
                        Async.RunSynchronously f
                        do! Async.Sleep(1)
                        changeCals(-1)
                        ()
                    }
            
            changeCals(1)
            Async.Start ((asyncF func), stopToken)
            () 

        member private this.EvaluateDecision (rankCur:DecisionRank) (stopToken:CancellationToken) (stopSource:CancellationTokenSource) (s:State,d:Decision<(State -> (bool*Option<Action>))>) =
            match d with
            | Choice f -> 
                this.asyncCalculationAF stopToken (
                    async
                        { 
                            use! handler = Async.OnCancel(fun () -> stopSource.Cancel())
                            let (b,a) = f s
                            lock decisionLock (fun () ->
                                let (cR,cA) = this.decidedAction
                                if b && cR > rankCur && a.IsSome then
                                    this.decidedAction <- (rankCur,a.Value)
                                    stopSource.Cancel()
                                )
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
                    




            

        member private this.EvaluteState (s:State) =
            this.EvaluateDecision 0 stopDeciders.Token (new CancellationTokenSource()) (s,decisionTree)
            

        member private this.asyncCalculationMany func values stopToken =
            List.iter (fun v -> (this.asyncCalculation stopToken (fun () -> func v))) values

        member private this.ReEvaluate percepts =
            stopDeciders.Cancel()
            let sharedPercepts = lock awaitingPerceptsLock (fun () -> 
                                        let percepts = this.awaitingPercepts
                                        this.awaitingPercepts <- []
                                        percepts)
            let mutable a = 0

            let b = a <- 1
                    a

            let stateData = lock stateLock (fun () -> this.BeliefData <- updateState this.BeliefData (percepts@sharedPercepts)
                                                      this.BeliefData)
            runningCalc <- 0
            this.EvaluteState stateData

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
                            let stateData = lock stateLock (fun () ->   this.BeliefData <- updateStateWhenGivenJob this.BeliefData ajob 
                                                                        this.BeliefData)
                            this.EvaluteState stateData
                            ()
                        }
                ()
            

        member private this.generateNewJobs() = 
            let jobTypes = Enum.GetValues(typeof<JobType>)
            let rec jobGenFunc jobtype state knownJobs =
                let jobopt = generateJob jobtype state knownJobs
                match jobopt with
                | Some job ->
                    JobCreatedEvent.Trigger (this, new UnaryValueEvent<IilAction>(buildJob job)) 
                    jobGenFunc jobtype state (job::knownJobs)                            
                | None -> ()

            let jobTypeList = List.ofSeq (jobTypes.Cast<JobType>())
            let knownjobs jobtype =List.filter (fun ((_, _, jt), _) -> jt = jobtype) this.KnownJobs
            let stateData = lock stateLock (fun () -> this.BeliefData)
            this.asyncCalculationMany (fun jobType -> jobGenFunc jobType stateData (knownjobs jobType) ) jobTypeList stopDeciders.Token

        member private this.evaluateJob job =
            let desire = decideJob(job)
            let (_,maxAction) = List.maxBy (fun (_,desire ) -> desire) this.decidedActions
            let maxCur = Math.Max(lastHighestDesire,maxAction)
            if maxCur < desire then
                JobDesiredEvent.Trigger(this, new UnaryValueEvent<IilAction>(buildJobAccept job))
                () 
        member private this.addDesiredAction (action,desire) =
            lock actionDeciderLock (fun () -> 
                let value = List.tryFind (fun (a,d) -> a = action) this.decidedActions
                if value.IsSome then
                    let (fa,fd) = value.Value
                    if desire > fd then
                        this.decidedActions <- (action,desire)::(List.filter (fun (a,d) -> a <> action) this.decidedActions)
                else
                    this.decidedActions <- (action,desire)::this.decidedActions
                )
            ()        

        interface IAgentLogic with
            member this.Close() = 
                stopLogic()
                ()
            member this.CurrentDecision = 
                let (bestAction,_) = lock actionDeciderLock (fun () -> List.maxBy snd this.decidedActions)
                buildIilAction bestAction

            member this.HandlePercepts(iilpercepts) = 
                if simEnded then
                    ()
                let ServerMessage = (parseIilPercepts iilpercepts)
                match ServerMessage with
                | AgentServerMsg msg ->
                    match msg with
                    | NewJobs jobs ->
                        lock knonwJobsLock (fun () -> this.KnownJobs <- jobs@this.KnownJobs)
                        this.asyncCalculationMany this.evaluateJob jobs stopDeciders.Token
                    | AcceptedJob id ->
                        this.CalculateAcceptedJob(id)  
                    | SharedPercepts percepts ->
                        lock awaitingPerceptsLock (fun () -> this.awaitingPercepts <- percepts@this.awaitingPercepts)
                        ()
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
                        this.asyncCalculationMany this.evaluateJob this.KnownJobs stopDeciders.Token
                        this.generateNewJobs()
                        let totalTime = deadline - actionTime
                        let forceDecision start totaltime =
                            async
                                {
                                    let awaitingDecision = ref true
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
            member this.JobCreated = JobCreatedEvent.Publish
            [<CLIEvent>]
            member this.JobDesired = JobDesiredEvent.Publish
            [<CLIEvent>]
            member this.EvaluationCompleted = EvaluationCompletedEvent.Publish
            [<CLIEvent>]
            member this.EvaluationStarted = EvaluationStartedEvent.Publish
            [<CLIEvent>]
            member this.ActionRequested = ActionRequestedEvent.Publish
            [<CLIEvent>]
            member this.SimulationEnded  = SimulationEndedEvent.Publish
    end