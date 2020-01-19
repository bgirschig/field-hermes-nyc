#!/bin/bash

cd $(dirname $0)
ROOT_DIR=$(pwd)

source env/bin/activate
pip install -r requirements.txt

pyinstaller \
  main.py \
  --onefile \
  --name "field detector service" \
  --distpath dist

# write to lock after the build so that if an error occurred we don't overwrite
# the previously-working config
echo "\
# This file is generated automatically (using pip freeze).
# It can be used to create deterministic builds.
#
# Do not edit manually
" > requirements-lock.txt
pip freeze >> requirements-lock.txt
