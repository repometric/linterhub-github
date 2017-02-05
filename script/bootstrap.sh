#!/bin/sh

cd src && \
echo "==> .NET version…" && \
dotnet --version && \
echo "==> Restoring .NET dependencies…" && \
dotnet restore && \
cd ..