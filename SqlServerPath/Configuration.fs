namespace SqlServerPath

module Configuration =
    type PathConfig = {drive: string; path: string}
        
    open Microsoft.Win32
    open System
    open System.IO

    let private (<|>) a b =
        match a with
        | Some(_) -> a
        | None -> b
    
    let private RegValue element =
        match element with
        | null -> None
        | _ -> Some(element.ToString())

    let private _2017 = @"SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL14.SQL\Setup";
    let private _2016 = @"SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.MSSQLSERVER\Setup";
    let private _2014 = @"SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL12.MSSQLSERVER\Setup";

    let private GetSqlDataRootValue bitness setupPath =
        try
            let regKeyHklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, bitness)
            let regSubKey = regKeyHklm.OpenSubKey(setupPath)

            RegValue(regSubKey.GetValue("SQLDataRoot"))
        with _ -> None

    let private GetSqlPath =
            GetSqlDataRootValue RegistryView.Registry32 _2017
        <|> GetSqlDataRootValue RegistryView.Registry64 _2017
        <|> GetSqlDataRootValue RegistryView.Registry32 _2016
        <|> GetSqlDataRootValue RegistryView.Registry64 _2016
        <|> GetSqlDataRootValue RegistryView.Registry32 _2014
        <|> GetSqlDataRootValue RegistryView.Registry64 _2014
    
    let GetSqlDataRoot =
        match GetSqlPath with
        | Some(path) -> { drive= path.Substring(0, 1)
                        ; path= Path.Combine(path.Substring(3), "DATA")
                        }
        | None -> raise (new InvalidOperationException("Cannot find installed SQL Server version"))
