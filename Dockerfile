FROM eosio/eos-dev:latest

WORKDIR /workdir

# copy files
RUN mkdir /home/cleos-net
RUN mkdir /home/cleos-net/agent
RUN mkdir /home/cleos-net/wallet
RUN mkdir /home/cleos-net/contracts

COPY ./Andoromeda.CleosNet.Agent /home/cleos-net/agent

# Install eosio.cdt
RUN wget https://github.com/eosio/eosio.cdt/releases/download/v1.4.1/eosio.cdt-1.4.1.x86_64.deb
RUN sudo apt install ./eosio.cdt-1.4.1.x86_64.deb

# Install .NET Core & Node.js
RUN wget -O libicu55.deb http://archive.ubuntu.com/ubuntu/pool/main/i/icu/libicu55_55.1-7ubuntu0.4_amd64.deb
RUN dpkg -i libicu55.deb
RUN wget -O packages-microsoft-prod.deb -q https://packages.microsoft.com/config/ubuntu/16.04/packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN curl -sL https://deb.nodesource.com/setup_10.x | sudo -E bash -
RUN apt-get update
RUN apt-get install dotnet-sdk-2.1 -y
RUN sudo apt-get install nodejs -y
RUN npm install yarn -g

WORKDIR /home/cleos-net/agent
