:: KeePass.exe --plgx-create C:\YourPluginDir --plgx-prereq-kp:2.09 --plgx-prereq-net:3.5
::

del /s /q %USERPROFILE%\AppData\Roaming\KeePass\PluginCache\*

del /q RSACertKeyProvider.plgx
rmdir /q /s %cd%\RSACertKeyProvider\obj
rmdir /q /s %cd%\RSACertKeyProvider\bin

SET kp="C:\Program Files (x86)\KeePass Password Safe 2\KeePass.exe"
%kp% --plgx-create %cd%\RSACertKeyProvider --plgx-prereq-net:3.5

pause