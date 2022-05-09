{ lib, buildDotnetModule, dotnetCorePackages }:

buildDotnetModule rec {
  pname = "deploy-cs";
  version = "0.1";

  src = ./.;

  projectFile = "./deploy-cs.sln";
  nugetDeps = ./deps.nix;
  dotnet-sdk = dotnetCorePackages.sdk_6_0;
  dotnet-runtime = dotnetCorePackages.sdk_6_0;
  dotnetFlags = [ "" ];
  executables = [ "deploy-cs" ];
}