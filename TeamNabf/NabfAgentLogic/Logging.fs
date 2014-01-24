module Logging

open XmasEngineExtensions.LoggerExtension
open System.IO
open JSLibrary

let sw = new StreamWriter(System.Console.OpenStandardOutput());
sw.AutoFlush <- true;
System.Console.SetOut(sw);

let logger = new Logger(sw, DebugLevel.Critical)

let logLock = new ExtendedConsole ()

let log debugLevel str = lock logLock (fun () -> logger.LogStringWithTimeStamp (str, debugLevel))

let logError = log DebugLevel.Error
let logCritical = log DebugLevel.Critical
let logInfo = DebugLevel.Info
let logAll = DebugLevel.All