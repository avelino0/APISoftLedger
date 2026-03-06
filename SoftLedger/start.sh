#!/bin/bash
set -e

echo "Restoring dependencies..."
dotnet restore

echo "Publishing SoftLedger..."
dotnet publish -c Release -o out

echo "Starting SoftLedger..."
dotnet out/SoftLedger.dll