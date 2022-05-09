# Deploy-CS

So, you're tired of colmena, deploy-rs and the rest, you want somthing in C# for some reason, or some other reason. How do you use this?

WELL I'M GLAD YOU ASKED.

Bootstrapping this package boils down to clone, `dotnet run` then edit the config it makes.

And then adding it to your system's `flake.nix` like this:

```
inputs = {
  ...
  ...
  deploy-cs.url = "github:Krutonium/deploy-cs";
  deploy-cs.inputs.nixpkgs.follows = "nixpkgs";
}
outputs = { ..., ..., deploy-cs }:
```
And then in your machine specific definitions:

```
modules = [
   ... other stuff...
   ({ pkgs, ... }: {
     nixpkgs.overlays = [
       (self: super: {
          deploy-cs = deploy-cs.defaultPackage.x86_64-linux;
       })
     ];
   })
```

And finally

```
environment.systemPackages = [
  pkgs.deploy-cs
];
```

:3 Enjoy!

