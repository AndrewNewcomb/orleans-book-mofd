#!/bin/bash
echo Running in $SHELL
pwd

cd ./OrleansBook.GrainInterfaces
dotnet build

cd ../OrleansBook.GrainClasses
dotnet build

cd ../OrleansBook.Host
dotnet build

cd ../OrleansBook.Client
dotnet build

cd ../OrleansBook.WebApi
dotnet build

cd ..