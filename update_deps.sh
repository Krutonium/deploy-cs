#!/usr/bin/env bash
dotnet restore --packages ./deps -f
nix-shell -p nuget-to-nix --run "nuget-to-nix ./deps > deps.nix"
rm -rd ./deps
