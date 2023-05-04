# Deploy-CS

So, you're tired of colmena, deploy-rs and the rest, you want something that doesn't require you to fuck around with your flake too much, and you want something that works.

Well have I got a solution for you!

Bootstrapping this package boils down to:

## Installation

`nix shell github:Krutonium/deploy-cs`
And then type `deploy` in the folder with your `flake.nix` file. It'll create a `config.json` for you to edit.
Inside of this file, you will want to configure each host.

And then add it to your system's `flake.nix` like this:

```nix
inputs = {
  deploy-cs.url = "github:Krutonium/deploy-cs";
  deploy-cs.inputs.nixpkgs.follows = "nixpkgs";
}
outputs = { ..., ..., deploy-cs }:
```
And then in your machine specific definitions:

```nix
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

```nix
environment.systemPackages = [
  pkgs.deploy-cs
];
```

## Configuration

Inside `config.json` You'll see a default configuration with mostly nonsense values. My config looks like this:
```json
{
  "MaxParallel": 5,
  "Update_Flake": true,
  "_machines": [
    {
      "Name": "uWebServer",
      "User": "root",
      "Ip": "10.0.0.1",
      "Verb": "switch",
      "Comment": "Web Server"
    },
    {
      "Name": "uGamingPC",
      "User": "root",
      "Ip": "10.0.0.2",
      "Verb": "switch",
      "Comment": "Gaming PC"
    },
    {
      "Name": "uMsiLaptop",
      "User": "root",
      "Ip": "10.0.0.4",
      "Verb": "switch",
      "Comment": "MSI Laptop"
    },
    {
      "Name": "uHPLaptop",
      "User": "root",
      "Ip": "10.0.0.5",
      "Verb": "switch",
      "Comment": "HP Laptop"
    },
    {
      "Name": "uMacBookPro",
      "User": "root",
      "Ip": "10.0.0.6",
      "Verb": "switch",
      "Comment": "MacBookPro3,1"
    }
  ]
}

```

The `MaxParallel` value is the maximum number of machines to run at once. This is useful if you have a lot of machines and don't want to overload your network. It limits two things specifically:
  - The Number of computers to ping at the same time (to check if they're online and ready)
  - The number of computers to run `nixos-rebuild switch` on at the same time.
One thing of note is that this is just the Maximum degrees of parallelism. It can and likely will be less, based on how many machines are online and ready at the same time, and the throughput of your CPU and network.


Building each derivation is done one at a time, because Nix itself strugges with doing this without downloading the same package multiple times at the same time. Once it's built, it can push to however many machines you want at the same time.
On the upside of this, if a package was already built for another system, it won't be built again, so if your fleet of computers use similar configurations, it'll be fast after the first couple of them.


The `Update_Flake` value is a simple boolean for if you want it to automatically update your flake. This is useful if you want to keep it up to date.

The `_machines` array is where you define your machines. Each machine has the following properties:
  - `Name`: The name of the machine. This is used for logging and for the `nixos-rebuild switch --target-host` argument.
  - `User`: The user to SSH into the machine as. This is usually root, but can be any user that has sudo access __and__ the ability to switch without needing a password.
  - `Ip`: The IP of the machine.
  - `Verb`: The verb to use. This can be `switch`, `boot` or `test`. `switch` is the default, and is what you want to use most of the time. `boot` is for when you want to boot a machine into a specific configuration, but not switch to it. `test` is for when you want to test a configuration, but not switch to it.
  - `Comment`: A comment for the machine. This is used for your own sake, think of it as a note you can leave yourself.
  
The IP can also be a hostname if you have reliable hostnames on your LAN. My Modem/Router combo doesn't. Could also technically be external.

This tool requires that the account your SSHing into has passwordless sudo access, and the ability to switch without a password. If you don't have this, you can add it by adding the following to your `configuration.nix`:
```nix
  users.users.<username>.extraGroups = [ "wheel" ];
  security.sudo.wheelNeedsPassword = false;
```
Or just `ssh` into the `root` account (! Bad Practice !).

If you happen to know how to set it up so that Nix itself doesn't need sudo access, please let me know. I'd love to make this tool not require sudo access.

:3 Enjoy!

