# cd into this file's directory
cd $(dirname $0)

# Activate the virtualenv
source env/bin/activate

# nodemon natively detects python files and runs them with the python interpreter
nodemon main.py --ignore env/