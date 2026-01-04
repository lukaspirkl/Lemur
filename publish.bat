del /s /f /q publish
rmdir /s /q publish

dotnet publish VentureUI/VentureUI.csproj -c Release -r win-x64 -p:PublishAot=true --output publish/win 

@pause
