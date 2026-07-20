[![Version](https://img.shields.io/github/v/release/KaraW3B/servers?sort=semver&display_name=tag&style=for-the-badge&label=version)](https://github.com/KaraW3B/servers/releases)
[![License](https://img.shields.io/github/license/KaraW3B/servers?style=for-the-badge)](https://github.com/KaraW3B/servers?tab=MIT-1-ov-file)
<p align="center" dir="auto">
  <img src="https://github.com/KaraW3B/resources/blob/49e3a6cfac800ed1ca2b6af4f19e7355d640d8f6/logo.png" style="height:300px;width:auto;" alt="KaraW3B Logo">
</p>

# 🎶 KaraW3B Songs Server

The KaraW3B songs server is the hearth of the project and will serves all your songs collections through an API

## 📦 Packages

KaraW3B NuGets aims to simplify integration of KaraW3B songs server in thirdparty clients

| Name                         | Content                                                     |
|------------------------------|-------------------------------------------------------------|
| KaraW3B.Server.Songs.Models  | Contains models used by the songs server API                |
| KaraW3B.SDK.Songs.Client     | Contains connectors to easily call KaraW3B songs server API |

## 🐋 How to run the container ?

Releases of the songs server build a Docker image on the GitHub Docker Repository (ghcr.io) 

To start a server instance with the latest version just runs the following command:
```
 docker run -d --name "KaraW3B-songs-server" \
	-p 7373:7373 \
	-v /path/to/my/songs:/songs \
	-v /path/to/database:/app/data \
	ghcr.io/karaw3b/songs-server:latest
```

> If you need to use a different port on your host for example 1234 just replace '-p 7373:7373' with '-p 1234:7373'

## 📖 How to use it ?

### Swagger
Once you launched the server, you can find a Swagger documentation of the API at http://my-server:7373/api/swagger

### Create the library
You can then create a library by calling the **PUT** endpoint method on /api/libraries
> If you mapped volumes like in the previous command then the path in the body should be **/songs**

### Launch library analyze
After the library creation, let's call the **POST** endpoint method on /api/libraries to start the analyze

## 🛠️ How to build it ?
If you want to contribute (or launch it on a different plateform than Docker) you can build the project form sources

### Prerequisites
- A GitHub PAT token with **packages:read** access
- The .NET 10 SDK installed on your machine

in a console type the following command to register your github token for installing NuGet packages:
```
dotnet nuget add source https://nuget.pkg.github.com/KaraW3B/index.json --name "KaraW3B-github" --username {YOUR_GITHUB_USER} --password {YOUR_GITHUB_PAT_TOKEN}
```

### Build
Then run a build in VisualStudio or from the **src** directory by using ```dotnet build```
