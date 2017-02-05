FROM microsoft/dotnet

# git
RUN git --version

# npm
RUN apt-get update && \
    apt-get install -y npm && \
    apt-get install -y unzip && \
    apt-get install -y nodejs-legacy
RUN npm --version

# jshint
RUN npm install -g jshint
RUN jshint --version

# cli
WORKDIR /app/cli
RUN curl -SLO "https://github.com/Repometric/linterhub-cli/releases/download/0.3.3/linterhub-cli-debian.8-x64-0.3.3.zip" && \
    unzip "linterhub-cli-debian.8-x64-0.3.3.zip" -d /app/cli && \
    rm "linterhub-cli-debian.8-x64-0.3.3.zip"

# app
COPY . /app
WORKDIR /app/src
RUN dotnet --version
RUN ["dotnet", "restore"]
RUN ["dotnet", "build"]
EXPOSE 8181/tcp

# variables
ENV ASPNETCORE_URLS http://*:8181
ENV GitPath /usr/bin/git
ENV TempPath temp
ENV GitHubToken TOKEN
ENV GitHubName Repometric
ENV GitHubUrl http://repometric.com

ENV CliPath /app/cli/bin/debian.8-x64/cli

# entrypoint
ENTRYPOINT ["dotnet", "run"]