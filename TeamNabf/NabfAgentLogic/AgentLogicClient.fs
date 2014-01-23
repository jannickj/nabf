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

    type public AgentLogicClient(name,decisionTreeGenerator) = class 
        
        
        [<DefaultValue>] val mutable private BeliefData : State
        [<DefaultValue>] val mutable private KnownJobs : List<Job>
        //[<DefaultValue>] val mutable private DesiredJobs : List<Job*Desirability>
        //[<DefaultValue>] val mutable private possibleActions : List<Action>
        //[<DefaultValue>] val mutable private undecidedDecisions : (DecisionRank*Decision<(State -> (bool*Option<Action>))>) list
        [<DefaultValue>] val mutable private awaitingPercepts : List<Percept>
        [<DefaultValue>] val mutable private simulationID : SimulationID

        let agentname = name
        let decisionTree = decisionTreeGenerator()
        let mutable simEnded = false
        let mutable runningCalc = 0
        let mutable decisionId = 0
        //let mutable decidedAction = false
        let mutable decidedAction = (Int32.MaxValue,Action.Recharge)
        //let mutable lastHighestDesire:Desirability = 0
        

        new(name) = AgentLogicClient(name,fun () -> generateDecisionTree)

        //Parallel helpers
        let mutable stopDeciders = new CancellationTokenSource()

        let stateLock = new Object()
        let decisionLock = new Object()
        let runningCalcLock = new Object()
        let knownJobsLock = new Object()
        let awaitingPerceptsLock = new Object()
        


        let SendAgentServerEvent = new Event<UnaryValueHandler<IilAction>, UnaryValueEvent<IilAction>>()
        let SendMarsServerEvent = new Event<UnaryValueHandler<IilAction>, UnaryValueEvent<IilAction>>()
        let EvaluationCompletedEvent = new Event<EventHandler, EventArgs>()
        let EvaluationStartedEvent = new Event<EventHandler, EventArgs>()
        let SimulationEndedEvent = new Event<EventHandler, EventArgs>()

        member public this.DecidedAction = decidedAction

        member private this.asyncCalculation name stopToken func = this.asyncCalculationAF name stopToken  (async {  func() })
                                                                            
        member private this.asyncCalculationAF name (stopToken:CancellationToken) func =
            if stopToken.IsCancellationRequested then
                    lock runningCalcLock (fun () -> 
                        printf "%s was cancelled before execution\n" name)
            else
                let changeCals value = 
                    lock runningCalcLock (fun () -> 
                    //Console.WriteLine(name+":"+runningCalc.ToString()+" change: "+value.ToString())
                    if runningCalc = 0 then
                        EvaluationStartedEvent.Trigger(this, new EventArgs())
                    runningCalc <- runningCalc + value
                    if runningCalc = 0 then
                        EvaluationCompletedEvent.Trigger(this, new EventArgs())
                    )
                let AsyncF f =
                    async
                        {
                            Async.StartImmediate(f,stopToken)
                            changeCals(-1)
                        }
                changeCals(1)

                Async.Start (AsyncF func,stopToken)
                () 

        member private this.EvaluateDecision    (rankCur:DecisionRank) 
                                                (stopToken:CancellationToken) 
                                                (stopSource:CancellationTokenSource) 
                                                (s:State,d:Decision<(State -> (bool*Option<Action>))>) =
            match d with
            | Choice f -> 
                
                this.asyncCalculationAF ("Calc Choice") stopToken (
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
                this.asyncCalculationAF ("Calc conditon") stopToken (
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
            

        member private this.asyncCalculationMany name func values stopToken =
            List.iter (fun v -> ignore <| (this.asyncCalculationAF name stopToken (async { ignore <| func v }))) values

        member private this.ReEvaluate percepts =
            stopDeciders.Cancel()
            stopDeciders <- new CancellationTokenSource()
            let sharedPercepts = lock awaitingPerceptsLock (fun () -> 
                                        let percepts = this.awaitingPercepts
                                        this.awaitingPercepts <- []
                                        percepts)
            let oldstate = (lock stateLock (fun () -> this.BeliefData))
            let generateSharedPercepts() =
                let sharedP = (selectSharedPercepts oldstate percepts)
                if not ( List.isEmpty sharedPercepts) then
                    let sharedPs = ShareKnowledge sharedPercepts
                    let action = buildIilSendMessage (this.simulationID,sharedPs)
                    SendAgentServerEvent.Trigger(this, new UnaryValueEvent<IilAction>(action))
            this.asyncCalculation "generating shared percept" stopDeciders.Token generateSharedPercepts
            let newstate = lock stateLock (fun () -> this.BeliefData <- updateState this.BeliefData (percepts@sharedPercepts)
                                                     this.BeliefData)
          
            
            runningCalc <- 0
            lock decisionLock ( fun () ->
                                    decidedAction <- (Int32.MaxValue,Action.Recharge)
                                    decisionId <- (decisionId + 1)
                                )
            let start = DateTime.Now
            this.EvaluteState()
            let dif = DateTime.Now - start
            Console.WriteLine("Evaluate state: "+dif.TotalMilliseconds.ToString());

        let stopLogic () =
            stopDeciders.Cancel()
            simEnded <- true
        
        member private this.CalculateAcceptedJob id moveTo =
            let foundJob = List.tryFind (fun ((jid,_,_,_),_) -> jid = id) this.KnownJobs
            if foundJob.IsSome then
                let update = 
                    async
                        {
                            let ajob = foundJob.Value
                            lock stateLock (fun () ->   this.BeliefData <- updateStateWhenGivenJob this.BeliefData ajob moveTo)
                            this.asyncCalculation "Calc accept job" stopDeciders.Token (fun () -> this.EvaluteState())
                        }
                Async.StartImmediate update

            
        member private this.removeUselessJobs() =
            let knownjobs = lock knownJobsLock (fun () -> this.KnownJobs)
            let removeJobFunc job =
                let state = lock stateLock (fun () -> this.BeliefData)
                let decide = shouldRemoveJob state job
                let ((jobid,_,_,_),_) = job
                if decide then
                    let removejobAct = buildIilSendMessage(this.simulationID,(RemoveJob jobid.Value))
                    SendAgentServerEvent.Trigger(this, new UnaryValueEvent<IilAction>(removejobAct))
            this.asyncCalculationMany "remove job calc" removeJobFunc knownjobs stopDeciders.Token

        member private this.generateNewJobs() = 
            let jobTypes = Enum.GetValues(typeof<JobType>)
            let rec jobGenFunc jobtype state knownJobs =
                let jobopt = generateJob jobtype state knownJobs
                match jobopt with
                | Some job ->
                    let metaAction = 
                        match job with
                        | ((Some jobid,_,_,_),_) -> UpdateJob job
                        | ((None,_,_,_),_) -> CreateJob job
                    let createjobMsg = buildIilSendMessage (this.simulationID,metaAction)
                    SendAgentServerEvent.Trigger (this, new UnaryValueEvent<IilAction>(createjobMsg)) 
                    jobGenFunc jobtype state (job::knownJobs)                            
                | None -> ()

            let jobTypeList = (List.ofSeq (jobTypes.Cast<JobType>())).Tail
            let joblist = lock knownJobsLock (fun () -> this.KnownJobs)
            let knownjobs jobtype =List.filter (fun ((_, _, jt,_), _) -> jt = jobtype) joblist
            let stateData = lock stateLock (fun () -> this.BeliefData)
            //List.iter (fun jobType -> (this.asyncCalculation "job" stopDeciders.Token )) jobTypeList 
            this.asyncCalculationMany "calc generate job" 
                    (fun jobType -> ignore <| jobGenFunc jobType stateData (knownjobs jobType)) 
                    jobTypeList stopDeciders.Token
        member private this.EvaluateJob  job =
            let jobs = lock knownJobsLock (fun () -> this.KnownJobs)
            let eval job () = 
                let stateData = lock stateLock (fun () -> this.BeliefData)
                let (desire,wantJob) = decideJob stateData job
                if wantJob then
                    let ((jobid,_,_,_),_) = job
                    let sendmsg = buildIilSendMessage (this.simulationID,ApplyJob((jobid.Value),desire))
                    SendAgentServerEvent.Trigger (this, new UnaryValueEvent<IilAction>(sendmsg))  
            this.asyncCalculation "evaluate job" stopDeciders.Token (eval job) 

        member private this.updateJob job =
            lock knownJobsLock (fun () -> 
                                    let filteredJobs = List.filter (fun ((Some jid,_,_,_),_) ->  
                                                                match job with
                                                                | ((Some id,_,_,_),_) -> not (id = jid)
                                                                | _ -> true) this.KnownJobs 
                                     
                                    this.KnownJobs <- job::filteredJobs)
            
            this.EvaluateJob job


        interface IAgentLogic with
            member this.Close() = 
                stopLogic()
                ()

            member this.HandlePercepts(iilpercepts) = 
                if simEnded then
                    ()
                let ServerMessage = (parseIilPercepts iilpercepts)
                match ServerMessage with
                | AgentServerMessage msg ->
                    match msg with
                    | AddedOrChangedJob job ->
                        this.updateJob job
                    | AcceptedJob (id,vn) ->
                        this.CalculateAcceptedJob (Some id) vn
                    | SharedPercepts percepts ->
                        ignore <| lock awaitingPerceptsLock (fun () -> this.awaitingPercepts <- percepts@this.awaitingPercepts)
                |  MarsServerMessage msg ->
                    match msg with
                    | SimulationEnd _ -> 
                        SimulationEndedEvent.Trigger(this, new EventArgs())
                        stopLogic()
                        ()
                    | SimulationStart sData ->
                        this.KnownJobs <- []
                        this.awaitingPercepts <- []
                        this.simulationID <- sData.SimId
                        let subscribeAction = buildIilSendMessage (this.simulationID, SimulationSubscribe)
                        SendAgentServerEvent.Trigger(this, new UnaryValueEvent<IilAction>(subscribeAction))
                        this.BeliefData <- buildInitState (agentname,sData)
                    | ActionRequest ((deadline, actionTime, id), percepts) ->
                        
                        let left = new DateTime(int64(deadline)*int64(10000))
                        let timeleft = left-DateTime.Now ;
                        let a = int64(deadline)
                        Console.WriteLine("Action request started timeleft "+a.ToString())
                        let newRoundAct = buildIilSendMessage (this.simulationID, NewRound(id))
                        SendAgentServerEvent.Trigger(this, new UnaryValueEvent<IilAction>(newRoundAct))
                        let start = DateTime.Now
                        
                        this.ReEvaluate percepts

                        let dif = DateTime.Now - start
                        Console.WriteLine("Evaluate percepts: "+dif.TotalMilliseconds.ToString());

                        let startAsyncTime = DateTime.Now
                        let knownJobs = lock knownJobsLock (fun () -> this.KnownJobs)
                        List.iter this.EvaluateJob knownJobs
                        this.generateNewJobs()
                        let totalTime = deadline - actionTime
                        let forceDecision start totaltime =
                            async
                                {
                                    
                                    let! token = Async.CancellationToken
                                    
                                    let awaitingDecision = ref true
                                    //Thread.Sleep(800)
                                    while awaitingDecision.Value do 
                                        if token.IsCancellationRequested then
                                            awaitingDecision:=false
                                        else                                          
                                            //Console.WriteLine(runningCalc);
                                            
                                            let dif = DateTime.Now - startAsyncTime
                                            //Console.WriteLine("async load time: "+dif.TotalMilliseconds.ToString())
                                            let expired = (System.DateTime.Now.Ticks - start)/(int64(10000))
                                        
                                            let runningCalcs = lock runningCalcLock (fun () -> runningCalc)
                                            //Console.WriteLine(runningCalcs)

                                            if runningCalcs <= 0 || (expired+int64(800)) > int64(totaltime) then
                                                SendMarsServerEvent.Trigger(this,new UnaryValueEvent<IilAction>(buildIilAction (float id) (lock decisionLock (fun () -> snd decidedAction))))
                                                awaitingDecision:=false
                                            ignore <| Async.Sleep(100)
                                }
                        Async.Start ((forceDecision System.DateTime.Now.Ticks totalTime),stopDeciders.Token)
                        
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