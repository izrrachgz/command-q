# Fyi-search
Stand Alone Search Service
C#
.NetCore 7


## Installation and Requirements

fyi-search requires [Docker](https://www.docker.com/products/docker-desktop/) v4.23+ to run.

For windows environment :

-Windows 10 64-bit: Home or Pro 21H2 (build 19044) or higher, or Enterprise or Education 21H2 (build 19044) or higher.

>Enable wsl (windows subsystem linux) v1.1.3.0+.

command via powershell

```sh
wsl --install
```

visit for more information on how to enable [wsl](https://learn.microsoft.com/en-us/windows/wsl/install) 


## Docker

Fyi-search is very easy to install and deploy in a Docker container.

> Note: if you are using visual studio, it links to docker desktop and this process can be automated for you to just click on docker run button.

By default, the Docker will expose port 8080, so change this within the
Dockerfile if necessary. When ready, simply use the Dockerfile to
build the image.

```sh
cd Fyi-search
docker build -t <youruser>/fyi-search:${version} .
```

This will create the Fyi-search image and pull in the necessary dependencies.
Be sure to swap out `${version}` with the actual
version of Fyi-search.

Once done, run the Docker image and map the port to whatever you wish on
your host. In this example, we simply map port 8000 of the host to
port 8080 of the Docker (or whatever port was exposed in the Dockerfile):
