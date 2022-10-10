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


Finally, run the application once, and it'll generate a `targets.json` file. You should open it up, and edit it. For example:
```
{
  "Devices": [
    {
      "Name": "uWebServer",
      "Ip": "192.168.0.10",
      "User": "krutonium",
      "Comment": "This is my Web Server. It hosts many things."
    },
    {
      "Name": "uGamingPC",
      "Ip": "192.168.0.40",
      "User": "krutonium",
      "Comment": "Gaming PC - For Gaming and software development"
    }
  ]
}
```

The IP can also be a hostname if you have reliable hostnames on your LAN. My Modem/Router combo doesn't. Could also technically be external.

This tool also assumes that the running account has PubKey Passwordless access to the target PC's.

:3 Enjoy!

