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


    type public AgentLogicClient() = 
        
        
        [<DefaultValue>] val mutable private BeliefData : State
        [<DefaultValue>] val mutable private KnownJobs : List<Job>
        //[<DefaultValue>] val mutable private DesiredJobs : List<Job*Desirability>
        [<DefaultValue>] val mutable private possibleActions : List<Action>
        [<DefaultValue>] val mutable private decidedActions : List<Action*Desirability>
        [<DefaultValue>] val mutable private awaitingPercepts : List<Percept>
        
        let mutable simEnded = false
        let mutable runningCalc = 0
        let mutable lastHighestDesire:Desirability = 0
        

        //Parallel helpers
        let stopDeciders = new CancellationTokenSource()
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

        member private this.asyncCalculation func stopToken =
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
                        f()
                        do! Async.Sleep(1)
                        changeCals(-1)
                        ()
                    }
            
            changeCals(1)
            Async.Start ((asyncF func), stopToken)
            () 

        member private this.asyncCalculationMany func values stopToken =
            List.iter (fun v -> (this.asyncCalculation (fun () -> func v) stopToken)) values

        member private this.ReEvaluate percepts =
            stopDeciders.Cancel()
            let sharedPercepts = lock awaitingPerceptsLock (fun () -> 
                                        let percepts = this.awaitingPercepts
                                        this.awaitingPercepts <- []
                                        percepts)
            this.BeliefData <- updateState this.BeliefData (percepts@sharedPercepts)
            runningCalc <- 0
            lastHighestDesire <- List.max (List.map (fun (_,desire) -> desire) this.decidedActions)
            this.decidedActions <- []
            this.possibleActions <- generateActions this.BeliefData
            let actionDecider state action =
                let desire = 10//actionDesirability state action
                this.addDesiredAction (action,desire)
                        
            //List.iter (fun action -> Async.Start ((),stopDeciders.Token)) localActions       
            this.asyncCalculationMany (fun action -> actionDecider this.BeliefData action) this.possibleActions stopDeciders.Token
            ()

        let stopLogic () =
            stopDeciders.Cancel()
            simEnded <- true
        
        member private this.CalculateAcceptedJob id =
            let foundJob = List.tryFind (fun ((jid,_,_),_) -> jid = id) this.KnownJobs
            if foundJob.IsNone then
                ()
            else
                let acceptedJob = foundJob.Value
                let full = lock actionDeciderLock (fun () ->
                        let contains elem = List.exists (fun (e, _) -> e = elem)
                        let unique l l' = List.filter (fun elem -> not <| (contains elem l')) l
                        let u = List.map (fun elem -> (elem,0)) (unique this.possibleActions this.decidedActions)
                        this.decidedActions@u)

                let reEvalActions (action, desire) =
                    let newDesire = 10//actionDesirabilityBasedOnJob this.BeliefData (action,desire) acceptedJob
                    this.addDesiredAction(action,newDesire)
                    ()
                this.asyncCalculationMany reEvalActions full stopDeciders.Token 

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
            this.asyncCalculationMany (fun jobType -> jobGenFunc jobType this.BeliefData (knownjobs jobType) ) jobTypeList stopDeciders.Token

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