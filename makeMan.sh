/usr/bin/env bash
nix-shell -p pandoc --run 'pandoc --standalone --to man "README.md" --output deploy.1'
