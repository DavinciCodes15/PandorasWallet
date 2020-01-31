Const ForReading = 1, ForWriting = 2, ForAppending = 8

Dim argNum, argCount:argCount = Wscript.Arguments.Count
If (argCount < 1) Then
        Wscript.Echo "This script is executed by Pandora's Wallet" &_
                vbLf & " The agrgument must be the file to install " &_
                vbLf & " the second is the log folder."
        Wscript.Quit 1
End If

Set fso = CreateObject("Scripting.FileSystemObject")
set DataLog = fso
GFileName = Wscript.Arguments(0)

If fso.FileExists(GFileName) Then
    GPath =  fso.GetParentFolderName(fso.GetFile(GFileName))
    lExecuted = false
    if CreateLog then
        intAnswer = Msgbox("Do you want to Upgrade Pandora's Wallet?", vbYesNo, "Upgrade Pandora's Wallet")
        If intAnswer = vbYes Then
            DataLog.WriteLine "Calling command to install"
            DataLog.WriteLine GFileName
            Set objShell = CreateObject ("WScript.shell")
            objShell.run "msiexec /l*v! """+ fso.BuildPath(GPath,"UpgradeInstall.log") +""" /passive /i """ + GFileName + """" , 10, true
            Set objShell = Nothing
        End If
    end if
end if

function CreateLog
   Set fso = CreateObject("Scripting.FileSystemObject")
   Path = fso.BuildPath( GPath, "UpgradeScript.log" )
   On Error Resume Next
   if not fso.FileExists( Path ) then
     Set DataLog = fso.CreateTextFile( Path )
   else
     Set DataLog = fso.OpenTextFile( Path, ForAppending, True)
   end if
   if Err then 
      CreateLog = false
   else
      DataLog.WriteLine "*******************************************************"
      DataLog.WriteLine "Executed on " & Date & " at " & Time
      DataLog.WriteLine "*******************************************************"
      CreateLog = true
   end if
   On Error Goto 0
end function


