@echo off
call "%VS90COMNTOOLS%\vsvars32.bat" %1
SET PATH=%PATH%;"%PROGRAMFILES%\Microsoft Team Foundation Server Power Tools";"%PROGRAMFILES%\Microsoft Team Foundation Server 2008 Power Tools";"%PROGRAMFILES(X86)%\Microsoft Team Foundation Server 2008 Power Tools"
call %2



