To set up your build environment

1. Create a folder for yourself in \users\codeplex_username
2. Copy \users\template\template.cmd into your users folder
3. Edit your template.cmd to point to your enlistment directory.  This will set up your build paths.  
   You can have as many template files as you wish.

3. Make a shortcut to buildEnvironment.cmd with 2 parameters - your platform (X86 or X64) and your template file

For Example: Shortcut Path: %COMSPEC% /k "C:\codeplex\Tools\buildEnvironment.cmd X64 C:\Codeplex\users\vsabella\vsabella1.cmd"
Start In Directory:         C:\codeplex


Thats It! Enjoy