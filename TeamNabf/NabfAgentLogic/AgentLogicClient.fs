namespace NabfAgentLogic
    open NabfAgentLogic.AgentInterfaces;
    open JSLibrary.Data.GenericEvents;
    open JSLibrary.IiLang;
    open JSLibrary.IiLang.DataContainers;
    open System;
    open NabfAgentLogic.AgentLogic;

    type AgentLogicClient() = 
        
        [<DefaultValue>] val mutable reEvalNeeded : bool
        [<DefaultValue>] val mutable BeliefData : State
        [<DefaultValue>] val mutable AvailableJobs : List<Job*Desirability>
        [<DefaultValue>] val mutable decision : Action
        let decisionMonitor = new Object()

        let JobCreatedEvent = new Event<UnaryValueHandler<IilAction>, UnaryValueEvent<IilAction>>()
        let JobLoadedEvent = new Event<UnaryValueHandler<JobID>, UnaryValueEvent<JobID>>()
        let PerceptsLoadedEvent = new Event<EventHandler, EventArgs>()
        let EvaluationCompletedEvent = new Event<EventHandler, EventArgs>()

        interface IAgentLogic with
            member this.EvaluateJob(id) = (null,false)
            member this.HandlePercepts(percepts) = 
                this.BeliefData <- updateState this.BeliefData (parseIilPercepts percepts)
                this.reEvalNeeded <- true
                ()


            member this.GetJobs = [(1,1)]
            member this.Start = 
                while true do
                    if this.reEvalNeeded then
                        let action = chooseAction this.BeliefData
                        lock decisionMonitor (fun () -> this.decision <- action)
                        ()
                   
                
                    

            member this.CurrentDecision = 
                let value= ref null
                lock decisionMonitor (fun () -> value := buildIilAction this.decision)
                !value 


            [<CLIEvent>]
            member this.JobCreated = JobCreatedEvent.Publish
            [<CLIEvent>]
            member this.JobLoaded = JobLoadedEvent.Publish
            [<CLIEvent>]
            member this.PerceptsLoaded = PerceptsLoadedEvent.Publish
            [<CLIEvent>]
            member this.EvaluationCompleted = EvaluationCompletedEvent.Publish