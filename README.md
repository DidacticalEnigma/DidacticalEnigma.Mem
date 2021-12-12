Didactical Enigma.Mem
=================

Simple translation memory server

Currently it assumes the source language is Japanese and target language is English, though if needed, it could be generalized to other languages.

A single project contains many translation units, each one has source text, target text, translation notes (optionally), and may have an associated context with it (by it having a same correlation id). Context stores a piece of textual or binary data, or both.

Each translation unit has a correlation id, which is an unique identifier for a given translation in a hierarchy, with each component of correlation id being separated by a slash (`/`).

A project's owner can set it to be publically readable, in which case you can search for translations and contexts in a given project without needing to log in. A project's owner can also invite other people to the project, which allows them to be able to update translations.


Installation
------------

(assumes Debian 11 on the target server)

The provided installation script sets up the application running behind Nginx reverse proxy, with HTTPS provided by Let's Encrypt's Certbot, connecting to the local PostgreSQL instance. The script was tested with a clean Debian install, but should work properly even in case if Nginx, Certbot, or PostgreSQL are already installed.

From the main directory, run

```
dotnet ef migrations script -o Installation/initial-migration.sql -p DidacticalEnigma.Mem/DidacticalEnigma.Mem.csproj
dotnet publish -c Release -r linux-x64 --self-contained false -o Installation/DidacticalEnigma.Mem
```

Edit the `Installation/debian.sh` file and fill in the parameters.

Copy the `Installation` folder to the target server, on the target server, change directory to the newly copied `Installation` one, and run 

```
sudo ./debian.sh
```

Configuration
-------------

`DatabaseConfiguration:ConnectionString` describes the [connection string](https://www.npgsql.org/doc/connection-string-parameters.html) to be used to connect to a PostgreSQL database.

`MeCabConfiguration:PathToDictionary` must be directed to the directory where [MeCab's IPADIC dictionary](https://github.com/DidacticalEnigma/DidacticalEnigma-Data/tree/master/mecab/ipadic) is located.