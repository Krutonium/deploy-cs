{
  description = "deploy-cs - Deploy in Style!";
  inputs.nixpkgs.url = "github:NixOS/nixpkgs/nixos-22.11";
    outputs = { self, nixpkgs }: {
      defaultPackage.x86_64-linux =
        with import nixpkgs { system = "x86_64-linux"; };
        pkgs.callPackage ./package.nix {};
    };
}
