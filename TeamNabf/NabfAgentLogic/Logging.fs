namespace NabfAgentLogic
module Logging =
    open JSLibrary.Logging
    open System
    open System.IO

    let debugLevel = DebugLevel.Error

    let sw = new StreamWriter (System.Console.OpenStandardOutput ());
    sw.AutoFlush <- true;
    System.Console.SetOut (sw);

    let logger = new Logger (sw, debugLevel);
    let logLock = new Object ();

    let log debugLevel str = lock logLock (fun () -> logger.LogStringWithTimeStamp (str, debugLevel))

    let logCritical = log DebugLevel.Critical
    let logError = log DebugLevel.Error
    let logInfo = log DebugLevel.Info
    let logAll = log DebugLevel.All


    