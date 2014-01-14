namespace IiLang
    module IiLangHandler =
        open JSLibrary.IiLang
        open JSLibrary.IiLang.Parameters
        open System.Xml.Serialization

        type Element = ParameterList of Element list
                     | Function      of (string * Element list)
                     | Numeral       of float
                     | Identifier    of string

        type Data = Percept of (string * Element)
                  | Action  of (string * Element)

        type PerceptCollection = Data list

        let rec buildIil (data : Element) : IilParameter = 
            let unpackParameters ls = List.map buildIil ls |> List.toSeq<IilParameter>
            match data with
            | ParameterList ls ->
                new IilParameterList (unpackParameters ls) 
                :> IilParameter
            | Function (name, ls) -> 
                new IilFunction (name, unpackParameters ls)
                :> IilParameter
            | Numeral num -> 
                new IilNumeral (num) 
                :> IilParameter
            | Identifier str -> 
                new IilIdentifier (str) 
                :> IilParameter

        let rec evalIil (iil : IilParameter) = 
            match iil with
            | :? Parameters.IilParameterList as paramlist 
                -> ParameterList (List.ofSeq paramlist.Parameters |> List.map evalIil)
            | :? Parameters.IilFunction as func           
                -> Function (func.Name, List.ofSeq func.Parameters |> List.map evalIil)
            | :? Parameters.IilIdentifier as id           
                -> Identifier id.Value
            | :? Parameters.IilNumeral as num             
                -> Numeral num.Value
            | _ -> failwith "the object %O is not a recognized iilang element (IilElement)"

   