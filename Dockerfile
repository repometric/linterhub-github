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
ENV GitHubToken TEST
ENV GitHubName Test-Core-Integration
ENV GitHubUrl http://google.com

# entrypoint
ENTRYPOINT ["dotnet", "run"]