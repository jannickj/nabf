namespace NabfAgentLogic
    open NabfAgentLogic.AgentInterfaces;
    open JSLibrary.Data.GenericEvents;
    open JSLibrary.IiLang;
    open JSLibrary.IiLang.DataContainers;
    open System;
    open NabfAgentLogic.AgentLogic;
    open System.Threading;
    open System.Linq;

    type public AgentLogicClient() = 
        
        
        [<DefaultValue>] val mutable private BeliefData : State
        [<DefaultValue>] val mutable private AvailableJobs : List<Job*Desirability>
        [<DefaultValue>] val mutable private localActions : Action List
        [<DefaultValue>] val mutable private decidedActions : List<Action*Desirability>
        
        let mutable simEnded = false

        //Parallel helpers
        let stopDeciders = new CancellationTokenSource()
        let actionDeciderLock = new Object()



        let JobCreatedEvent = new Event<UnaryValueHandler<IilAction>, UnaryValueEvent<IilAction>>()
        let EvaluationCompletedEvent = new Event<EventHandler, EventArgs>()
        let SimulationEndedEvent = new Event<EventHandler, EventArgs>()

        member private this.ReEvaluate() =
            stopDeciders.Cancel()
            this.localActions <- generateActions this.BeliefData
            let actionDecider state action =
                async
                    {
                        let desire = actionDesirability state action
                        lock actionDeciderLock (fun () -> this.decidedActions <- (action,desire) :: this.decidedActions)
                    }
                        
            List.iter (fun action -> Async.Start ((actionDecider this.BeliefData action),stopDeciders.Token)) this.localActions
                        
            ()

        interface IAgentLogic with
            member this.Close() = ()

            member this.HandlePercepts(iilpercepts) = 
                
                let data = (parseIilPercepts iilpercepts)
                match data with
                | JobCollection jobs -> ()
                    
                | SimulationEnd -> 
                    SimulationEndedEvent.Trigger(this, new EventArgs())
                    ()              
                | PerceptCollection percepts ->
                    this.BeliefData <- updateState this.BeliefData percepts
                    this.ReEvaluate()
                    let newJobs = generateJobs this.BeliefData (List.map (fun (job, _) -> job) this.AvailableJobs )
                    let jobTypes = Enum.GetValues(typeof<JobType>)
                    let rec jobGenFunc jobtype state knownJobs =
                        let jobopt = generateJob jobtype state knownJobs
                        match jobopt with
                        | Some job ->
                            JobCreatedEvent.Trigger (this, new UnaryValueEvent<IilAction>(buildJob job)) 
                            jobGenFunc jobtype state (job::knownJobs)                            
                        | None -> ()

                    let asyncJobGen jobtype state knownJobs=
                        async 
                            {
                               jobGenFunc jobtype state knownJobs
                               ()
                            }
                    
                    let jobTypeList = List.ofSeq (jobTypes.Cast<JobType>())
                    let knownjobs jobtype =List.filter (fun ((_, _, jt), _) -> jt = jobtype) (List.map (fun (job,_) -> job) this.AvailableJobs)
                    List.iter (fun jobType -> Async.Start ((asyncJobGen jobType this.BeliefData (knownjobs jobType) ),stopDeciders.Token)) jobTypeList 
                    ()


            member this.GetJobs = [(1,1)]
           
            member this.CurrentDecision = 
                let (bestAction,_) = lock actionDeciderLock (fun () -> List.maxBy snd this.decidedActions)
                buildIilAction bestAction


            [<CLIEvent>]
            member this.JobCreated = JobCreatedEvent.Publish
            [<CLIEvent>]
            member this.EvaluationCompleted = EvaluationCompletedEvent.Publish
            [<CLIEvent>]
            member this.SimulationEnded  = SimulationEndedEvent.Publish