# �ϥΩx�誺 .NET ASP.NET Core �B��ɬM���@����¦�M��
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# �ƻs�w�sĶ�� .NET �{����e����
COPY ./bin/Release/net8.0/publish .

# �]�m�J�f�I
ENTRYPOINT ["dotnet", "MqttSignalRApi.dll"]