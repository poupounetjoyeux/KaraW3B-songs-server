FROM mcr.microsoft.com/dotnet/sdk:10.0

EXPOSE 7373

ARG UUID=1001
ARG GUID=1001
RUN groupadd -g $GUID -o KaraW3B
RUN useradd -m -u $UUID -g $GUID -o -s /bin/bash KaraW3B

COPY src/ /src/
WORKDIR /src
RUN dotnet build --configuration Release

RUN mkdir /app
WORKDIR /app
RUN mv /src/bin/Release/* ./
RUN chown -R KaraW3B:KaraW3B /app

RUN rm -r /src

USER $UUID:$GUID
ENTRYPOINT ["dotnet", "KaraW3B.Server.Host.dll"]
