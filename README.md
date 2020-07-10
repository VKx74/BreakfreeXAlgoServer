# The BFT AlgoServer

Developed with #C, #Websocket and #EntityFramework ORM.

Linux deploy:

####Build project:

sudo /bin/bash -c "pushd ./src/BFT.AlgoService.API && popd && pushd ./cli-linux && ./build-bits-linux.sh ../src"


####Build image:

sudo docker build -t algoserver -t registry.gitlab.com/breakfree-trading-platform/algoserver .

####Run:

sudo docker run -it -p 4000:80 algoserver .
