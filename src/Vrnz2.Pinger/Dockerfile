FROM mcr.microsoft.com/dotnet/core/aspnet:3.1

EXPOSE 80
EXPOSE 443

COPY bin/Release/netcoreapp3.1/publish/ App/
WORKDIR /App
ENTRYPOINT ["dotnet", "Vrnz2.Pinger.dll"]